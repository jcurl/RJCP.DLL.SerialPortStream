// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System;
    using Buffer;

    internal sealed class SerialWriteBuffer : MemoryWriteBuffer, ISerialWriteBuffer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerialWriteBuffer"/> class.
        /// </summary>
        /// <param name="length">The size of the buffer to allocate.</param>
        /// <param name="pinned">If set to <see langword="true" />, the buffers are pinned in memory.</param>
        public SerialWriteBuffer(int length, bool pinned)
            : base(length, pinned) { }

        /// <summary>
        /// Occurs when the user adds data to the buffer that we can send data out.
        /// </summary>
        public event EventHandler<SerialBufferEventArgs> WriteEvent;

        private void OnWriteEvent(object sender, SerialBufferEventArgs args)
        {
            EventHandler<SerialBufferEventArgs> handler = WriteEvent;
            if (handler != null) {
                handler(sender, args);
            }
        }

        /// <summary>
        /// Called when the user wants to write.
        /// </summary>
        /// <param name="bytes">The number of bytes the user wrote to the buffer.</param>
        /// <remarks>
        /// The write occurs from the user layer, but the driver layer may need to be notified. Override this class and
        /// provide your own implementation for notification.
        /// </remarks>
        protected override void OnWrite(int bytes)
        {
            if (bytes > 0) {
                OnWriteEvent(this, new SerialBufferEventArgs(bytes));
            }
        }
    }
}
