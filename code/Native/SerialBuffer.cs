// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using Datastructures;

    /// <summary>
    /// Manages two buffers, for reading and writing, between a Stream and a Native Serial object.
    /// </summary>
    /// <remarks>
    /// This class is used to help implement streams with time out functionality when the actual
    /// object that reads and writes data doesn't support streams, but simply reads and writes
    /// data using low level APIs.
    /// <para>It is to be expected that this object is only used by one reader and one writer. The
    /// reader and writer may be different threads. That is, the properties under SerialData are
    /// accessed only by one thread, the properties under StreamData are accessed only by one
    /// single (but may be different to SerialData) thread.</para>
    /// </remarks>
    internal class SerialBuffer : IDisposable
    {
        private CircularBuffer<byte> m_ReadBuffer;
        private readonly GCHandle m_ReadHandle;
        private readonly object m_ReadLock = new object();
        private readonly ManualResetEvent m_ReadBufferNotEmptyEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent m_ReadBufferNotFullEvent = new ManualResetEvent(true);
        private readonly ManualResetEvent m_ReadEvent = new ManualResetEvent(false);

        private CircularBuffer<byte> m_WriteBuffer;
        private readonly GCHandle m_WriteHandle;
        private readonly object m_WriteLock = new object();
        private readonly ManualResetEvent m_WriteBufferNotFullEvent = new ManualResetEvent(true);
        private readonly ManualResetEvent m_WriteBufferNotEmptyEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent m_TxEmptyEvent = new ManualResetEvent(true);

        private readonly AutoResetEvent m_AbortWriteEvent = new AutoResetEvent(false);

        private readonly bool m_Pinned;

        /// <summary>
        /// Container structure for properties and methods related to the Native Serial object.
        /// </summary>
        public struct SerialData
        {
            private readonly SerialBuffer m_SerialBuffer;

            /// <summary>
            /// Initializes a new instance of the <see cref="SerialData"/> struct.
            /// </summary>
            /// <param name="parent">The parent.</param>
            internal SerialData(SerialBuffer parent)
            {
                m_SerialBuffer = parent;
            }

            /// <summary>
            /// Access the read buffer directly.
            /// </summary>
            /// <value>
            /// The read buffer.
            /// </value>
            public CircularBuffer<byte> ReadBuffer { get { return m_SerialBuffer.m_ReadBuffer; } }

            /// <summary>
            /// Gets the read buffer offset end, used for giving to native API's.
            /// </summary>
            /// <value>
            /// The read buffer offset end.
            /// </value>
            public IntPtr ReadBufferOffsetEnd
            {
                get {
                    return !m_SerialBuffer.m_Pinned ?
                        IntPtr.Zero :
                        m_SerialBuffer.m_ReadHandle.AddrOfPinnedObject() + m_SerialBuffer.m_ReadBuffer.End;
                }
            }

            /// <summary>
            /// Access the write buffer directly.
            /// </summary>
            /// <value>
            /// The write buffer.
            /// </value>
            public CircularBuffer<byte> WriteBuffer { get { return m_SerialBuffer.m_WriteBuffer; } }

            /// <summary>
            /// Gets the write buffer offset start, used for giving to native API's.
            /// </summary>
            /// <value>
            /// The write buffer offset start.
            /// </value>
            public IntPtr WriteBufferOffsetStart
            {
                get
                {
                    return !m_SerialBuffer.m_Pinned ?
                        IntPtr.Zero :
                        m_SerialBuffer.m_WriteHandle.AddrOfPinnedObject() + m_SerialBuffer.m_WriteBuffer.Start;
                }
            }

            /// <summary>
            /// Update the read circular queue indicating data has been received, and is available to the stream object.
            /// </summary>
            /// <param name="count">The count.</param>
            public void ReadBufferProduce(int count)
            {
                lock (m_SerialBuffer.m_ReadLock) {
                    m_SerialBuffer.m_ReadBuffer.Produce(count);
                    m_SerialBuffer.m_ReadBufferNotEmptyEvent.Set();
                    m_SerialBuffer.m_ReadEvent.Set();
                    if (m_SerialBuffer.m_ReadBuffer.Free == 0) {
                        m_SerialBuffer.m_ReadBufferNotFullEvent.Reset();
                    }
                }
            }

            /// <summary>
            /// Update the write circular queue indicating data has been written, freeing space for more data to write.
            /// </summary>
            /// <param name="count">The count.</param>
            public void WriteBufferConsume(int count)
            {
                lock (m_SerialBuffer.m_WriteLock) {
                    m_SerialBuffer.m_WriteBuffer.Consume(count);
                    m_SerialBuffer.m_WriteBufferNotFullEvent.Set();
                    if (m_SerialBuffer.m_WriteBuffer.Length == 0) {
                        m_SerialBuffer.m_WriteBufferNotEmptyEvent.Reset();
                    }
                }
            }

            /// <summary>
            /// Indicates that the write buffer is now empty.
            /// </summary>
            /// <returns><c>true</c> if the write buffer is empty.</returns>
            /// <remarks>
            /// Systems that return immediately after a Write before the write has been sent over the wire
            /// should call this method when the hardware indicates that data is flushed.
            /// <para>Systems that only return from the Write when data is completely written, or that do not
            /// receive events when the hardware buffer is empty, should call this method immediately after
            /// the Write is done.</para><para>Typically Windows systems may return before the hardware buffer is completely empty,
            /// where they notify later of an empty buffer with the EV_TXEMPTY event.</para>
            /// </remarks>
            public bool TxEmptyEvent()
            {
                lock (m_SerialBuffer.m_WriteLock) {
                    if (m_SerialBuffer.m_WriteBuffer.Length == 0) {
                        m_SerialBuffer.m_TxEmptyEvent.Set();
                        return true;
                    }
                    return false;
                }
            }

            /// <summary>
            /// Gets a value indicating whether this instance uses a pinned buffer.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance uses a pinned buffer; otherwise, <c>false</c>.
            /// </value>
            public bool IsPinnedBuffer
            {
                get { return m_SerialBuffer.m_Pinned; }
            }

            /// <summary>
            /// Gets the event handle that is signalled when the read buffer is not full.
            /// </summary>
            /// <value>
            /// The event handle that is signalled when the read buffer is not full.
            /// </value>
            public WaitHandle ReadBufferNotFull
            {
                get { return m_SerialBuffer.m_ReadBufferNotFullEvent; }
            }

            /// <summary>
            /// Gets the event handle that is signalled when data is in the write buffer.
            /// </summary>
            /// <value>
            /// The event handle that is signalled when data is in the write buffer.
            /// </value>
            public WaitHandle WriteBufferNotEmpty
            {
                get { return m_SerialBuffer.m_WriteBufferNotEmptyEvent; }
            }

            /// <summary>
            /// Purges the write buffer.
            /// </summary>
            public void Purge()
            {
                lock (m_SerialBuffer.m_WriteLock) {
                    m_SerialBuffer.m_WriteBuffer.Reset();
                    m_SerialBuffer.m_WriteBufferNotEmptyEvent.Reset();
                    m_SerialBuffer.m_WriteBufferNotFullEvent.Set();
                    m_SerialBuffer.m_TxEmptyEvent.Set();
                }
            }
        }

        /// <summary>
        /// Container structure for properties and methods related to the Stream object.
        /// </summary>
        public struct StreamData
        {
            private readonly SerialBuffer m_SerialBuffer;

            /// <summary>
            /// Initializes a new instance of the <see cref="StreamData"/> struct.
            /// </summary>
            /// <param name="parent">The parent.</param>
            internal StreamData(SerialBuffer parent)
            {
                m_SerialBuffer = parent;
            }

            /// <summary>
            /// Waits up to a specified time out for data to be available to read.
            /// </summary>
            /// <param name="timeout">The time out in milliseconds.</param>
            /// <returns><c>true</c> if data is available to read in time; <c>false</c> otherwise.</returns>
            public bool WaitForRead(int timeout)
            {
                return m_SerialBuffer.m_ReadBufferNotEmptyEvent.WaitOne(timeout);
            }

            /// <summary>
            /// Waits up to a specified time out for data to be available to read.
            /// </summary>
            /// <param name="count">The number of bytes that should be in the read buffer.</param>
            /// <param name="timeout">The time out in milliseconds.</param>
            /// <returns><c>true</c> if data is available to read in time; <c>false</c> otherwise.</returns>
            public bool WaitForRead(int count, int timeout)
            {
                if (count == 0) return true;
                lock (m_SerialBuffer.m_ReadLock) {
                    if (count > m_SerialBuffer.m_ReadBuffer.Capacity) return false;
                }

                TimerExpiry timer = new TimerExpiry(timeout);
                do {
                    lock (m_SerialBuffer.m_ReadLock) {
                        if (m_SerialBuffer.m_ReadBuffer.Length >= count) return true;
                        m_SerialBuffer.m_ReadEvent.Reset();
                    }
                    if (m_SerialBuffer.m_ReadEvent.WaitOne(timer.RemainingTime())) {
                        return true;
                    }
                } while (!timer.Expired);
                return false;
            }

            /// <summary>
            /// Reads data received by the Serial object, copying it into a buffer and reducing the read buffer size.
            /// </summary>
            /// <param name="buffer">The buffer to copy data into.</param>
            /// <param name="offset">The offset where to copy data into..</param>
            /// <param name="count">The number of bytes to copy.</param>
            /// <returns>Number of bytes actually read from the queue.</returns>
            public int Read(byte[] buffer, int offset, int count)
            {
                lock (m_SerialBuffer.m_ReadLock) {
                    int bytes = m_SerialBuffer.m_ReadBuffer.MoveTo(buffer, offset, count);
                    if (m_SerialBuffer.m_ReadBuffer.Length == 0) {
                        m_SerialBuffer.m_ReadBufferNotEmptyEvent.Reset();
                    }
                    m_SerialBuffer.m_ReadBufferNotFullEvent.Set();
                    return bytes;
                }
            }

            /// <summary>
            /// Consume bytes from the incoming buffer.
            /// </summary>
            /// <param name="count">The number of bytes to discard at the beginning of the read byte buffer.</param>
            public void ReadConsume(int count)
            {
                lock (m_SerialBuffer.m_ReadLock) {
                    m_SerialBuffer.m_ReadBuffer.Consume(count);
                    if (m_SerialBuffer.m_ReadBuffer.Length == 0) {
                        m_SerialBuffer.m_ReadBufferNotEmptyEvent.Reset();
                    }
                    m_SerialBuffer.m_ReadBufferNotFullEvent.Set();
                }
            }

            /// <summary>
            /// Reads data received by the serial object, converted to characters using the specified decoder.
            /// </summary>
            /// <param name="buffer">The character buffer to read into.</param>
            /// <param name="offset">The offset into <paramref name="buffer"/>.</param>
            /// <param name="count">The number of characters to write into <paramref name="buffer"/>.</param>
            /// <param name="decoder">The decoder to use for the conversion.</param>
            /// <returns>The number of characters read.</returns>
            /// <remarks>
            /// This method has no input checks that it is internal
            /// </remarks>
            public int Read(char[] buffer, int offset, int count, Decoder decoder)
            {
                int bu;
                int cu;
                bool complete;
                lock (m_SerialBuffer.m_ReadLock) {
                    decoder.Convert(m_SerialBuffer.m_ReadBuffer, buffer, offset, count, false, out bu, out cu, out complete);
                    if (m_SerialBuffer.m_ReadBuffer.Length == 0) {
                        m_SerialBuffer.m_ReadBufferNotEmptyEvent.Reset();
                    }
                    m_SerialBuffer.m_ReadBufferNotFullEvent.Set();
                    return cu;
                }
            }

            /// <summary>
            /// Reads a single byte from the input queue.
            /// </summary>
            /// <returns>The byte, cast to an Int32, or -1 if the end of the stream has been read.</returns>
            public int ReadByte()
            {
                lock (m_SerialBuffer.m_ReadLock) {
                    if (m_SerialBuffer.m_ReadBuffer.Length == 0) return -1;
                    int v = m_SerialBuffer.m_ReadBuffer[0];
                    m_SerialBuffer.m_ReadBuffer.Consume(1);
                    if (m_SerialBuffer.m_ReadBuffer.Length == 0) {
                        m_SerialBuffer.m_ReadBufferNotEmptyEvent.Reset();
                    }
                    m_SerialBuffer.m_ReadBufferNotFullEvent.Set();
                    return v;
                }
            }

            /// <summary>
            /// Gets the number of bytes in the internal read buffer only.
            /// </summary>
            /// <remarks>
            /// This value is independent of the actual number of bytes in the serial port
            /// hardware buffer. It will only return that which is currently obtained by
            /// the I/O thread.
            /// </remarks>
            public int BytesToRead
            {
                get
                {
                    lock (m_SerialBuffer.m_ReadLock) {
                        return m_SerialBuffer.m_ReadBuffer.Length;
                    }
                }
            }

            /// <summary>
            /// Discards data from the receive buffer.
            /// </summary>
            public void DiscardInBuffer()
            {
                lock (m_SerialBuffer.m_ReadLock) {
                    m_SerialBuffer.m_ReadBuffer.Consume(m_SerialBuffer.m_ReadBuffer.Length);
                    m_SerialBuffer.m_ReadBufferNotFullEvent.Set();
                    m_SerialBuffer.m_ReadBufferNotEmptyEvent.Reset();
                }
            }

            /// <summary>
            /// Waits up to a specified time out for enough data to be free in the write buffer.
            /// </summary>
            /// <param name="count">The number of bytes required to be free.</param>
            /// <param name="timeout">The time out in milliseconds.</param>
            /// <returns><c>true</c> if <paramref name="count"/> bytes are available for writing to the buffer;
            /// <c>false</c> if there is not enough buffer available within the <paramref name="timeout"/>
            /// parameter. If <paramref name="count"/> is larger than the capacity of the buffer, <c>false</c>
            /// is returned immediately.</returns>
            public bool WaitForWrite(int count, int timeout)
            {
                if (count == 0) return true;
                lock (m_SerialBuffer.m_WriteLock) {
                    if (count > m_SerialBuffer.m_WriteBuffer.Capacity) return false;
                }

                m_SerialBuffer.m_AbortWriteEvent.Reset();
                TimerExpiry timer = new TimerExpiry(timeout);
                do {
                    lock (m_SerialBuffer.m_WriteLock) {
                        if (m_SerialBuffer.m_WriteBuffer.Free >= count) return true;
                        m_SerialBuffer.m_WriteBufferNotFullEvent.Reset();
                    }
                    // This manual reset event is always set every time data is removed from the buffer
                    WaitHandle[] handles = new WaitHandle[] { m_SerialBuffer.m_AbortWriteEvent, m_SerialBuffer.m_WriteBufferNotFullEvent };
                    int triggered = WaitHandle.WaitAny(handles, timer.RemainingTime());
                    switch (triggered) {
                    case WaitHandle.WaitTimeout:
                        break;
                    case 0:
                        // Someone aborted the wait.
                        return false;
                    case 1:
                        // Data is available to write
                        return true;
                    }
                } while (!timer.Expired);
                return false;
            }

            /// <summary>
            /// Aborts the wait for write.
            /// </summary>
            public void AbortWaitForWrite()
            {
                m_SerialBuffer.m_AbortWriteEvent.Set();
            }

            /// <summary>
            /// Puts data into the write buffer for the Serial object to send.
            /// </summary>
            /// <param name="buffer">The buffer to copy data from.</param>
            /// <param name="offset">The offset where to copy the data from.</param>
            /// <param name="count">The number of bytes to copy.</param>
            /// <returns>Number of bytes actually written to the queue.</returns>
            public int Write(byte[] buffer, int offset, int count)
            {
                lock (m_SerialBuffer.m_WriteLock) {
                    int bytes = m_SerialBuffer.m_WriteBuffer.Append(buffer, offset, count);
                    m_SerialBuffer.m_WriteBufferNotEmptyEvent.Set();
                    m_SerialBuffer.m_TxEmptyEvent.Reset();
                    if (m_SerialBuffer.m_WriteBuffer.Free == 0) {
                        m_SerialBuffer.m_WriteBufferNotFullEvent.Reset();
                    }
                    m_SerialBuffer.OnWriteEvent(m_SerialBuffer, new EventArgs());
                    return bytes;
                }
            }

            /// <summary>
            /// Gets the number of bytes in the internal write buffer only.
            /// </summary>
            /// <remarks>
            /// This value is independent of the actual number of bytes in the serial port
            /// hardware buffer. It will only return that which is currently not completely written
            /// by the I/O thread.
            /// </remarks>
            public int BytesToWrite
            {
                get
                {
                    lock (m_SerialBuffer.m_WriteLock) {
                        return m_SerialBuffer.m_WriteBuffer.Length;
                    }
                }
            }

            /// <summary>
            /// Waits for all data in the write buffer to be written with notification also by the serial buffer.
            /// </summary>
            /// <param name="timeout">The time out in milliseconds.</param>
            /// <returns><c>true</c> if the data was flushed within the specified time out; <c>false</c> otherwise.</returns>
            public bool Flush(int timeout)
            {
                // This manual reset event is always set every time data is removed from the buffer
                m_SerialBuffer.m_AbortWriteEvent.Reset();
                WaitHandle[] handles = new WaitHandle[] { m_SerialBuffer.m_AbortWriteEvent, m_SerialBuffer.m_TxEmptyEvent };
                int triggered = WaitHandle.WaitAny(handles, timeout);
                switch (triggered) {
                case WaitHandle.WaitTimeout:
                    return false;
                case 0:
                    // Someone aborted the wait.
                    return false;
                case 1:
                    // Data is available to write
                    return true;
                }
                throw new ApplicationException("Unexpected code flow");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialBuffer"/> class, where the buffer is not pinned.
        /// </summary>
        /// <param name="readBuffer">The read buffer size in bytes.</param>
        /// <param name="writeBuffer">The write buffer size in bytes.</param>
        /// <remarks>
        /// Allocates buffer space for reading and writing (accessible via the <see cref="Serial"/> and <see cref="Stream"/>
        /// properties). The buffers are not pinned, meaning they should not be used for native methods (unless pinned explicitly).
        /// </remarks>
        public SerialBuffer(int readBuffer, int writeBuffer) : this(readBuffer, writeBuffer, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialBuffer"/> class.
        /// </summary>
        /// <param name="readBuffer">The read buffer size in bytes.</param>
        /// <param name="writeBuffer">The write buffer size in bytes.</param>
        /// <param name="pinned">if set to <c>true</c> then the read and write buffers are pinned.</param>
        /// <remarks>
        /// Allocates buffer space for reading and writing (accessible via the <see cref="Serial"/> and <see cref="Stream"/>
        /// properties). If specified, the buffers can be pinned. THis causes the buffers to not move for the duration of
        /// the program. The usual warnings apply with pinning buffers, you must be careful to avoid memory problems as
        /// the GC will not be able to reallocate or free space. Note, you must pin the buffers if you intend to use
        /// overlapped I/O.
        /// </remarks>
        public SerialBuffer(int readBuffer, int writeBuffer, bool pinned)
        {
            if (pinned) {
                byte[] read = new byte[readBuffer];
                m_ReadHandle = GCHandle.Alloc(read, GCHandleType.Pinned);
                m_ReadBuffer = new CircularBuffer<byte>(read, 0);

                byte[] write = new byte[writeBuffer];
                m_WriteHandle = GCHandle.Alloc(write, GCHandleType.Pinned);
                m_WriteBuffer = new CircularBuffer<byte>(write, 0);
            } else {
                m_ReadBuffer = new CircularBuffer<byte>(readBuffer);
                m_WriteBuffer = new CircularBuffer<byte>(writeBuffer);
            }
            m_Pinned = pinned;

            Serial = new SerialData(this);
            Stream = new StreamData(this);
        }

        /// <summary>
        /// Access to properties and methods specific to the native serial object.
        /// </summary>
        /// <value>
        /// Access to properties and methods specific to the native serial object.
        /// </value>
        public SerialData Serial { get; private set; }

        /// <summary>
        /// Access to properties and methods specific to the stream object.
        /// </summary>
        /// <value>
        /// Access to properties and methods specific to the stream object.
        /// </value>
        public StreamData Stream { get; private set; }

        /// <summary>
        /// Object to use for locking access to the byte read buffer.
        /// </summary>
        /// <value>
        /// The object to use for locking access to the byte read buffer.
        /// </value>
        public object ReadLock { get { return m_ReadLock; } }

        /// <summary>
        /// Object to use for locking access to the byte write buffer.
        /// </summary>
        /// <value>
        /// The object to use for locking access to the byte write buffer.
        /// </value>
        public object WriteLock { get { return m_WriteLock; } }

        private void OnWriteEvent(object sender, EventArgs args)
        {
            EventHandler handler = WriteEvent;
            if (handler != null) {
                handler(sender, args);
            }
        }

        /// <summary>
        /// Event raised when data is available to write. This could be used to
        /// abort an existing connection.
        /// </summary>
        public event EventHandler WriteEvent;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // This is a sealed class, so we have "private void" instead of "protected virtual"
            if (disposing) {
                if (m_Pinned) {
                    // Dispose managed objects here.
                    m_ReadHandle.Free();
                    m_WriteHandle.Free();
                }
                m_ReadBufferNotEmptyEvent.Dispose();
                m_ReadBufferNotFullEvent.Dispose();
                m_ReadEvent.Dispose();
                m_WriteBufferNotFullEvent.Dispose();
                m_WriteBufferNotEmptyEvent.Dispose();
                m_TxEmptyEvent.Dispose();
                m_AbortWriteEvent.Dispose();
                m_ReadBuffer = null;
                m_WriteBuffer = null;
            }
        }
    }
}
