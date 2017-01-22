// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Unix
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class SerialUnix : INativeSerialDll
    {
        [SuppressUnmanagedCodeSecurity]
        private static class SafeNativeMethods
        {
            [DllImport("libnserial.so.1")]
            internal static extern IntPtr serial_version();
        }

        [SuppressUnmanagedCodeSecurity]
        private static class UnsafeNativeMethods
        {
            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern IntPtr serial_init();

            [DllImport("libnserial.so.1")]
            internal static extern void serial_terminate(IntPtr handle);

            [DllImport("libnserial.so.1", SetLastError = true, ThrowOnUnmappableChar = true, BestFitMapping = false)]
            internal static extern int serial_setdevicename(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string deviceName);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern IntPtr serial_getdevicename(IntPtr handle);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setbaud(IntPtr handle, int baud);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getbaud(IntPtr handle, out int baud);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setdatabits(IntPtr handle, int databits);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getdatabits(IntPtr handle, out int databits);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setparity(IntPtr handle, Parity parity);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getparity(IntPtr handle, out Parity parity);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setstopbits(IntPtr handle, StopBits stopbits);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getstopbits(IntPtr handle, out StopBits stopbits);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setdiscardnull(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool discardnull);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getdiscardnull(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool discardnull);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setparityreplace(IntPtr handle, int parityReplace);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getparityreplace(IntPtr handle, out int parityReplace);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_settxcontinueonxoff(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool txContinueOnXOff);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_gettxcontinueonxoff(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool txContinueOnXOff);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setxofflimit(IntPtr handle, int xoffLimit);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getxofflimit(IntPtr handle, out int xoffLimit);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setxonlimit(IntPtr handle, int xonLimit);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getxonlimit(IntPtr handle, out int xonLimit);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_sethandshake(IntPtr handle, Handshake handshake);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_gethandshake(IntPtr handle, out Handshake handshake);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_open(IntPtr handle);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_close(IntPtr handle);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_isopen(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool isOpen);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setproperties(IntPtr handle);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getproperties(IntPtr handle);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getdcd(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool dcd);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getri(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool ri);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getdsr(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool dsr);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getcts(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool cts);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setdtr(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool dtr);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getdtr(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool dtr);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setrts(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool rts);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getrts(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool rts);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_setbreak(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] bool breakState);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_getbreak(IntPtr handle, [MarshalAs(UnmanagedType.Bool)] out bool breakState);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern IntPtr serial_error(IntPtr handle);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern SerialReadWriteEvent serial_waitforevent(IntPtr handle, SerialReadWriteEvent rwevent, int timeout);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_abortwaitforevent(IntPtr handle);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_read(IntPtr handle, IntPtr buffer, int length);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_write(IntPtr handle, IntPtr buffer, int length);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern WaitForModemEvent serial_waitformodemevent(IntPtr handle, WaitForModemEvent mevent);

            [DllImport("libnserial.so.1", SetLastError = true)]
            internal static extern int serial_abortwaitformodemevent(IntPtr handle);
        }

        [ThreadStatic]
        private static int m_ErrNo = 0;

        public int errno
        {
            get { return m_ErrNo; }
            set { m_ErrNo = value; }
        }

        public string serial_version()
        {
            IntPtr version = SafeNativeMethods.serial_version();
            return Marshal.PtrToStringAnsi(version);
        }

        public IntPtr serial_init()
        {
            IntPtr result = UnsafeNativeMethods.serial_init();
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public void serial_terminate(IntPtr handle)
        {
            UnsafeNativeMethods.serial_terminate(handle);
            errno = Marshal.GetLastWin32Error();
        }

        public int serial_setdevicename(IntPtr handle, string deviceName)
        {
            int result = UnsafeNativeMethods.serial_setdevicename(handle, deviceName);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public string serial_getdevicename(IntPtr handle)
        {
            IntPtr deviceName = UnsafeNativeMethods.serial_getdevicename(handle);
            errno = Marshal.GetLastWin32Error();
            if (deviceName.Equals(IntPtr.Zero)) return null;
            return Marshal.PtrToStringAnsi(deviceName);
        }

        public int serial_setbaud(IntPtr handle, int baud)
        {
            int result = UnsafeNativeMethods.serial_setbaud(handle, baud);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getbaud(IntPtr handle, out int baud)
        {
            int result = UnsafeNativeMethods.serial_getbaud(handle, out baud);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setdatabits(IntPtr handle, int databits)
        {
            int result = UnsafeNativeMethods.serial_setdatabits(handle, databits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getdatabits(IntPtr handle, out int databits)
        {
            int result = UnsafeNativeMethods.serial_getdatabits(handle, out databits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setparity(IntPtr handle, Parity parity)
        {
            int result = UnsafeNativeMethods.serial_setparity(handle, parity);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getparity(IntPtr handle, out Parity parity)
        {
            int result = UnsafeNativeMethods.serial_getparity(handle, out parity);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setstopbits(IntPtr handle, StopBits stopbits)
        {
            int result = UnsafeNativeMethods.serial_setstopbits(handle, stopbits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getstopbits(IntPtr handle, out StopBits stopbits)
        {
            int result = UnsafeNativeMethods.serial_getstopbits(handle, out stopbits);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setdiscardnull(IntPtr handle, bool discardnull)
        {
            int result = UnsafeNativeMethods.serial_setdiscardnull(handle, discardnull);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getdiscardnull(IntPtr handle, out bool discardNull)
        {
            int result = UnsafeNativeMethods.serial_getdiscardnull(handle, out discardNull);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setparityreplace(IntPtr handle, int parityReplace)
        {
            int result = UnsafeNativeMethods.serial_setparityreplace(handle, parityReplace);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getparityreplace(IntPtr handle, out int parityReplace)
        {
            int result = UnsafeNativeMethods.serial_getparityreplace(handle, out parityReplace);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_settxcontinueonxoff(IntPtr handle, bool txContinueOnXOff)
        {
            int result = UnsafeNativeMethods.serial_settxcontinueonxoff(handle, txContinueOnXOff);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_gettxcontinueonxoff(IntPtr handle, out bool txContinueOnXOff)
        {
            int result = UnsafeNativeMethods.serial_gettxcontinueonxoff(handle, out txContinueOnXOff);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setxofflimit(IntPtr handle, int xoffLimit)
        {
            int result = UnsafeNativeMethods.serial_setxofflimit(handle, xoffLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getxofflimit(IntPtr handle, out int xoffLimit)
        {
            int result = UnsafeNativeMethods.serial_getxofflimit(handle, out xoffLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setxonlimit(IntPtr handle, int xonLimit)
        {
            int result = UnsafeNativeMethods.serial_setxonlimit(handle, xonLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getxonlimit(IntPtr handle, out int xonLimit)
        {
            int result = UnsafeNativeMethods.serial_getxonlimit(handle, out xonLimit);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_sethandshake(IntPtr handle, Handshake handshake)
        {
            int result = UnsafeNativeMethods.serial_sethandshake(handle, handshake);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_gethandshake(IntPtr handle, out Handshake handshake)
        {
            int result = UnsafeNativeMethods.serial_gethandshake(handle, out handshake);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_open(IntPtr handle)
        {
            int result = UnsafeNativeMethods.serial_open(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_close(IntPtr handle)
        {
            int result = UnsafeNativeMethods.serial_close(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_isopen(IntPtr handle, out bool isOpen)
        {
            int result = UnsafeNativeMethods.serial_isopen(handle, out isOpen);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setproperties(IntPtr handle)
        {
            int result = UnsafeNativeMethods.serial_setproperties(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getproperties(IntPtr handle)
        {
            int result = UnsafeNativeMethods.serial_getproperties(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getdcd(IntPtr handle, out bool dcd)
        {
            int result = UnsafeNativeMethods.serial_getdcd(handle, out dcd);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getri(IntPtr handle, out bool ri)
        {
            int result = UnsafeNativeMethods.serial_getri(handle, out ri);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getdsr(IntPtr handle, out bool dsr)
        {
            int result = UnsafeNativeMethods.serial_getdsr(handle, out dsr);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getcts(IntPtr handle, out bool cts)
        {
            int result = UnsafeNativeMethods.serial_getcts(handle, out cts);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setdtr(IntPtr handle, bool dtr)
        {
            int result = UnsafeNativeMethods.serial_setdtr(handle, dtr);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getdtr(IntPtr handle, out bool dtr)
        {
            int result = UnsafeNativeMethods.serial_getdtr(handle, out dtr);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setrts(IntPtr handle, bool rts)
        {
            int result = UnsafeNativeMethods.serial_setrts(handle, rts);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getrts(IntPtr handle, out bool rts)
        {
            int result = UnsafeNativeMethods.serial_getrts(handle, out rts);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_setbreak(IntPtr handle, bool breakState)
        {
            int result = UnsafeNativeMethods.serial_setbreak(handle, breakState);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_getbreak(IntPtr handle, out bool breakState)
        {
            int result = UnsafeNativeMethods.serial_getbreak(handle, out breakState);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public string serial_error(IntPtr handle)
        {
            IntPtr errorString = UnsafeNativeMethods.serial_error(handle);
            errno = Marshal.GetLastWin32Error();
            if (errorString.Equals(IntPtr.Zero)) return null;
            return Marshal.PtrToStringAnsi(errorString);
        }

        public SerialReadWriteEvent serial_waitforevent(IntPtr handle, SerialReadWriteEvent rwevent, int timeout)
        {
            SerialReadWriteEvent result = UnsafeNativeMethods.serial_waitforevent(handle, rwevent, timeout);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_abortwaitforevent(IntPtr handle)
        {
            int result = UnsafeNativeMethods.serial_abortwaitforevent(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_read(IntPtr handle, IntPtr buffer, int length)
        {
            int result = UnsafeNativeMethods.serial_read(handle, buffer, length);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_write(IntPtr handle, IntPtr buffer, int length)
        {
            int result = UnsafeNativeMethods.serial_write(handle, buffer, length);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public WaitForModemEvent serial_waitformodemevent(IntPtr handle, WaitForModemEvent mevent)
        {
            WaitForModemEvent result = UnsafeNativeMethods.serial_waitformodemevent(handle, mevent);
            errno = Marshal.GetLastWin32Error();
            return result;
        }

        public int serial_abortwaitformodemevent(IntPtr handle)
        {
            int result = UnsafeNativeMethods.serial_abortwaitformodemevent(handle);
            errno = Marshal.GetLastWin32Error();
            return result;
        }
    }
}
