// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.SerialPortStreamTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using Datastructures;
    using Native;

    [TestFixture]
    public class ReadToTest
    {
        public class ReadToCache
        {
            // Buffers for reading characters. We reserve one "char" at the end for 2-byte UTF16 sequences, to guarantee
            // guarantee we can always read into the cache. So you'll also see free space checks that we always need at
            // least two chars before conversion.
            private const int c_MaxLine = 1024;
            private readonly CircularBuffer<char> m_ReadCache = new CircularBuffer<char>(c_MaxLine + 1);
            private readonly CircularBuffer<int> m_ReadOffsets = new CircularBuffer<int>(c_MaxLine + 1);

            private int m_ReadOffset;    // Offset into byte buffer for next character
            private int m_LastChar;      // Position of m_ReadOffset on last successful character read

            // Overflow of the first character is used to know the precise state of the decoder in case we
            // need to discard the cache. We can reset the decoder, and know what the first byte was that we
            // read to ensure consistent behaviour, because after we read the first byte, we know the
            // decoder doesn't have any internal data cached anymore.
            private int m_ReadOverflow = -1;         // Number of bytes to discard due to overflow
            private readonly char[] m_ReadOverflowChar = new char[2];   // First character that was lost
            private bool m_ReadOverflowUtf32;        // Indicates if two UTF16 overflowed or not.

            private Encoding m_Encoding = Encoding.GetEncoding("UTF-8");
            private Decoder m_Decoder;

            /// <summary>
            /// Gets or sets the byte encoding for pre- and post-transmission conversion of text.
            /// </summary>
            /// <remarks>
            /// The encoding is used for encoding string information to byte format when sending
            /// over the serial port, or receiving data via the serial port. It is only used
            /// with the read/write functions that accept strings (and not used for byte based
            /// reading and writing).
            /// </remarks>
            public Encoding Encoding
            {
                get { return m_Encoding; }
                set
                {
                    if (value != null) {
                        m_Encoding = value;
                        m_Decoder = null;
                    } else {
                        throw new ArgumentNullException("value", "Encoding may not be null");
                    }
                }
            }

            private Decoder Decoder
            {
                get
                {
                    if (m_Encoding != null) {
                        if (m_Decoder == null) m_Decoder = m_Encoding.GetDecoder();
                        return m_Decoder;
                    } else {
                        return null;
                    }
                }
            }

            /// <summary>
            /// Reads a single character from the byte buffer provided, putting it into the internal cache
            /// </summary>
            /// <param name="buffer">The buffer to read.</param>
            /// <returns><c>true</c> if a character was found; <c>false</c> otherwise.</returns>
            /// <exception cref="System.ArgumentNullException">buffer may not be null.</exception>
            /// <remarks>
            /// It is expected that this method be used iteratively to find a single character. 
            /// </remarks>
            public bool PeekChar(CircularBuffer<byte> buffer)
            {
                int bytesRead = 0;
                if (buffer == null) throw new ArgumentNullException("buffer");
                int readlen = buffer.Length;
                if (m_ReadOffset >= readlen) return false;

                char[] oneChar = new char[2];
                int cu = 0;
                while (cu == 0 && m_ReadOffset < readlen) {
                    int bu; bool complete;
                    try {
                        // Some UTF8 sequences may result in two UTF16 characters being generated.
                        Decoder.Convert(buffer.Array, buffer.ToArrayIndex(m_ReadOffset), 1, oneChar, 0, 1, false, out bu, out cu, out complete);
                    } catch (System.ArgumentException ex) {
                        if (!ex.ParamName.Equals("chars")) throw;
                        Decoder.Convert(buffer.Array, buffer.ToArrayIndex(m_ReadOffset), 1, oneChar, 0, 2, false, out bu, out cu, out complete);
                    }
                    bytesRead += bu;
                    m_ReadOffset++;
                }
                if (cu == 0) return false;

                if (m_ReadCache.Free <= 1) Overflow();

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

            /// <summary>
            /// Resets the cache, taking optionally into account a previous overflow.
            /// </summary>
            /// <param name="withOverflow">if set to <c>true</c> a previous overflow is taken into account and reinserted into
            /// the buffer, as if the first character had already been read.</param>
            public void Reset(bool withOverflow)
            {
                m_ReadCache.Reset();
                m_ReadOffsets.Reset();
                m_Decoder.Reset();
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

            /// <summary>
            /// Compare the end of the read line cache to the string given.
            /// </summary>
            /// <param name="text">The text to compare.</param>
            /// <returns><c>true</c> if there is a match, <c>false</c> otherwise.</returns>
            /// <remarks>
            /// The ReadTo method should call this after decoding each individual byte.
            /// </remarks>
            public bool ReadToMatch(string text)
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
            /// Indicates if the ReadTo buffer has cached data.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has cached data; otherwise, <c>false</c>.
            /// </value>
            public bool IsCached
            {
                get { return m_ReadOffset != 0; }
            }

            /// <summary>
            /// Indicates if the amount of data read exceeds the line length.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is overflowed; otherwise, <c>false</c>.
            /// </value>
            public bool IsOverflowed
            {
                get { return m_ReadOverflow != -1; }
            }
        }

        [TestFixture]
        public class ReadToCacheTest
        {
            [Test]
            [Category("ReadToTest")]
            public void SimplePeekChar()
            {
                CircularBuffer<byte> buffer = new CircularBuffer<byte>(new byte[] { 0x65 });
                ReadToCache rtc = new ReadToCache();

                Assert.That(rtc.PeekChar(buffer), Is.True);
            }

            [Test]
            [Category("ReadToTest")]
            public void SimplePeekCharMultipleBytes()
            {
                CircularBuffer<byte> buffer = new CircularBuffer<byte>(new byte[] { 0x65, 0x66, 0x67 });
                ReadToCache rtc = new ReadToCache();

                Assert.That(rtc.PeekChar(buffer), Is.True);
                Assert.That(rtc.PeekChar(buffer), Is.True);
                Assert.That(rtc.PeekChar(buffer), Is.True);
                Assert.That(rtc.PeekChar(buffer), Is.False);
            }

            [Test]
            [Category("ReadToTest")]
            public void SimplePeekCharMultipleBytesMbcsEuro()
            {
                CircularBuffer<byte> buffer = new CircularBuffer<byte>(new byte[] { 0xE2, 0x82, 0xAC, 0x40 });
                ReadToCache rtc = new ReadToCache();

                Assert.That(rtc.PeekChar(buffer), Is.True);
                Assert.That(rtc.PeekChar(buffer), Is.True);
                Assert.That(rtc.PeekChar(buffer), Is.False);
            }

            [Test]
            [Category("ReadToTest")]
            public void SimplePeekCharMultipleBytesMbcsUtf32()
            {
                CircularBuffer<byte> buffer = new CircularBuffer<byte>(new byte[] { 0xF3, 0xA0, 0x82, 0x84, 0x30 });
                ReadToCache rtc = new ReadToCache();

                Assert.That(rtc.PeekChar(buffer), Is.True);
                Assert.That(rtc.PeekChar(buffer), Is.True);
                Assert.That(rtc.PeekChar(buffer), Is.False);
            }

            [Test]
            [Category("ReadToTest")]
            public void SimplePeekCharMultipleBytesMbcsUtf32ByteByByte()
            {
                byte[] bbuffer = { 0xF3, 0xA0, 0x82, 0x84, 0x30 };
                CircularBuffer<byte> buffer = new CircularBuffer<byte>(256);
                ReadToCache rtc = new ReadToCache();

                Assert.That(rtc.PeekChar(buffer), Is.False);

                buffer.Append(bbuffer[0]);
                Assert.That(rtc.PeekChar(buffer), Is.False);

                buffer.Append(bbuffer[1]);
                Assert.That(rtc.PeekChar(buffer), Is.False);

                buffer.Append(bbuffer[2]);
                Assert.That(rtc.PeekChar(buffer), Is.False);

                buffer.Append(bbuffer[3]);
                Assert.That(rtc.PeekChar(buffer), Is.True);

                buffer.Append(bbuffer[4]);
                Assert.That(rtc.PeekChar(buffer), Is.True);

                Assert.That(rtc.PeekChar(buffer), Is.False);
            }

            [Test]
            [Category("ReadToTest")]
            public void SimpleOverflow()
            {
                ReadToCache rtc = new ReadToCache();

                Random r = new Random();
                byte[] bbuffer = new byte[2048];

                // Ensure that each byte is ASCII for simplicity
                for (int i = 0; i < bbuffer.Length; i++) {
                    bbuffer[i] = (byte)r.Next(48, 126);
                }

                CircularBuffer<byte> buffer = new CircularBuffer<byte>(4096);
                buffer.Append(bbuffer);

                // We now peek 1023 times, which we shouldn't have an overflow
                for (int i = 0; i < 1023; i++) {
                    Assert.That(rtc.PeekChar(buffer), Is.True);
                }

                // 1024 times, fills the buffer. Shouldn't overflow.
                Assert.That(rtc.PeekChar(buffer), Is.True);

                // With this one we should overflow.
                //  m_ReadOverflow = 1
                //  m_ReadOverflowChar[0] is the first character
                //  m_ReadOverflowUtf32 = false
                Assert.That(rtc.PeekChar(buffer), Is.True);

                // Check that the next overflow is still correct
                //  m_ReadOverflow, m_ReadOverflowChar, m_ReadOverflowUtf32 shouldn't change
                Assert.That(rtc.PeekChar(buffer), Is.True);

                rtc.Reset(true);
            }

            [Test]
            [Category("ReadToTest")]
            public void OverflowWithUtf32AtEnd()
            {
                ReadToCache rtc = new ReadToCache();

                Random r = new Random();
                byte[] bbuffer = new byte[2048];

                // Ensure that each byte is ASCII for simplicity
                for (int i = 0; i < 1022; i++) {
                    bbuffer[i] = (byte)r.Next(48, 126);
                }
                bbuffer[1023] = 0xF3;
                bbuffer[1024] = 0xA0;
                bbuffer[1025] = 0x82;
                bbuffer[1026] = 0x84;
                for (int i = 1027; i < bbuffer.Length; i++) {
                    bbuffer[i] = (byte)r.Next(48, 126);
                }

                CircularBuffer<byte> buffer = new CircularBuffer<byte>(4096);
                buffer.Append(bbuffer);

                // We now peek 1023 times, which we shouldn't have an overflow
                for (int i = 0; i < 1023; i++) {
                    Assert.That(rtc.PeekChar(buffer), Is.True);
                }

                // Shouldn't overflow, but we've now got 1025 characters
                Assert.That(rtc.PeekChar(buffer), Is.True);

                // With this one we should overflow.
                //  m_ReadOverflow = 1
                //  m_ReadOverflowChar[0] is the first character
                //  m_ReadOverflowUtf32 = false
                Assert.That(rtc.PeekChar(buffer), Is.True);

                // Check that the next overflow is still correct
                //  m_ReadOverflow, m_ReadOverflowChar, m_ReadOverflowUtf32 shouldn't change
                Assert.That(rtc.PeekChar(buffer), Is.True);

                rtc.Reset(true);
            }

            [Test]
            [Category("ReadToTest")]
            public void OverflowWithUtf32AtBeginning()
            {
                ReadToCache rtc = new ReadToCache();

                Random r = new Random();
                byte[] bbuffer = new byte[2048];

                bbuffer[0] = 0xF3;
                bbuffer[1] = 0xA0;
                bbuffer[2] = 0x82;
                bbuffer[3] = 0x84;
                // Ensure that each byte is ASCII for simplicity
                for (int i = 4; i < bbuffer.Length; i++) {
                    bbuffer[i] = (byte)r.Next(48, 126);
                }

                CircularBuffer<byte> buffer = new CircularBuffer<byte>(4096);
                buffer.Append(bbuffer);

                // We now peek 1023 times, which we shouldn't have an overflow
                for (int i = 0; i < 1023; i++) {
                    Assert.That(rtc.PeekChar(buffer), Is.True);
                }

                // 1024 times, fills the buffer. No overflow expected here.
                Assert.That(rtc.PeekChar(buffer), Is.True);

                // With this one we should overflow.
                //  m_ReadOverflow = 4
                //  m_ReadOverflowChar[0] and [1] is the first character
                //  m_ReadOverflowUtf32 = true
                Assert.That(rtc.PeekChar(buffer), Is.True);

                // Check that the next overflow is still correct
                //  m_ReadOverflow, m_ReadOverflowChar, m_ReadOverflowUtf32 shouldn't change
                Assert.That(rtc.PeekChar(buffer), Is.True);

                rtc.Reset(true);
            }
        }
    }
}
