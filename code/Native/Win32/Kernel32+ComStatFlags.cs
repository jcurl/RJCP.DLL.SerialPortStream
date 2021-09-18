// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Win32
{
    internal static partial class Kernel32
    {
        public enum ComStatFlags
        {
            CtsHold = 0x01,
            DsrHold = 0x02,
            RlsdHold = 0x04,
            XoffHold = 0x08,
            XoffSent = 0x10,
            Eof = 0x20,
            Txim = 0x40
        }
    }
}
