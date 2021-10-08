// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    /// <summary>
    /// Write buffer management for the <see cref="VirtualNativeSerial"/> implementation.
    /// </summary>
    public class VirtualSerialWriteBuffer : SerialWriteBuffer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualSerialWriteBuffer"/> class.
        /// </summary>
        /// <param name="length">The size of the buffer to allocate.</param>
        public VirtualSerialWriteBuffer(int length)
            : base(length, false) { }

        /// <summary>
        /// Read and consume data into the buffer that the user wrote to.
        /// </summary>
        /// <param name="buffer">The buffer to get what the user write to send to the low level interfaces.</param>
        /// <param name="offset">The offset into <paramref name="buffer" />.</param>
        /// <param name="count">The number of bytes to copy from <paramref name="buffer" />.</param>
        /// <returns>The actual number of bytes copied.</returns>
        public int ReadSentData(byte[] buffer, int offset, int count)
        {
            lock (Lock) {
                int bytes = WriteBuffer.CopyTo(buffer, offset, count);
                Consume(bytes);       // Tells the SerialPortStream that data was now read
                return bytes;
            }
        }

        /// <summary>
        /// Gets the length of data that the user has sent to the serial port, that the virtual implementation can read.
        /// </summary>
        /// <value>
        /// The amount of data available to read, useful when calling with <see cref="ReadSentData(byte[], int, int)"/>.
        /// </value>
        public int SentDataLength
        {
            get
            {
                lock (Lock) {
                    return WriteBuffer.Length;
                }
            }
        }

        /// <summary>
        /// Gets the amount of free space remaining that the user can still send.
        /// </summary>
        /// <value>
        /// The amount of free space remaining that the user can still send, useful with
        /// <see cref="ReadSentData(byte[], int, int)"/>.
        /// </value>
        public int SentDataFree
        {
            get
            {
                lock (Lock) {
                    return WriteBuffer.Free;
                }
            }
        }
    }
}
