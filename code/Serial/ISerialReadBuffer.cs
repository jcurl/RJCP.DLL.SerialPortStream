// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using Buffer.Memory;

    /// <summary>
    /// Interface for writing to the memory region that the user reads via stream APIs.
    /// </summary>
    public interface ISerialReadBuffer : IReadBuffer
    {
    }
}
