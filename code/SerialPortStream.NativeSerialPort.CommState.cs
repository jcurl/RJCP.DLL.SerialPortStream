// $URL$
// $Id$

// Copyright © Jason Curl 2012-2014
// See http://serialportstream.codeplex.com for license details (MS-PL License)

namespace RJCP.IO.Ports
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Text;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;
    using RJCP.IO.Ports.Native;

    public partial class SerialPortStream
    {
        private sealed partial class NativeSerialPort : IDisposable
        {
            /// <summary>
            /// Abstracts the Win32 API GetCommState() and SetCommState().
            /// </summary>
            public sealed class CommState
            {
                private SafeFileHandle m_ComPortHandle;
                private NativeMethods.DCB m_Dcb = new NativeMethods.DCB();

                /// <summary>
                /// Default constructor for a DCB.
                /// </summary>
                /// <remarks>
                /// The default constructor allocates space for the serial DCB (Device Control Block).
                /// This object is not associated with a serial port so setting and getting the state
                /// functions will fail with an exception.
                /// </remarks>
                internal CommState()
                {
                    m_Dcb.DCBLength = Marshal.SizeOf(m_Dcb);
                }

                /// <summary>
                /// Constructor for a DCB, prefilled with data from the handle provided.
                /// </summary>
                /// <remarks>
                /// This constructor allocates space for a DCB (Device Control Block) and gets the
                /// state of the serial port provided by the handle. You can then update the DCB
                /// with the properties of this class and set them with the SetCommState() method.
                /// </remarks>
                /// <param name="handle">Valid handle to a serial port object.</param>
                internal CommState(SafeFileHandle handle) : this(handle, null) { }

                /// <summary>
                /// Constructor for a DCB, using the DCB provided by another object.
                /// </summary>
                /// <param name="handle">Valid handle to a serial port object.</param>
                /// <param name="state">A CommState object for which the DCB settings should be
                /// taken. Note, the handle managed by the state object is ignored. If this
                /// parameter is null, then a default DCB is created.</param>
                internal CommState(SafeFileHandle handle, CommState state)
                {
                    m_ComPortHandle = handle;
                    if (state == null) {
                        m_Dcb.DCBLength = Marshal.SizeOf(m_Dcb);
                        GetCommState();
                    } else {
                        m_Dcb = state.m_Dcb;
                    }
                }

                /// <summary>
                /// Call Win32API to get the DCB for the current serial port object.
                /// </summary>
                /// <remarks>
                /// Calls the Win32 native API to get the DCB for the current serial port object.
                /// This assumes that the handle is valid. The Operating System will check the
                /// handle for validity. If there are problems, the IOException() exception
                /// will be raised.
                /// </remarks>
                /// <exception cref="IOException">The DCB could not be obtained.</exception>
                public void GetCommState()
                {
                    if (!UnsafeNativeMethods.GetCommState(m_ComPortHandle, ref m_Dcb)) {
                        throw new IOException("Unable to get serial port state", Marshal.GetLastWin32Error());
                    }
                }

                /// <summary>
                /// Call Win32API to set the DCB based on the current properties of this object.
                /// </summary>
                /// <remarks>
                /// Calls the Win32 native API to get the DCB for the current serial port object.
                /// This assumes that the handle is valid. The Operating System will check the
                /// handle for validity. If there are problems, the IOException() exception
                /// will be raised.
                /// </remarks>
                /// <exception cref="IOException">The DCB could not be set. The reasons why the
                /// DCB cannot be set may be dependent on an invalid handle, or an invalid
                /// combination of data in the DCB.</exception>
                public void SetCommState()
                {
                    if (!UnsafeNativeMethods.SetCommState(m_ComPortHandle, ref m_Dcb)) {
                        throw new IOException("Unable to set the serial port state", Marshal.GetLastWin32Error());
                    }
                }

                public bool Binary
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.Binary) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.Binary)) | (value ? NativeMethods.DcbFlags.Binary : 0); }
                }

                public int BaudRate
                {
                    get { return (int)m_Dcb.BaudRate; }
                    set { m_Dcb.BaudRate = (uint)value; }
                }

                public bool ParityEnable
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.Parity) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.Parity)) | (value ? NativeMethods.DcbFlags.Parity : 0); }
                }

                public bool OutCtsFlow
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.OutxCtsFlow) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.OutxCtsFlow)) | (value ? NativeMethods.DcbFlags.OutxCtsFlow : 0); }
                }

                public bool OutDsrFlow
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.OutxDsrFlow) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.OutxDsrFlow)) | (value ? NativeMethods.DcbFlags.OutxDsrFlow : 0); }
                }

                public DtrControl DtrControl
                {
                    get
                    {
                        int dtrc = (int)(m_Dcb.Flags & NativeMethods.DcbFlags.DtrControlMask);
                        return (DtrControl)(dtrc >> 4);
                    }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.DtrControlMask)) | (NativeMethods.DcbFlags)(((int)value) << 4); }
                }

                public bool DsrSensitivity
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.DsrSensitivity) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.DsrSensitivity)) | (value ? NativeMethods.DcbFlags.DsrSensitivity : 0); }
                }

                public bool TxContinueOnXOff
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.TxContinueOnXOff) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.TxContinueOnXOff)) | (value ? NativeMethods.DcbFlags.TxContinueOnXOff : 0); }
                }

                public bool OutX
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.OutX) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.OutX)) | (value ? NativeMethods.DcbFlags.OutX : 0); }
                }

                public bool InX
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.InX) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.InX)) | (value ? NativeMethods.DcbFlags.InX : 0); }
                }

                public byte XOnChar
                {
                    get { return m_Dcb.XonChar; }
                    set { m_Dcb.XonChar = value; }
                }

                public byte XOffChar
                {
                    get { return m_Dcb.XoffChar; }
                    set { m_Dcb.XoffChar = value; }
                }

                public bool ErrorCharEnabled
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.ErrorChar) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.ErrorChar)) | (value ? NativeMethods.DcbFlags.ErrorChar : 0); }
                }

                public byte ErrorChar
                {
                    get { return m_Dcb.ErrorChar; }
                    set { m_Dcb.ErrorChar = value; }
                }

                public byte EventChar
                {
                    get { return m_Dcb.EvtChar; }
                    set { m_Dcb.EvtChar = value; }
                }

                public byte EofChar
                {
                    get { return m_Dcb.EofChar; }
                    set { m_Dcb.EofChar = value; }
                }

                public bool Null
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.Null) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.Null)) | (value ? NativeMethods.DcbFlags.Null : 0); }
                }

                public RtsControl RtsControl
                {
                    get
                    {
                        int dtrc = (int)(m_Dcb.Flags & NativeMethods.DcbFlags.RtsControlMask);
                        return (RtsControl)(dtrc >> 12);
                    }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.RtsControlMask)) | (NativeMethods.DcbFlags)(((int)value) << 12); }
                }

                public bool AbortOnError
                {
                    get { return (m_Dcb.Flags & NativeMethods.DcbFlags.AbortOnError) != 0; }
                    set { m_Dcb.Flags = (m_Dcb.Flags & (~NativeMethods.DcbFlags.AbortOnError)) | (value ? NativeMethods.DcbFlags.AbortOnError : 0); }
                }

                public int XonLim
                {
                    get { return m_Dcb.XonLim; }
                    set { m_Dcb.XonLim = (ushort)value; }
                }

                public int XoffLim
                {
                    get { return m_Dcb.XoffLim; }
                    set { m_Dcb.XoffLim = (ushort)value; }
                }

                public int ByteSize
                {
                    get { return m_Dcb.ByteSize; }
                    set { m_Dcb.ByteSize = (byte)value; }
                }

                public Parity Parity
                {
                    get { return (Parity)m_Dcb.Parity; }
                    set { m_Dcb.Parity = (byte)value; }
                }

                public StopBits StopBits
                {
                    get { return (StopBits)m_Dcb.StopBits; }
                    set { m_Dcb.StopBits = (byte)value; }
                }
            }
        }
    }
}
