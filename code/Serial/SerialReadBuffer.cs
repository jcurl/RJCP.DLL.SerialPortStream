namespace RJCP.IO.Ports.Serial
{
    using System;
    using System.Text;
    using Buffer;

    /// <summary>
    /// Provides buffer management for reading from the serial port.
    /// </summary>
    /// <remarks>
    /// Extends functionality from <see cref="MemoryReadBuffer"/> to include character based processing.
    /// </remarks>
    public class SerialReadBuffer : MemoryReadBuffer, ISerialReadBuffer, IReadChars
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerialReadBuffer"/> class.
        /// </summary>
        /// <param name="length">The size of the buffer to allocate.</param>
        /// <param name="pinned">If set to <see langword="true" />, the buffers are pinned in memory.</param>
        public SerialReadBuffer(int length, bool pinned)
            : base(length, pinned) { }

        private Encoding m_Encoding;

        // Buffers for reading characters. We reserve one "char" at the end for 2-byte UTF16 sequences, to guarantee
        // guarantee we can always read into the cache. So you'll also see free space checks that we always need at
        // least two chars before conversion.
        private const int c_MaxLine = 1024;
        private readonly CircularBuffer<char> m_ReadCache = new(c_MaxLine + 1);
        private readonly CircularBuffer<int> m_ReadOffsets = new(c_MaxLine + 1);

        private int m_ReadOffset;    // Offset into byte buffer for next character
        private int m_LastChar;      // Position of m_ReadOffset on last successful character read

        // Overflow of the first character is used to know the precise state of the decoder in case we
        // need to discard the cache. We can reset the decoder, and know what the first byte was that we
        // read to ensure consistent behaviour, because after we read the first byte, we know the
        // decoder doesn't have any internal data cached any more.
        private int m_ReadOverflow = -1;         // Number of bytes to discard due to overflow
        private readonly char[] m_ReadOverflowChar = new char[2];   // First character that was lost
        private bool m_ReadOverflowUtf32;        // Indicates if two UTF16 overflowed or not.

        /// <summary>
        /// Occurs when the user adds data to the buffer that we can send data out.
        /// </summary>
        public event EventHandler<SerialBufferEventArgs> ReadEvent;

        private void OnReadEvent(object sender, SerialBufferEventArgs args)
        {
            EventHandler<SerialBufferEventArgs> handler = ReadEvent;
            if (handler is not null) {
                handler(sender, args);
            }
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
            get { return m_Encoding; }
            set
            {
                ThrowHelper.ThrowIfNull(value);
                m_Encoding = value;
                Decoder = m_Encoding.GetDecoder();
            }
        }

        private Decoder Decoder { get; set; }

        private bool PeekChar()
        {
            int readLen = ReadBuffer.Length;
            if (m_ReadOffset >= readLen) return false;
            if (m_ReadCache.Free <= 1) Overflow();

            char[] oneChar = new char[2];
            int cu = 0;
            while (cu == 0 && m_ReadOffset < readLen) {
                int bu;
                try {
                    // Some UTF8 sequences may result in two UTF16 characters being generated.
                    Decoder.Convert(ReadBuffer.Array, ReadBuffer.ToArrayIndex(m_ReadOffset), 1,
                        oneChar, 0, 1, false, out bu, out cu, out _);
                } catch (ArgumentException ex) {
                    if (!ex.ParamName.Equals("chars")) throw;
                    Decoder.Convert(ReadBuffer.Array, ReadBuffer.ToArrayIndex(m_ReadOffset), 1,
                        oneChar, 0, 2, false, out bu, out cu, out _);
                }
                m_ReadOffset += bu;
            }
            if (cu == 0) return false;

            m_ReadCache.Append(oneChar, 0, cu);
            m_ReadOffsets.Append(m_ReadOffset - m_LastChar);
            if (cu > 1) m_ReadOffsets.Append(0);
            m_LastChar = m_ReadOffset;
            return true;
        }

        private void Overflow()
        {
            // If we haven't overflowed our m_ReadCache yet, then we record the first character and its
            // offset. So if we have to reset the cache, we can reset the decoder and put that first
            // character into the read buffer.
            //
            // If we have already overflowed, then we just continue doing so. m_ReadOffset is the number
            // of bytes we've consumed independent of if we've overflowed or not.

            int consume = 1;
            if (m_ReadOverflow == -1) {
                m_ReadOverflowChar[0] = m_ReadCache[0];
                m_ReadOverflow = m_ReadOffsets[0];
                if (m_ReadOffsets[1] == 0) {
                    m_ReadOverflowUtf32 = true;
                    m_ReadOverflowChar[1] = m_ReadCache[1];
                    consume = 2;
                }
            }
            m_ReadCache.Consume(consume);
            m_ReadOffsets.Consume(consume);
        }

        private bool IsOverflowed
        {
            get { return m_ReadOverflow != -1; }
        }

        private bool IsCached
        {
            get { return m_ReadCache.Length != 0; }
        }

        private void CharReset(bool withOverflow)
        {
            m_ReadCache.Reset();
            m_ReadOffsets.Reset();
            Decoder.Reset();
            if (withOverflow) {
                m_ReadCache.Append(m_ReadOverflowChar, 0, m_ReadOverflowUtf32 ? 2 : 1);
                m_ReadOffsets.Append(m_ReadOverflow);
                if (m_ReadOverflowUtf32) m_ReadOffsets.Append(0);
                m_ReadOffset = m_ReadOverflow;
            } else {
                m_ReadOffset = 0;
            }
            m_LastChar = m_ReadOffset;
            m_ReadOverflow = -1;
            m_ReadOverflowUtf32 = false;
        }

        private void ReadToConsume(int chars)
        {
            int bytesRead = 0;
            for (int i = 0; i < chars; i++) {
                bytesRead += m_ReadOffsets[i];
            }
            m_LastChar -= bytesRead;
            m_ReadOffset -= bytesRead;
            m_ReadCache.Consume(chars);
            m_ReadOffsets.Consume(chars);
            ReadBuffer.Consume(bytesRead);
        }

        /// <summary>
        /// Waits for new data to arrive that can be used to recheck for new character data.
        /// </summary>
        /// <param name="timeout">The time out in milliseconds.</param>
        /// <returns><see langword="true"/> if one more byte is available since the last <see cref="ReadTo"/>
        /// call; <see langword="false"/> otherwise</returns>
        public bool WaitForReadChar(int timeout)
        {
            return WaitForRead(m_ReadOffset + 1, timeout);
        }

        /// <summary>
        /// Reads characters from the byte buffer into the character buffer, consuming data from the byte buffer.
        /// </summary>
        /// <param name="buffer">The character buffer to write to.</param>
        /// <param name="offset">The offset to write to in <paramref name="buffer"/>.</param>
        /// <param name="count">The number of characters to read into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read into <paramref name="buffer"/>.</returns>
        /// <remarks>
        /// Data is read from the read byte buffer converted using the decoder into <paramref name="buffer"/>.
        /// </remarks>
        public int Read(char[] buffer, int offset, int count)
        {
            lock (Lock) {
                try {
                    int chars = 0;
                    if (IsOverflowed) CharReset(true);
                    if (IsCached) {
                        chars = m_ReadCache.CopyTo(buffer, offset, count);
                        ReadToConsume(chars);
                        if (chars == count) return chars;
                    }

                    Decoder.Convert(ReadBuffer, buffer, offset + chars, count - chars,
                        false, out int bu, out int cu, out bool complete);
                    return chars + cu;
                } finally {
                    CheckBufferState(false);
                }
            }
        }

        /// <summary>
        /// Synchronously reads one character from the SerialPortStream input buffer.
        /// </summary>
        /// <returns>The character that was read. -1 indicates no data was available
        /// within the time out.</returns>
        public int ReadChar()
        {
            lock (Lock) {
                try {
                    char[] schar = new char[1];
                    if (IsOverflowed) CharReset(true);

                    bool dataAvailable = IsCached;
                    if (!IsCached) {
                        dataAvailable = PeekChar();
                    }

                    // Get the next byte from the cache, or put the next byte in the cache
                    if (dataAvailable) {
                        m_ReadCache.CopyTo(schar, 0, 1);
                        ReadToConsume(1);
                        return schar[0];
                    }
                } finally {
                    CheckBufferState(false);
                }
            }
            return -1;
        }

        private string m_ReadToString;

        private bool ReadToMatch(string text)
        {
            int bl = m_ReadCache.Length;
            int offset = bl - text.Length;
            if (offset < 0) return false;

            for (int i = 0; i < text.Length; i++) {
                if (m_ReadCache[i + offset] != text[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Reads from the cached and byte stream looking for the text specified.
        /// </summary>
        /// <param name="text">The text to indicate where the read operation stops.</param>
        /// <param name="line">On success, contains the line up to the text string requested.</param>
        /// <returns><see langword="true"/> if a line was found; <see langword="false"/> otherwise.</returns>
        public bool ReadTo(string text, out string line)
        {
            lock (Lock) {
                try {
                    bool changedText = !text.Equals(m_ReadToString);
                    if (changedText) {
                        m_ReadToString = text;
                        if (IsOverflowed) CharReset(true);

                        int readLen = m_ReadCache.Length;
                        if (readLen >= text.Length) {
                            // Check if the text already exists
                            string lbuffer = m_ReadCache.GetString();
                            int p = lbuffer.IndexOf(text, StringComparison.Ordinal);
                            if (p != -1) {
                                // It does exist, so consume up to the buffered portion
#if NETFRAMEWORK
                                line = lbuffer.Substring(0, p);
#else
                                line = lbuffer[..p];
#endif
                                int l = p + text.Length;
                                ReadToConsume(l);
                                return true;
                            }
                        }
                    }

                    while (!ReadToMatch(text)) {

                        // Decoders in .NET are designed for streams and not really for reading
                        // a little bit of data. By design, they are "greedy", they consume as much
                        // byte data as possible. The data that they consume is cached internally.
                        // Because it's not possible to ask the decoder only to decode if there
                        // is sufficient bytes, we have to keep account of how many bytes are
                        // consumed for each character.

                        bool newChar = PeekChar();
                        if (!newChar) {
                            // Didn't find the string and there's no new data.
                            line = null;
                            return false;
                        }
                    }

                    // Found the string
                    ReadBuffer.Consume(m_ReadOffset);
                    line = m_ReadCache.GetString(m_ReadCache.Length - text.Length);
                    CharReset(false);
                    return true;
                } finally {
                    CheckBufferState(false);
                }
            }
        }

        /// <summary>
        /// Reads all immediately available bytes.
        /// </summary>
        /// <returns>The contents of the stream and the input buffer of the SerialPortStream.</returns>
        public string ReadExisting()
        {
            try {
                StringBuilder sb = new();
                if (IsOverflowed) CharReset(true);
                sb.Append(m_ReadCache.GetString());
                CharReset(false);

                lock (Lock) {
                    do {
                        char[] c = new char[2048];
                        Decoder.Convert(ReadBuffer, c, 0, c.Length, false, out int bu, out int cu, out bool complete);
                        sb.Append(c, 0, cu);
                    } while (ReadBuffer.Length > 0);
                }
                return sb.ToString();
            } finally {
                CheckBufferState(false);
            }
        }

        /// <summary>
        /// Called when a read operation is finished, that derived classes can perform additional actions.
        /// </summary>
        /// <param name="bytes">The number of bytes that were just read and consumed.</param>
        protected override void OnRead(int bytes)
        {
            if (bytes > 0) {
                CharReset(false);
                OnReadEvent(this, new SerialBufferEventArgs(bytes));
            }
        }

        /// <summary>
        /// Called when <see cref="MemoryReadBuffer.Reset" /> is requested.
        /// </summary>
        /// <remarks>Allows a safe way that derived classes can reset their state.</remarks>
        protected override void OnReset()
        {
            m_ReadCache.Reset();
            m_ReadOffsets.Reset();
            m_ReadOffset = 0;
            m_LastChar = 0;
            m_ReadOverflow = -1;
        }
    }
}
