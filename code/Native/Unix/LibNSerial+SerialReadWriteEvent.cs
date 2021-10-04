// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Unix
{
    using System;

    internal partial class LibNSerial
    {
        [Flags]
        public enum SerialReadWriteEvent
        {
            Error = -1,
            NoEvent = 0,
            ReadEvent = 1,
            WriteEvent = 2,
            ReadWriteEvent = ReadEvent + WriteEvent
        }
    }
}
