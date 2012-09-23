// $URL$
// $Id$
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Collections.Specialized;

namespace RJCP.IO.Ports
{
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

                private Native.ModemStat m_ModemStatus;

                internal CommModemStatus(SafeFileHandle handle)
                {
                    m_ComPortHandle = handle;
                }

                public void GetCommModemStatus()
                {
                    Native.ModemStat s;
                    if (!Native.GetCommModemStatus(m_ComPortHandle, out s)) {
                        throw new IOException("Unable to get serial port modem state", Marshal.GetLastWin32Error());
                    }

                    m_ModemStatus = s;
                }

                public bool Cts { get { return (m_ModemStatus & Native.ModemStat.MS_CTS_ON) != 0; } }

                public bool Dsr { get { return (m_ModemStatus & Native.ModemStat.MS_DSR_ON) != 0; } }

                public bool Ring { get { return (m_ModemStatus & Native.ModemStat.MS_RING_ON) != 0; } }

                public bool Rlsd { get { return (m_ModemStatus & Native.ModemStat.MS_RLSD_ON) != 0; } }

                public void ClearCommBreak()
                {
                    if (!Native.ClearCommBreak(m_ComPortHandle)) {
                        throw new IOException("Unable to clear the serial break state", Marshal.GetLastWin32Error());
                    }
                }

                public void SetCommBreak()
                {
                    if (!Native.SetCommBreak(m_ComPortHandle)) {
                        throw new IOException("Unable to set the serial break state", Marshal.GetLastWin32Error());
                    }
                }

                public void SetDtr(bool value)
                {
                    if (!Native.EscapeCommFunction(m_ComPortHandle, value ? Native.ExtendedFunctions.SETDTR : Native.ExtendedFunctions.CLRDTR)) {
                        throw new IOException("Unable to set DTR state explicitly", Marshal.GetLastWin32Error());
                    }
                }

                public void SetRts(bool value)
                {
                    if (!Native.EscapeCommFunction(m_ComPortHandle, value ? Native.ExtendedFunctions.SETRTS : Native.ExtendedFunctions.CLRRTS)) {
                        throw new IOException("Unable to set RTS state explicitly", Marshal.GetLastWin32Error());
                    }
                }

            }
        }
    }
}
