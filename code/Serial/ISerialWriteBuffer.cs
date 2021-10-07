// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System;
    using Buffer.Memory;

    /// <summary>
    /// Interface for reading the memory region that the user writes to via stream APIs.
    /// </summary>
    public interface ISerialWriteBuffer : IWriteBuffer
    {
        /// <summary>
        /// Occurs when the user adds data to the buffer that we can send data out.
        /// </summary>
        event EventHandler WriteEvent;
    }
}
