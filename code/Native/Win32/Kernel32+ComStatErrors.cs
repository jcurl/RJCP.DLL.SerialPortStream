// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        [Flags]
        public enum ComStatErrors
        {
            CE_RXOVER = 0x0001,
            CE_OVERRUN = 0x0002,
            CE_RXPARITY = 0x0004,
            CE_FRAME = 0x0008,
            CE_BREAK = 0x0010,
            CE_TXFULL = 0x0100,
            CE_PTO = 0x0200,
            CE_IOE = 0x0400,
            CE_DNS = 0x0800,
            CE_OOP = 0x1000,
            CE_MODE = 0x8000
        }
    }
}
