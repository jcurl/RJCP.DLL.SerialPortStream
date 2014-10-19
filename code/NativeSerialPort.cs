// $URL$
// $Id$

// Copyright © Jason Curl 2012-2014
// See http://serialportstream.codeplex.com for license details (MS-PL License)

namespace RJCP.IO.Ports
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;
    using System.Collections.Specialized;
    using RJCP.Datastructures;

    public partial class SerialPortStream
    {
        private sealed partial class NativeSerialPort : IDisposable
        {
            private string m_Port;
            private SafeFileHandle m_ComPortHandle;
            private CommState m_CommState;
            private CommProperties m_CommProperties;
            private CommModemStatus m_CommModem;
            private CommOverlappedIo m_CommIo;

            /// <summary>
            /// Type of DTR (Data Terminal Ready) control to use.
            /// </summary>
            public enum DtrControl
            {
                /// <summary>
                /// Disable DTR line.
                /// </summary>
                Disable = 0,

                /// <summary>
                /// Enable DTR line.
                /// </summary>
                Enable = 1,

                /// <summary>
                /// DTR Handshaking.
                /// </summary>
                Handshake = 2
            }

            /// <summary>
            /// RTS (Request to Send) to use.
            /// </summary>
            public enum RtsControl
            {
                /// <summary>
                /// Disable RTS line.
                /// </summary>
                Disable = 0,

                /// <summary>
                /// Enable the RTS line.
                /// </summary>
                Enable = 1,

                /// <summary>
                /// RTS Handshaking.
                /// </summary>
                Handshake = 2,

                /// <summary>
                /// RTS Toggling.
                /// </summary>
                Toggle = 3
            }

            /// <summary>
            /// Default constructor, doesn't associate with a COM port.
            /// </summary>
            public NativeSerialPort() 
            {
                m_CommState = new CommState();
                m_CommIo = new CommOverlappedIo();
            }

            /// <summary>
            /// Create a new NativeSerialPort object, opening a connection to the COM port specified.
            /// </summary>
            /// <param name="port">The port to open.</param>
            public NativeSerialPort(string port)
            {
                Port = port;
                Open();
            }

            /// <summary>
            /// The name of the port to open and operate with.
            /// </summary>
            public string Port
            {
                get { return m_Port; }
                set {
                    if (IsDisposed) throw new ObjectDisposedException("NativeSerialPort");
                    if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Must provide a valid port name");

                    if (m_Port != value) {
                        if (IsOpen) throw new InvalidOperationException("Serial Port currently open");
                        m_Port = value;
                    }
                }
            }

            private int m_DriverInQueue = 4096;

            /// <summary>
            /// Specify the driver In Queue at the time it is opened.
            /// </summary>
            /// <remarks>
            /// This provides the driver a recommended internal input buffer, in bytes. 
            /// </remarks>
            public int DriverInQueue
            {
                get { return m_DriverInQueue; }
                set
                {
                    if (value <= 0) throw new ArgumentOutOfRangeException("value", "value must be a positive integer");
                    m_DriverInQueue = value;
                }
            }

            private int m_DriverOutQueue = 2048;

            /// <summary>
            /// Specify the driver Out Queue at the time it is opened.
            /// </summary>
            /// <remarks>
            /// This provides the driver a recommended internal output buffer, in bytes. 
            /// </remarks>
            public int DriverOutQueue
            {
                get { return m_DriverOutQueue; }
                set
                {
                    if (value <= 0) throw new ArgumentOutOfRangeException("value", "value must be a positive integer");
                    m_DriverInQueue = value;
                }
            }

            /// <summary>
            /// Open the port specified by the Port property.
            /// </summary>
            public void Open()
            {
                if (IsDisposed) throw new ObjectDisposedException("NativeSerialPort");
                if (string.IsNullOrWhiteSpace(m_Port)) throw new InvalidOperationException("Port must first be set");
                if (IsOpen) throw new InvalidOperationException("Serial Port currently open");

                m_ComPortHandle = UnsafeNativeMethods.CreateFile(@"\\.\" + m_Port,
                    NativeMethods.FileAccess.GENERIC_READ | NativeMethods.FileAccess.GENERIC_WRITE,
                    NativeMethods.FileShare.FILE_SHARE_NONE, 
                    IntPtr.Zero,
                    NativeMethods.CreationDisposition.OPEN_EXISTING,
                    NativeMethods.FileAttributes.FILE_FLAG_OVERLAPPED,
                    IntPtr.Zero);
                if (m_ComPortHandle.IsInvalid) WinIOError();

                NativeMethods.FileType t = UnsafeNativeMethods.GetFileType(m_ComPortHandle);
                if (t != NativeMethods.FileType.FILE_TYPE_CHAR && t != NativeMethods.FileType.FILE_TYPE_UNKNOWN) {
                    m_ComPortHandle.Close();
                    m_ComPortHandle = null;
                    throw new IOException("Wrong Filetype: " + m_Port);
                }

                // Set the default parameters
                UnsafeNativeMethods.SetupComm(m_ComPortHandle, m_DriverInQueue, m_DriverOutQueue);
                
                m_CommState = new CommState(m_ComPortHandle, m_CommState);
                m_CommProperties = new CommProperties(m_ComPortHandle);
                m_CommModem = new CommModemStatus(m_ComPortHandle);
                m_CommIo = new CommOverlappedIo(m_ComPortHandle, m_CommIo, Port);
            }

            /// <summary>
            /// Open the port specified, changing also the Port property.
            /// </summary>
            /// <param name="port">The port to open.</param>
            public void Open(string port)
            {
                Port = port;
                Open();
            }

            /// <summary>
            /// Tests if the COM Port is still managed by this class.
            /// </summary>
            public bool IsOpen
            {
                get { return m_ComPortHandle != null && !m_ComPortHandle.IsClosed && !m_ComPortHandle.IsInvalid; }
            }

            /// <summary>
            /// Get information about the Comm Port status via GetCommState() and SetCommState().
            /// </summary>
            public CommState SerialPortCommState
            {
                get { return m_CommState; }
            }

            /// <summary>
            /// Get information about the Comm Port status via GetCommProperties().
            /// </summary>
            public CommProperties SerialPortCommProperties
            {
                get { return m_CommProperties; }
            }

            /// <summary>
            /// Get information about the Comm Port status via GetCommModemStatus().
            /// </summary>
            public CommModemStatus SerialPortModemStatus
            {
                get { return m_CommModem; }
            }

            /// <summary>
            /// An object to manage overlapped I/O. Through this you can start/stop I/O.
            /// </summary>
            public CommOverlappedIo SerialPortIo
            {
                get { return m_CommIo; }
            }

            private void WinIOError()
            {
                int e = Marshal.GetLastWin32Error();

                switch (e) {
                case 2:
                case 3:
                    throw new IOException("Port not found: " + m_Port, e);
                case 5:
                    throw new UnauthorizedAccessException("Access Denied: " + m_Port);
                case 32:
                    throw new IOException("Sharing violation: " + m_Port, e);
                case 206:
                    throw new PathTooLongException("Path too long: " + m_Port);
                }
                throw new IOException("Unknown error 0x" + e.ToString("X") + ": " + m_Port, e);
            }

            /// <summary>
            /// Close the file handle and release resources.
            /// </summary>
            public void Close() 
            {
                if (!IsDisposed && IsOpen) {
                    m_CommIo.Stop();
                    m_ComPortHandle.Close();
                    m_ComPortHandle = null;
                }
            }

            // CA1805: false is the default value.
            private bool m_IsDisposed;

            /// <summary>
            /// Indicate if the NativeSerialPort object is disposed.
            /// </summary>
            public bool IsDisposed
            {
                get { return m_IsDisposed; }
            }

            /// <summary>
            /// Close the file handle and release resources, disposing this object.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Release resources.
            /// </summary>
            /// <param name="disposing"><b>true</b> if this is being called from dispose(), or <b>false</b> 
            /// if by the finaliser.</param>
            private void Dispose(bool disposing)
            {
                if (disposing) {
                    if (IsOpen) Close();
                    m_CommIo.Dispose();
                    m_CommIo = null;
                    m_CommState = null;
                    m_CommProperties = null;
                    m_CommModem = null;
                }

                // Note: the SafeFileHandle will close the object itself when finalising, so
                // we don't need to do it here. It would be different if we managed the handle
                // with an IntPtr however.
                m_IsDisposed = true;
            }
        }
    }
}
