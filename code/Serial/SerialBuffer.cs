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
    /// This class is used to help implement streams with time out functionality when the actual object that reads and
    /// writes data doesn't support streams, but simply reads and writes data using low level APIs.
    /// <para>
    /// It is to be expected that this object is only used by one reader and one writer. The reader and writer may be
    /// different threads. That is, the properties under SerialData are accessed only by one thread, the properties
    /// under StreamData are accessed only by one single (but may be different to SerialData) thread.
    /// </para>
    /// <para>
    /// When this object is first instantiated, buffers are not allocated. This makes the properties
    /// <see cref="SerialRead"/>, <see cref="SerialWrite"/>, <see cref="ReadStream"/>, <see cref="WriteStream"/>,
    /// <see cref="ReadChars"/> initially return <see langword="null"/>. This allows the buffer sizes to be first set
    /// with the properties <see cref="ReadBufferSize"/> and <see cref="WriteBufferSize"/>. The buffers are first
    /// allocated to a call with <see cref="Reset"/>, which allocate the buffers, or reset the buffers to their original
    /// state. If the buffer sizes change after allocation, the <see cref="Reset"/> method will reallocate new buffers.
    /// Lazy allocation of buffers are not used to avoid unnecessary checking of allocation on potentially very
    /// frequently called methods.
    /// </para>
    /// </remarks>
    public sealed class SerialBuffer : IDisposable
    {
        private readonly bool m_Pinned;
        private SerialReadBuffer m_ReadBuffer;
        private SerialWriteBuffer m_WriteBuffer;
        private Encoding m_Encoding = Encoding.GetEncoding("UTF-8");

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialBuffer"/> class. When buffers are allocated, they are not
        /// pinned.
        /// </summary>
        public SerialBuffer() : this(false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialBuffer"/> class.
        /// </summary>
        /// <param name="pinned">
        /// Buffers will be pinned when allocated with a call to <see cref="Reset"/> if this is <see langword="true"/>.
        /// </param>
        public SerialBuffer(bool pinned)
        {
            m_Pinned = pinned;
        }

        /// <summary>
        /// Indicates if the buffers are allocated.
        /// </summary>
        /// <value>
        /// Is <see langword="true"/> if this instance has buffers allocated; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// Buffers are allocated automatically with a call to <see cref="Reset"/>. Buffers are not allocated
        /// automatically when this class is initialized as they can be modified first with <see cref="ReadBufferSize"/>
        /// and <see cref="WriteBufferSize"/>.
        /// </remarks>
        public bool IsBufferAllocated
        {
            get { return m_ReadBuffer != null; }
        }

        /// <summary>
        /// Gets or sets the byte encoding for pre- and post-transmission conversion of text.
        /// </summary>
        /// <remarks>
        /// The encoding is used for encoding string information to byte format when sending over the serial port, or
        /// receiving data via the serial port. It is only used with the read/write functions that accept strings (and
        /// not used for byte based reading and writing).
        /// </remarks>
        public Encoding Encoding
        {
            get
            {
                if (ReadChars != null) return ReadChars.Encoding;
                return m_Encoding;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Encoding));
                if (ReadChars != null) ReadChars.Encoding = value;
                m_Encoding = value;
            }
        }

        private int m_ReadBufferSize = 10048576;

        /// <summary>
        /// Gets or sets the size of the read buffer.
        /// </summary>
        /// <value>The size of the read buffer.</value>
        /// <remarks>
        /// The buffers are allocated on the call to <see cref="Reset"/>. If the buffers are already allocated (as
        /// determined by <see cref="IsBufferAllocated"/>), the changes don't take effect until the next call to
        /// <see cref="Reset"/>.
        /// </remarks>
        public int ReadBufferSize
        {
            get { return m_ReadBufferSize; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(SerialBuffer));
                if (m_ReadBuffer != null) throw new InvalidOperationException("Buffer already allocated");
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(ReadBufferSize), "Must be a positive integer");

                m_ReadBufferSize = (value < 1024) ? 1024 : value;
            }
        }

        private int m_WriteBufferSize = 131072;

        /// <summary>
        /// Gets or sets the size of the write buffer.
        /// </summary>
        /// <value>The size of the write buffer.</value>
        /// <remarks>
        /// The buffers are allocated on the call to <see cref="Reset"/>. If the buffers are already allocated (as
        /// determined by <see cref="IsBufferAllocated"/>), the changes don't take effect until the next call to
        /// <see cref="Reset"/>.
        /// </remarks>
        public int WriteBufferSize
        {
            get { return m_WriteBufferSize; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(SerialBuffer));
                if (m_WriteBuffer != null) throw new InvalidOperationException("Buffer already allocated");
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(WriteBufferSize), "Must be a positive integer");

                m_WriteBufferSize = (value < 1024) ? 1024 : value;
            }
        }

        /// <summary>
        /// Object with methods and properties for low level API to write to the buffer.
        /// </summary>
        /// <value>Object with methods and properties for low level API to write to the buffer.</value>
        /// <remarks>
        /// This object is normally used by low level implementations, such as classes implementing
        /// <see cref="INativeSerial"/>. This property is <see langword="null"/> until the first call to
        /// <see cref="Reset"/>, when the internal buffers are first allocated.
        /// </remarks>
        public IReadBuffer SerialRead { get { return m_ReadBuffer; } }

        /// <summary>
        /// Object with methods and properties for <see cref="System.IO.Stream"/> similar methods.
        /// </summary>
        /// <value>Object with methods and properties for <see cref="System.IO.Stream"/> similar methods.</value>
        /// <remarks>
        /// This object is used by the <see cref="SerialPortStream"/> implementation for read related methods, where low
        /// level API writes to the buffer with <see cref="SerialRead"/>, this object can report information to user
        /// applications. This property is <see langword="null"/> until the first call to <see cref="Reset"/>, when the
        /// internal buffers are first allocated.
        /// </remarks>
        public IReadBufferStream ReadStream { get { return m_ReadBuffer; } }

        /// <summary>
        /// Object with methods and properties for character based reading, specific to the
        /// <see cref="SerialPortStream"/>.
        /// </summary>
        /// <value>
        /// Object with methods and properties for character based reading, specific to the
        /// <see cref="SerialPortStream"/>.
        /// </value>
        /// <remarks>
        /// This object access the data written to buffers via the low-level API, and converts and manages the character
        /// based interpretations. Please note, this object contains the <see cref="IReadChars.Encoding"/> property and
        /// should not be set directly. Instead, set the encoding via the property on this object,
        /// <see cref="Encoding"/>. The reason for this, as this property is <see langword="null"/> until a call to
        /// <see cref="Reset"/> is called.
        /// </remarks>
        public IReadChars ReadChars { get { return m_ReadBuffer; } }

        /// <summary>
        /// Object with methods and properties for low level API to read from the buffer to write to device drivers.
        /// </summary>
        /// <value>
        /// Object with methods and properties for low level API to read from the buffer to write to device drivers.
        /// </value>
        /// <remarks>
        /// This object is used by low level implementations, such as classes implementing <see cref="INativeSerial"/>.
        /// This property is <see langword="null"/> until the first call to <see cref="Reset"/>, when the internal
        /// buffers are first allocated.
        /// </remarks>
        public IWriteBuffer SerialWrite { get { return m_WriteBuffer; } }

        /// <summary>
        /// Object with methods and properties for <see cref="System.IO.Stream"/> similar methods.
        /// </summary>
        /// <value>Object with methods and properties for <see cref="System.IO.Stream"/> similar methods.</value>
        /// <remarks>
        /// This object is used by the <see cref="SerialPortStream"/> implementation for write related methods, data is
        /// written to the buffer, and low level API reads from the buffer with <see cref="SerialWrite"/>, to write to
        /// low level drivers. This property is <see langword="null"/> until the first call to <see cref="Reset"/>, when
        /// the internal buffers are first allocated.
        /// </remarks>
        public IWriteBufferStream WriteStream { get { return m_WriteBuffer; } }

        /// <summary>
        /// Occurs when the user writes to the buffers via the <see cref="WriteStream"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="WriteStreamEvent"/> is intended for low level API code to know when there is data written to
        /// the write buffer. For some operating systems, this can help to interrupt an I/O loop to read data from the
        /// write buffer to send to the low level drivers.
        /// </remarks>
        public event EventHandler WriteStreamEvent
        {
            add { m_WriteBuffer.WriteEvent += value; }
            remove { m_WriteBuffer.WriteEvent -= value; }
        }

        /// <summary>
        /// Resets this instance. If buffers aren't allocated, they will be.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <remarks>
        /// Calling <see cref="Reset"/> will allocate the buffers, if not already allocated, or reset the buffers to
        /// their default state.
        /// </remarks>
        public void Reset()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(SerialBuffer));

            if (m_ReadBuffer != null && m_ReadBuffer.Buffer.Length == ReadBufferSize) {
                m_ReadBuffer.Reset();
            } else {
                m_ReadBuffer = new SerialReadBuffer(ReadBufferSize, m_Pinned) {
                    Encoding = Encoding
                };
            }

            if (m_WriteBuffer != null && m_WriteBuffer.Buffer.Length == WriteBufferSize) {
                m_WriteBuffer.Reset();
            } else {
                m_WriteBuffer = new SerialWriteBuffer(WriteBufferSize, m_Pinned);
            }
        }

        /// <summary>
        /// Closes this instance. Buffers are still available, so data can be read after closure.
        /// </summary>
        public void Close()
        {
            if (IsDisposed) return;

            if (m_ReadBuffer != null) m_ReadBuffer.DeviceDead();
            if (m_WriteBuffer != null) m_WriteBuffer.DeviceDead();
        }

        private bool IsDisposed { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting managed and/or unmanaged
        /// resources.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;

            if (m_ReadBuffer != null) m_ReadBuffer.Dispose();
            if (m_WriteBuffer != null) m_WriteBuffer.Dispose();
            m_ReadBuffer = null;
            m_WriteBuffer = null;
            IsDisposed = true;
        }
    }
}
