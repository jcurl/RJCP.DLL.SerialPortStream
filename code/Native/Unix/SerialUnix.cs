// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Unix
{
    using System;
    using System.Runtime.InteropServices;

    internal class SerialUnix : INativeSerialDll
    {
        [ThreadStatic]
        private int m_ErrNo = 0;

        public int errno
        {
            get { return m_ErrNo; }
            set { m_ErrNo = value; }
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_version")]
        private static extern IntPtr nserial_version();
        public string serial_version()
        {
            IntPtr version = nserial_version();
            return Marshal.PtrToStringAnsi(version);
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_init", SetLastError=true)]
        private static extern IntPtr nserial_init();
        public IntPtr serial_init()
        {
            IntPtr result = nserial_init();
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_terminate")]
        private static extern void nserial_terminate(IntPtr handle);
        public void serial_terminate(IntPtr handle)
        {
            nserial_terminate(handle);
            errno = Marshal.GetLastWin32Error();
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setdevicename", SetLastError=true)]
        private static extern int nserial_setdevicename(IntPtr handle, string deviceName);
        public int serial_setdevicename(IntPtr handle, string deviceName)
        {
            int result = nserial_setdevicename(handle, deviceName);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getdevicename", SetLastError=true)]
        private static extern IntPtr nserial_getdevicename(IntPtr handle);
        public string serial_getdevicename(IntPtr handle)
        {
            IntPtr deviceName = nserial_getdevicename(handle);
            errno = Marshal.GetLastWin32Error();
            if (deviceName.Equals(IntPtr.Zero)) return null;
            return Marshal.PtrToStringAnsi(deviceName);
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setbaud", SetLastError=true)]
        private static extern int nserial_setbaud(IntPtr handle, int baud);
        public int serial_setbaud(IntPtr handle, int baud)
        {
            int result = nserial_setbaud(handle, baud);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getbaud", SetLastError=true)]
        private static extern int nserial_getbaud(IntPtr handle, out int baud);
        public int serial_getbaud(IntPtr handle, out int baud)
        {
            int result = nserial_getbaud(handle, out baud);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setdatabits", SetLastError=true)]
        private static extern int nserial_setdatabits(IntPtr handle, int databits);
        public int serial_setdatabits(IntPtr handle, int databits)
        {
            int result = nserial_setdatabits(handle, databits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getdatabits", SetLastError=true)]
        private static extern int nserial_getdatabits(IntPtr handle, out int databits);
        public int serial_getdatabits(IntPtr handle, out int databits)
        {
            int result = nserial_getdatabits(handle, out databits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setparity", SetLastError=true)]
        private static extern int nserial_setparity(IntPtr handle, Parity parity);
        public int serial_setparity(IntPtr handle, Parity parity)
        {
            int result = nserial_setparity(handle, parity);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getparity", SetLastError=true)]
        private static extern int nserial_getparity(IntPtr handle, out Parity parity);
        public int serial_getparity(IntPtr handle, out Parity parity)
        {
            int result = nserial_getparity(handle, out parity);
            errno = Marshal.GetLastWin32Error();
            return result;
        }
            
        [DllImport("libnserial.so.1", EntryPoint="serial_setstopbits", SetLastError=true)]
        private static extern int nserial_setstopbits(IntPtr handle, StopBits stopbits);
        public int serial_setstopbits(IntPtr handle, StopBits stopbits)
        {
            int result = nserial_setstopbits(handle, stopbits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getstopbits", SetLastError=true)]
        private static extern int nserial_getstopbits(IntPtr handle, out StopBits stopbits);
        public int serial_getstopbits(IntPtr handle, out StopBits stopbits)
        {
            int result = nserial_getstopbits(handle, out stopbits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setdiscardnull", SetLastError=true)]
        private static extern int nserial_setdiscardnull(IntPtr handle, bool discardnull);
        public int serial_setdiscardnull(IntPtr handle, bool discardnull)
        {
            int result = nserial_setdiscardnull(handle, discardnull);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getdiscardnull", SetLastError=true)]
        private static extern int nserial_getdiscardnull(IntPtr handle, out bool discardnull);
        public int serial_getdiscardnull(IntPtr handle, out bool discardNull)
        {
            int result = nserial_getdiscardnull(handle, out discardNull);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setparityreplace", SetLastError=true)]
        private static extern int nserial_setparityreplace(IntPtr handle, int parityReplace);
        public int serial_setparityreplace(IntPtr handle, int parityReplace)
        {
            int result = nserial_setparityreplace(handle, parityReplace);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getparityreplace", SetLastError=true)]
        private static extern int nserial_getparityreplace(IntPtr handle, out int parityReplace);
        public int serial_getparityreplace(IntPtr handle, out int parityReplace)
        {
            int result = nserial_getparityreplace(handle, out parityReplace);
            errno = Marshal.GetLastWin32Error();
            return result;
        }
            
        [DllImport("libnserial.so.1", EntryPoint="serial_settxcontinueonxoff", SetLastError=true)]
        private static extern int nserial_settxcontinueonxoff(IntPtr handle, bool txContinueOnXOff);
        public int serial_settxcontinueonxoff(IntPtr handle, bool txContinueOnXOff)
        {
            int result = nserial_settxcontinueonxoff(handle, txContinueOnXOff);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_gettxcontinueonxoff", SetLastError=true)]
        private static extern int nserial_gettxcontinueonxoff(IntPtr handle, out bool txContinueOnXOff);
        public int serial_gettxcontinueonxoff(IntPtr handle, out bool txContinueOnXOff)
        {
            int result = nserial_gettxcontinueonxoff(handle, out txContinueOnXOff);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setxofflimit", SetLastError=true)]
        private static extern int nserial_setxofflimit(IntPtr handle, int xoffLimit);
        public int serial_setxofflimit(IntPtr handle, int xoffLimit)
        {
            int result = nserial_setxofflimit(handle, xoffLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getxofflimit", SetLastError=true)]
        private static extern int nserial_getxofflimit(IntPtr handle, out int xoffLimit);
        public int serial_getxofflimit(IntPtr handle, out int xoffLimit)
        {
            int result = nserial_getxofflimit(handle, out xoffLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setxonlimit", SetLastError=true)]
        private static extern int nserial_setxonlimit(IntPtr handle, int xonLimit);
        public int serial_setxonlimit(IntPtr handle, int xonLimit)
        {
            int result = nserial_setxonlimit(handle, xonLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getxonlimit", SetLastError=true)]
        private static extern int nserial_getxonlimit(IntPtr handle, out int xonLimit);
        public int serial_getxonlimit(IntPtr handle, out int xonLimit)
        {
            int result = nserial_getxonlimit(handle, out xonLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_sethandshake", SetLastError=true)]
        private static extern int nserial_sethandshake(IntPtr handle, Handshake handshake);
        public int serial_sethandshake(IntPtr handle, Handshake handshake)
        {
            int result = nserial_sethandshake(handle, handshake);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_gethandshake", SetLastError=true)]
        private static extern int nserial_gethandshake(IntPtr handle, out Handshake handshake);
        public int serial_gethandshake(IntPtr handle, out Handshake handshake)
        {
            int result = nserial_gethandshake(handle, out handshake);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_open", SetLastError=true)]
        private static extern int nserial_open(IntPtr handle);
        public int serial_open(IntPtr handle)
        {
            int result = nserial_open(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_close", SetLastError=true)]
        private static extern int nserial_close(IntPtr handle);
        public int serial_close(IntPtr handle)
        {
            int result = nserial_close(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_isopen", SetLastError=true)]
        private static extern int nserial_isopen(IntPtr handle, out bool isOpen);
        public int serial_isopen(IntPtr handle, out bool isOpen)
        {
            int result = nserial_isopen(handle, out isOpen);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setproperties", SetLastError=true)]
        private static extern int nserial_setproperties(IntPtr handle);
        public int serial_setproperties(IntPtr handle)
        {
            int result = nserial_setproperties(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getproperties", SetLastError=true)]
        private static extern int nserial_getproperties(IntPtr handle);
        public int serial_getproperties(IntPtr handle)
        {
            int result = nserial_getproperties(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getdcd", SetLastError=true)]
        private static extern int nserial_getdcd(IntPtr handle, out bool dcd);
        public int serial_getdcd(IntPtr handle, out bool dcd)
        {
            int result = nserial_getdcd(handle, out dcd);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getri", SetLastError=true)]
        private static extern int nserial_getri(IntPtr handle, out bool ri);
        public int serial_getri(IntPtr handle, out bool ri)
        {
            int result = nserial_getri(handle, out ri);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getdsr", SetLastError=true)]
        private static extern int nserial_getdsr(IntPtr handle, out bool dsr);
        public int serial_getdsr(IntPtr handle, out bool dsr)
        {
            int result = nserial_getdsr(handle, out dsr);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getcts", SetLastError=true)]
        private static extern int nserial_getcts(IntPtr handle, out bool cts);
        public int serial_getcts(IntPtr handle, out bool cts)
        {
            int result = nserial_getcts(handle, out cts);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setdtr", SetLastError=true)]
        private static extern int nserial_setdtr(IntPtr handle, bool dtr);
        public int serial_setdtr(IntPtr handle, bool dtr)
        {
            int result = nserial_setdtr(handle, dtr);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getdtr", SetLastError=true)]
        private static extern int nserial_getdtr(IntPtr handle, out bool dtr);
        public int serial_getdtr(IntPtr handle, out bool dtr)
        {
            int result = nserial_getdtr(handle, out dtr);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setrts", SetLastError=true)]
        private static extern int nserial_setrts(IntPtr handle, bool rts);
        public int serial_setrts(IntPtr handle, bool rts)
        {
            int result = nserial_setrts(handle, rts);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getrts", SetLastError=true)]
        private static extern int nserial_getrts(IntPtr handle, out bool rts);
        public int serial_getrts(IntPtr handle, out bool rts)
        {
            int result = nserial_getrts(handle, out rts);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_setbreak", SetLastError=true)]
        private static extern int nserial_setbreak(IntPtr handle, bool breakState);
        public int serial_setbreak(IntPtr handle, bool breakState)
        {
            int result = nserial_setbreak(handle, breakState);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_getbreak", SetLastError=true)]
        private static extern int nserial_getbreak(IntPtr handle, out bool breakState);
        public int serial_getbreak(IntPtr handle, out bool breakState)
        {
            int result = nserial_getbreak(handle, out breakState);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_error", SetLastError=true)]
        private static extern IntPtr nserial_error(IntPtr handle);
        public string serial_error(IntPtr handle)
        {
            IntPtr errorString = nserial_error(handle);
            errno = Marshal.GetLastWin32Error();
            if (errorString.Equals(IntPtr.Zero)) return null;
            return Marshal.PtrToStringAnsi(errorString);
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_waitforevent", SetLastError=true)]
        private static extern SerialReadWriteEvent nserial_waitforevent(IntPtr handle, SerialReadWriteEvent rwevent, int timeout);
        public SerialReadWriteEvent serial_waitforevent(IntPtr handle, SerialReadWriteEvent rwevent, int timeout)
        {
            SerialReadWriteEvent result = nserial_waitforevent(handle, rwevent, timeout);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_abortwaitforevent", SetLastError=true)]
        private static extern int nserial_abortwaitforevent(IntPtr handle);
        public int serial_abortwaitforevent(IntPtr handle)
        {
            int result = nserial_abortwaitforevent(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_read", SetLastError=true)]
        private static extern int nserial_read(IntPtr handle, IntPtr buffer, int length);
        public int serial_read(IntPtr handle, IntPtr buffer, int length)
        {
            int result = nserial_read(handle, buffer, length);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_write", SetLastError=true)]
        private static extern int nserial_write(IntPtr handle, IntPtr buffer, int length);
        public int serial_write(IntPtr handle, IntPtr buffer, int length)
        {
            int result = nserial_write(handle, buffer, length);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_waitformodemevent", SetLastError=true)]
        private static extern WaitForModemEvent nserial_waitformodemevent(IntPtr handle, WaitForModemEvent mevent);
        public WaitForModemEvent serial_waitformodemevent(IntPtr handle, WaitForModemEvent mevent)
        {
            WaitForModemEvent result = nserial_waitformodemevent(handle, mevent);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        [DllImport("libnserial.so.1", EntryPoint="serial_abortwaitformodemevent", SetLastError=true)]
        private static extern int nserial_abortwaitformodemevent(IntPtr handle);
        public int serial_abortwaitformodemevent(IntPtr handle)
        {
            int result = nserial_abortwaitformodemevent(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }
    }
}
