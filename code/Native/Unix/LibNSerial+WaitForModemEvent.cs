// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

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

