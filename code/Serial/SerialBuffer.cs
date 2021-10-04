// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System;
    using System.Text;
    using Buffer.Memory;

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
    internal sealed class SerialBuffer : IDisposable
    {
        private readonly SerialReadBuffer m_ReadBuffer;
        private readonly SerialWriteBuffer m_WriteBuffer;

        public SerialBuffer(int readBuffer, int writeBuffer, Encoding encoding)
            : this(readBuffer, writeBuffer, encoding, false) { }

        public SerialBuffer(int readBuffer, int writeBuffer, Encoding encoding, bool pinned)
        {
            m_ReadBuffer = new SerialReadBuffer(readBuffer, encoding, pinned);
            m_WriteBuffer = new SerialWriteBuffer(writeBuffer, pinned);
        }

        public IReadBuffer SerialRead { get { return m_ReadBuffer; } }

        public IReadBufferStream ReadStream { get { return m_ReadBuffer; } }

        public IReadChars ReadChars { get { return m_ReadBuffer; } }

        public IWriteBuffer SerialWrite { get { return m_WriteBuffer; } }

        public IWriteBufferStream WriteStream { get { return m_WriteBuffer; } }

        public event EventHandler WriteStreamEvent
        {
            add { m_WriteBuffer.WriteEvent += value; }
            remove { m_WriteBuffer.WriteEvent -= value; }
        }

        public void Reset()
        {
            m_ReadBuffer.Reset();
            m_WriteBuffer.Reset();
        }

        public void Close()
        {
            m_ReadBuffer.DeviceDead();
            m_WriteBuffer.DeviceDead();
        }

        public void Dispose()
        {
            // This is a sealed class, so we have "private void" instead of "protected virtual"
            m_ReadBuffer.Dispose();
            m_WriteBuffer.Dispose();
        }
    }
}
