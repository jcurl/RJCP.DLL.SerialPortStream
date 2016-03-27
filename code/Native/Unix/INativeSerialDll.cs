// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Unix
{
    using System;

    interface INativeSerialDll
    {
        string serial_version();

        IntPtr serial_init();
        void serial_terminate(IntPtr handle);

        int serial_setdevicename(IntPtr handle, string deviceName);
        string serial_getdevicename(IntPtr handle);

        int serial_setbaud(IntPtr handle, int baud);
        int serial_getbaud(IntPtr handle, out int baud);
        int serial_setdatabits(IntPtr handle, int databits);
        int serial_getdatabits(IntPtr handle, out int databits);
        int serial_setparity(IntPtr handle, Parity parity);
        int serial_getparity(IntPtr handle, out Parity parity);
        int serial_setstopbits(IntPtr handle, StopBits stopbits);
        int serial_getstopbits(IntPtr handle, out StopBits stopbits);
        int serial_setdiscardnull(IntPtr handle, bool discardNull);
        int serial_getdiscardnull(IntPtr handle, out bool discardNull);
        int serial_setparityreplace(IntPtr handle, int parityReplace);
        int serial_getparityreplace(IntPtr handle, out int parityreplace);
        int serial_settxcontinueonxoff(IntPtr handle, bool txContinueOnXOff);
        int serial_gettxcontinueonxoff(IntPtr handle, out bool txContinueOnXOff);
        int serial_setxofflimit(IntPtr handle, int xofflimit);
        int serial_getxofflimit(IntPtr handle, out int xofflimit);
        int serial_setxonlimit(IntPtr handle, int xonlimit);
        int serial_getxonlimit(IntPtr handle, out int xonlimit);
        int serial_sethandshake(IntPtr handle, Handshake handshake);
        int serial_gethandshake(IntPtr handle, out Handshake handshake);

        int serial_open(IntPtr handle);
        int serial_close(IntPtr handle);
        int serial_isopen(IntPtr handle, out bool isOpen);

        int serial_setproperties(IntPtr handle);
        int serial_getproperties(IntPtr handle);

        int serial_getdcd(IntPtr handle, out bool dcd);
        int serial_getri(IntPtr handle, out bool ri);
        int serial_getdsr(IntPtr handle, out bool dsr);
        int serial_getcts(IntPtr handle, out bool cts);
        int serial_setdtr(IntPtr handle, bool dtr);
        int serial_getdtr(IntPtr handle, out bool dtr);
        int serial_setrts(IntPtr handle, bool rts);
        int serial_getrts(IntPtr handle, out bool rts);
        int serial_setbreak(IntPtr handle, bool breakState);
        int serial_getbreak(IntPtr handle, out bool breakState);

        string serial_error(IntPtr handle);
        int errno { get; }

        WaitForModemEvent serial_waitformodemevent(IntPtr handle, WaitForModemEvent mevent);
        int serial_abortwaitformodemevent(IntPtr handle);
        SerialReadWriteEvent serial_waitforevent(IntPtr handle, SerialReadWriteEvent rwevent, int timeout);
        int serial_abortwaitforevent(IntPtr handle);
        int serial_read(IntPtr handle, IntPtr data, int length);
        int serial_write(IntPtr handle, IntPtr data, int length);
    }
}
