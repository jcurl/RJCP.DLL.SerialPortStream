// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Unix
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    [SupportedOSPlatform("linux")]
    internal static partial class LibNSerial
    {
        [ThreadStatic]
        private static int m_ErrNo;

        public static int errno
        {
            get { return m_ErrNo; }
            set { m_ErrNo = value; }
        }

        public static string serial_version()
        {
            IntPtr version = Dll.serial_version();
            return Marshal.PtrToStringAnsi(version);
        }

        public static SafeSerialHandle serial_init()
        {
            SafeSerialHandle result = Dll.serial_init();
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static void serial_terminate(SafeSerialHandle handle)
        {
            handle.Dispose();
        }

        public static PortDescription[] serial_getports(SafeSerialHandle handle)
        {
            // The portdesc is an array of two string pointers, where the last element is zero
            IntPtr portdesc;
            portdesc = Dll.serial_getports(handle);
            errno = Marshal.GetLastWin32Error();
            if (portdesc.Equals(IntPtr.Zero)) return null;

            // Get the number of ports in the system.
            int portNum = 0;
            IntPtr portName;
            do {
                portName = Marshal.ReadIntPtr(portdesc, portNum * 2 * IntPtr.Size);
                if (portName != IntPtr.Zero) portNum++;
            } while (portName != IntPtr.Zero);

            // Copy them into our struct
            PortDescription[] ports = new PortDescription[portNum];
            for (int i = 0; i < portNum; i++) {
                IntPtr portPtr = Marshal.ReadIntPtr(portdesc, i * 2 * IntPtr.Size);
                string port = Marshal.PtrToStringAnsi(portPtr);
                IntPtr descPtr = Marshal.ReadIntPtr(portdesc, i * 2 * IntPtr.Size + IntPtr.Size);
                string desc;
                if (descPtr.Equals(IntPtr.Zero)) {
                    desc = string.Empty;
                } else {
                    desc = Marshal.PtrToStringAnsi(descPtr);
                }
                ports[i] = new PortDescription(port, desc);
            }

            return ports;
        }

        public static int serial_setdevicename(SafeSerialHandle handle, string deviceName)
        {
            int result = Dll.serial_setdevicename(handle, deviceName);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static string serial_getdevicename(SafeSerialHandle handle)
        {
            IntPtr deviceName = Dll.serial_getdevicename(handle);
            errno = Marshal.GetLastWin32Error();
            if (deviceName.Equals(IntPtr.Zero)) return null;
            return Marshal.PtrToStringAnsi(deviceName);
        }

        public static int serial_setbaud(SafeSerialHandle handle, int baud)
        {
            int result = Dll.serial_setbaud(handle, baud);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getbaud(SafeSerialHandle handle, out int baud)
        {
            int result = Dll.serial_getbaud(handle, out baud);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setdatabits(SafeSerialHandle handle, int databits)
        {
            int result = Dll.serial_setdatabits(handle, databits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getdatabits(SafeSerialHandle handle, out int databits)
        {
            int result = Dll.serial_getdatabits(handle, out databits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setparity(SafeSerialHandle handle, Parity parity)
        {
            int result = Dll.serial_setparity(handle, parity);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getparity(SafeSerialHandle handle, out Parity parity)
        {
            int result = Dll.serial_getparity(handle, out parity);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setstopbits(SafeSerialHandle handle, StopBits stopbits)
        {
            int result = Dll.serial_setstopbits(handle, stopbits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getstopbits(SafeSerialHandle handle, out StopBits stopbits)
        {
            int result = Dll.serial_getstopbits(handle, out stopbits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setdiscardnull(SafeSerialHandle handle, bool discardNull)
        {
            int result = Dll.serial_setdiscardnull(handle, discardNull);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getdiscardnull(SafeSerialHandle handle, out bool discardNull)
        {
            int result = Dll.serial_getdiscardnull(handle, out discardNull);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setparityreplace(SafeSerialHandle handle, int parityReplace)
        {
            int result = Dll.serial_setparityreplace(handle, parityReplace);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getparityreplace(SafeSerialHandle handle, out int parityReplace)
        {
            int result = Dll.serial_getparityreplace(handle, out parityReplace);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_settxcontinueonxoff(SafeSerialHandle handle, bool txContinueOnXOff)
        {
            int result = Dll.serial_settxcontinueonxoff(handle, txContinueOnXOff);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_gettxcontinueonxoff(SafeSerialHandle handle, out bool txContinueOnXOff)
        {
            int result = Dll.serial_gettxcontinueonxoff(handle, out txContinueOnXOff);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setxofflimit(SafeSerialHandle handle, int xoffLimit)
        {
            int result = Dll.serial_setxofflimit(handle, xoffLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getxofflimit(SafeSerialHandle handle, out int xoffLimit)
        {
            int result = Dll.serial_getxofflimit(handle, out xoffLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setxonlimit(SafeSerialHandle handle, int xonLimit)
        {
            int result = Dll.serial_setxonlimit(handle, xonLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getxonlimit(SafeSerialHandle handle, out int xonLimit)
        {
            int result = Dll.serial_getxonlimit(handle, out xonLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_sethandshake(SafeSerialHandle handle, Handshake handshake)
        {
            int result = Dll.serial_sethandshake(handle, handshake);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_gethandshake(SafeSerialHandle handle, out Handshake handshake)
        {
            int result = Dll.serial_gethandshake(handle, out handshake);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_open(SafeSerialHandle handle)
        {
            int result = Dll.serial_open(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_close(SafeSerialHandle handle)
        {
            int result = Dll.serial_close(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_isopen(SafeSerialHandle handle, out bool isOpen)
        {
            int result = Dll.serial_isopen(handle, out isOpen);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setproperties(SafeSerialHandle handle)
        {
            int result = Dll.serial_setproperties(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getproperties(SafeSerialHandle handle)
        {
            int result = Dll.serial_getproperties(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getdcd(SafeSerialHandle handle, out bool dcd)
        {
            int result = Dll.serial_getdcd(handle, out dcd);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getri(SafeSerialHandle handle, out bool ri)
        {
            int result = Dll.serial_getri(handle, out ri);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getdsr(SafeSerialHandle handle, out bool dsr)
        {
            int result = Dll.serial_getdsr(handle, out dsr);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getcts(SafeSerialHandle handle, out bool cts)
        {
            int result = Dll.serial_getcts(handle, out cts);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setdtr(SafeSerialHandle handle, bool dtr)
        {
            int result = Dll.serial_setdtr(handle, dtr);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getdtr(SafeSerialHandle handle, out bool dtr)
        {
            int result = Dll.serial_getdtr(handle, out dtr);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setrts(SafeSerialHandle handle, bool rts)
        {
            int result = Dll.serial_setrts(handle, rts);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getrts(SafeSerialHandle handle, out bool rts)
        {
            int result = Dll.serial_getrts(handle, out rts);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_setbreak(SafeSerialHandle handle, bool breakState)
        {
            int result = Dll.serial_setbreak(handle, breakState);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_getbreak(SafeSerialHandle handle, out bool breakState)
        {
            int result = Dll.serial_getbreak(handle, out breakState);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static string serial_error(SafeSerialHandle handle)
        {
            IntPtr errorString = Dll.serial_error(handle);
            errno = Marshal.GetLastWin32Error();
            if (errorString.Equals(IntPtr.Zero)) return null;
            return Marshal.PtrToStringAnsi(errorString);
        }

        public static SerialReadWriteEvent serial_waitforevent(SafeSerialHandle handle, SerialReadWriteEvent rwevent, int timeout)
        {
            SerialReadWriteEvent result = Dll.serial_waitforevent(handle, rwevent, timeout);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_abortwaitforevent(SafeSerialHandle handle)
        {
            int result = Dll.serial_abortwaitforevent(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_read(SafeSerialHandle handle, IntPtr data, int length)
        {
            int result = Dll.serial_read(handle, data, length);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_write(SafeSerialHandle handle, IntPtr data, int length)
        {
            int result = Dll.serial_write(handle, data, length);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static WaitForModemEvent serial_waitformodemevent(SafeSerialHandle handle, WaitForModemEvent mevent)
        {
            WaitForModemEvent result = Dll.serial_waitformodemevent(handle, mevent);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_abortwaitformodemevent(SafeSerialHandle handle)
        {
            int result = Dll.serial_abortwaitformodemevent(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_discardinbuffer(SafeSerialHandle handle)
        {
            int result = Dll.serial_discardinbuffer(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static int serial_discardoutbuffer(SafeSerialHandle handle)
        {
            int result = Dll.serial_discardoutbuffer(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public static SysErrNo netfx_errno(int errno)
        {
            return (SysErrNo)Dll.netfx_errno(errno);
        }

        public static string netfx_errstring(int errno)
        {
            IntPtr strerror = Dll.netfx_errstring(errno);
            return Marshal.PtrToStringAnsi(strerror);
        }
    }
}
