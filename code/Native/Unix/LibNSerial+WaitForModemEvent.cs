namespace RJCP.IO.Ports.Native.Unix
{
    using System;

    internal static partial class LibNSerial
    {
        [Flags]
        public enum WaitForModemEvent
        {
            Error = -1,
            None = 0,
            DataCarrierDetect = 1,
            RingIndicator = 2,
            DataSetReady = 4,
            ClearToSend = 8
        }
    }
}

