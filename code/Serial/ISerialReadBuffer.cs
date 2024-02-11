namespace RJCP.IO.Ports.Serial
{
    using System;
    using Buffer.Memory;

    /// <summary>
    /// Interface for writing to the memory region that the user reads via stream APIs.
    /// </summary>
    public interface ISerialReadBuffer : IReadBuffer
    {
        /// <summary>
        /// Occurs when the user adds data to the buffer that we can send data out.
        /// </summary>
        event EventHandler<SerialBufferEventArgs> ReadEvent;
    }
}
