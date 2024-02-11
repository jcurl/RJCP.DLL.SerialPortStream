namespace RJCP.IO.Ports.Native.Unix
{
    using System;

    internal static partial class LibNSerial
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
