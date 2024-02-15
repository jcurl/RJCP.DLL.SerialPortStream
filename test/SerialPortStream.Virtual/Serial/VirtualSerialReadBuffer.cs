namespace RJCP.IO.Ports.Serial
{
    using System;

    /// <summary>
    /// Read buffer management for the <see cref="VirtualNativeSerial"/> implementation.
    /// </summary>
    public class VirtualSerialReadBuffer : SerialReadBuffer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualSerialReadBuffer"/> class.
        /// </summary>
        /// <param name="length">The size of the buffer to allocate.</param>
        public VirtualSerialReadBuffer(int length)
            : base(length, false) { }

        /// <summary>
        /// Append data into the buffer that the user can read from.
        /// </summary>
        /// <param name="buffer">The buffer containing the data that was received by the low level interfaces.</param>
        /// <param name="offset">The offset into <paramref name="buffer"/>.</param>
        /// <param name="count">The number of bytes to copy from <paramref name="buffer"/>.</param>
        /// <returns>The actual number of bytes copied.</returns>
        public int WriteReceivedData(byte[] buffer, int offset, int count)
        {
            lock (Lock) {
                int length = ReadBuffer.Append(buffer, offset, count);
                CheckBufferState(true);
                OnDataReceived(this, new SerialDataReceivedEventArgs(SerialData.Chars));
                return length;
            }
        }

        /// <summary>
        /// Occurs when data is received, immediately when a write occurs with
        /// <see cref="WriteReceivedData(byte[], int, int)"/>.
        /// </summary>
        public event EventHandler<SerialDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Handles the <see cref="DataReceived" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SerialDataReceivedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            EventHandler<SerialDataReceivedEventArgs> handler = DataReceived;
            if (handler is not null) handler(sender, args);
        }

        /// <summary>
        /// Gets the amount of data available in the buffer that can be read.
        /// </summary>
        /// <value>
        /// The amount of data that can be read from the user after a call to
        /// <see cref="WriteReceivedData(byte[], int, int)"/>.
        /// </value>
        public int ReceivedDataLength
        {
            get
            {
                lock (Lock) {
                    return ReadBuffer.Length;
                }
            }
        }

        /// <summary>
        /// Gets the amount of bytes free in the read buffer.
        /// </summary>
        /// <value>The amount of bytes free in the read buffer.</value>
        /// <remarks>
        /// This method defines the number of bytes that can be written with a call to
        /// <see cref="WriteReceivedData(byte[], int, int)"/>.
        /// </remarks>
        public int ReceivedDataFree
        {
            get
            {
                lock (Lock) {
                    return ReadBuffer.Free;
                }
            }
        }
    }
}
