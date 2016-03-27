// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Unix
{
    using System;

    [Flags]
    internal enum SerialReadWriteEvent
    {
        Error = -1,
        NoEvent = 0,
        ReadEvent = 1,
        WriteEvent = 2,
        ReadWriteEvent = 3
    }
}
