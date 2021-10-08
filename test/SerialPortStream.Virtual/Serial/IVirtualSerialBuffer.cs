// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System;

    /// <summary>
    /// Interface to access serial buffers from <see cref="VirtualNativeSerial"/> in a safe way.
    /// </summary>
    public interface IVirtualSerialBuffer
    {
        /// <summary>
        /// Occurs when the user requests to write data to the serial port.
        /// </summary>
        /// <remarks>
        /// Use this event for getting notified of a write event, instead of
        /// <see cref="Buffer"/><see cref="ISerialWriteBuffer.WriteEvent"/>, which can only be registered after a call
        /// to <see cref="INativeSerial.StartMonitor"/> (that would otherwise be impractical).
        /// </remarks>
        event EventHandler<SerialBufferEventArgs> WriteEvent;

        /// <summary>
        /// Occurs when the user has read data from the serial port.
        /// </summary>
        /// <remarks>
        /// Use this event for getting notified when the user has read from the pending read buffers, instead of
        /// <see cref="Buffer"/><see cref="ISerialReadBuffer.ReadEvent"/>, which can only be registered after a call to
        /// <see cref="INativeSerial.StartMonitor"/> (that would otherwise be impractical).
        /// </remarks>
        event EventHandler<SerialBufferEventArgs> ReadEvent;

        /// <summary>
        /// Gets the data from the serial port.
        /// </summary>
        /// <param name="buffer">The buffer to get what the user write to send to the low level interfaces.</param>
        /// <param name="offset">The offset into <paramref name="buffer"/>.</param>
        /// <param name="count">The number of bytes to copy from <paramref name="buffer"/>.</param>
        /// <returns>The actual number of bytes copied.</returns>
        /// <remarks>
        /// Use this method for getting data that is to be sent.
        /// </remarks>
        int ReadSentData(byte[] buffer, int offset, int count);

        /// <summary>
        /// Simulates data received by the serial port, allowing the stream to read data.
        /// </summary>
        /// <param name="buffer">The buffer containing the data that was received by the low level interfaces.</param>
        /// <param name="offset">The offset into <paramref name="buffer"/>.</param>
        /// <param name="count">The number of bytes to copy from <paramref name="buffer"/>.</param>
        /// <returns>The actual number of bytes copied.</returns>
        /// <remarks>
        /// Use this method for simulating received data.
        /// </remarks>
        int WriteReceivedData(byte[] buffer, int offset, int count);

        /// <summary>
        /// Gets the length of data that the user has sent to the serial port, that the virtual implementation can read.
        /// </summary>
        /// <value>
        /// The amount of data available to read, useful when calling with <see cref="ReadSentData(byte[], int, int)"/>.
        /// </value>
        int SentDataLength { get; }

        /// <summary>
        /// Gets the amount of free space remaining that the user can still send.
        /// </summary>
        /// <value>
        /// The amount of free space remaining that the user can still send, useful with
        /// <see cref="ReadSentData(byte[], int, int)"/>.
        /// </value>
        int SentDataFree { get; }

        /// <summary>
        /// Gets the amount of data available in the buffer that can be read.
        /// </summary>
        /// <value>
        /// The amount of data that can be read from the user after a call to
        /// <see cref="WriteReceivedData(byte[], int, int)"/>.
        /// </value>
        int ReceivedDataLength { get; }

        /// <summary>
        /// Gets the amount of bytes free in the read buffer.
        /// </summary>
        /// <value>The amount of bytes free in the read buffer.</value>
        /// <remarks>
        /// This method defines the number of bytes that can be written with a call to
        /// <see cref="WriteReceivedData(byte[], int, int)"/>.
        /// </remarks>
        int ReceivedDataFree { get; }
    }
}
