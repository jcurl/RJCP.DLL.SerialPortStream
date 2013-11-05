// $URL$
// $Id$

// Copyright © Jason Curl 2012
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

    public partial class SerialPortStream
    {
        private sealed partial class NativeSerialPort : IDisposable
        {
            /// <summary>
            /// Abstracts the Win32 API GetCommModemStatus()
            /// </summary>
            public sealed class CommModemStatus
            {
                private SafeFileHandle m_ComPortHandle;

                private NativeMethods.ModemStat m_ModemStatus;

                internal CommModemStatus(SafeFileHandle handle)
                {
                    m_ComPortHandle = handle;
                }

                public void GetCommModemStatus()
                {
                    NativeMethods.ModemStat s;
                    if (!UnsafeNativeMethods.GetCommModemStatus(m_ComPortHandle, out s)) {
                        throw new IOException("Unable to get serial port modem state", Marshal.GetLastWin32Error());
                    }

                    m_ModemStatus = s;
                }

                public bool Cts { get { return (m_ModemStatus & NativeMethods.ModemStat.MS_CTS_ON) != 0; } }

                public bool Dsr { get { return (m_ModemStatus & NativeMethods.ModemStat.MS_DSR_ON) != 0; } }

                public bool Ring { get { return (m_ModemStatus & NativeMethods.ModemStat.MS_RING_ON) != 0; } }

                public bool Rlsd { get { return (m_ModemStatus & NativeMethods.ModemStat.MS_RLSD_ON) != 0; } }

                public void ClearCommBreak()
                {
                    if (!UnsafeNativeMethods.ClearCommBreak(m_ComPortHandle)) {
                        throw new IOException("Unable to clear the serial break state", Marshal.GetLastWin32Error());
                    }
                }

                public void SetCommBreak()
                {
                    if (!UnsafeNativeMethods.SetCommBreak(m_ComPortHandle)) {
                        throw new IOException("Unable to set the serial break state", Marshal.GetLastWin32Error());
                    }
                }

                public void SetDtr(bool value)
                {
                    if (!UnsafeNativeMethods.EscapeCommFunction(m_ComPortHandle, value ? NativeMethods.ExtendedFunctions.SETDTR : NativeMethods.ExtendedFunctions.CLRDTR)) {
                        throw new IOException("Unable to set DTR state explicitly", Marshal.GetLastWin32Error());
                    }
                }

                public void SetRts(bool value)
                {
                    if (!UnsafeNativeMethods.EscapeCommFunction(m_ComPortHandle, value ? NativeMethods.ExtendedFunctions.SETRTS : NativeMethods.ExtendedFunctions.CLRRTS)) {
                        throw new IOException("Unable to set RTS state explicitly", Marshal.GetLastWin32Error());
                    }
                }
            }
        }
    }
}
