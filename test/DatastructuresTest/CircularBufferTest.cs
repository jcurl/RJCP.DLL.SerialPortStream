// Copyright © Jason Curl 2012-2020
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.Datastructures.CircularBufferTest
{
    using System;
    using System.Text;
    using NUnit.Framework;

    [TestFixture(Category = "Datastructures.CircularBuffer")]
    public class CircularBufferTest
    {
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(int.MinValue)]
        public void CircularBuffer_InvalidCapacity(int capacity)
        {
            Assert.That(() => { _ = new CircularBuffer<byte>(capacity); }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void CircularBuffer_NullArray()
        {
            Assert.That(() => { _ = new CircularBuffer<byte>(null); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CircularBuffer_EmptyArray()
        {
            Assert.That(() => { _ = new CircularBuffer<byte>(new byte[0]); }, Throws.TypeOf<ArgumentException>());
        }

        [Test]public void CircularBuffer_NullArrayCount()
        {
            Assert.That(() => { _ = new CircularBuffer<byte>(null, 0); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CircularBuffer_EmptyArrayCount()
        {
            Assert.That(() => { _ = new CircularBuffer<byte>(new byte[0], 1); }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void CircularBuffer_CountOutOfBounds()
        {
            Assert.That(() => { _ = new CircularBuffer<byte>(new byte[10], 11); }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void CircularBuffer_NullArrayCountOffset()
        {
            Assert.That(() => { _ = new CircularBuffer<byte>(null, 0, 0); }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CircularBuffer_EmptyArrayCountOffset()
        {
            Assert.That(() => { _ = new CircularBuffer<byte>(new byte[0], 0, 0); }, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void CircularBuffer_CountOutOfBoundsCountWithOffset()
        {
            Assert.That(() => { _ = new CircularBuffer<byte>(new byte[10], 0, 11); }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void CircularBuffer_CountOutOfBoundsOffset()
        {
            Assert.That(() => { _ = new CircularBuffer<byte>(new byte[10], 11, 0); }, Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void CircularBuffer_ProduceConsume()
        {
            CircularBuffer<byte> cb = new CircularBuffer<byte>(50);
            Assert.That(cb.Capacity, Is.EqualTo(50));

            // Initial state
            Assert.That(cb.Start, Is.EqualTo(0));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cb.ReadLength, Is.EqualTo(0));
            Assert.That(cb.End, Is.EqualTo(0));
            Assert.That(cb.Free, Is.EqualTo(50));
            Assert.That(cb.WriteLength, Is.EqualTo(50));

            // Test 1: Allocate 50 bytes
            cb.Produce(50);
            Assert.That(cb.Start, Is.EqualTo(0));
            Assert.That(cb.Length, Is.EqualTo(50));
            Assert.That(cb.ReadLength, Is.EqualTo(50));
            Assert.That(cb.End, Is.EqualTo(0));
            Assert.That(cb.Free, Is.EqualTo(0));
            Assert.That(cb.WriteLength, Is.EqualTo(0));

            // Test 2: Free 50 bytes
            cb.Consume(50);
            Assert.That(cb.Start, Is.EqualTo(0));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cb.ReadLength, Is.EqualTo(0));
            Assert.That(cb.End, Is.EqualTo(0));
            Assert.That(cb.Free, Is.EqualTo(50));
            Assert.That(cb.WriteLength, Is.EqualTo(50));

            // Test 3: Allocate 25 bytes
            cb.Produce(25);
            Assert.That(cb.Start, Is.EqualTo(0));
            Assert.That(cb.Length, Is.EqualTo(25));
            Assert.That(cb.ReadLength, Is.EqualTo(25));
            Assert.That(cb.End, Is.EqualTo(25));
            Assert.That(cb.Free, Is.EqualTo(25));
            Assert.That(cb.WriteLength, Is.EqualTo(25));

            // Test 4: Free 24 bytes
            cb.Consume(24);
            Assert.That(cb.Start, Is.EqualTo(24));
            Assert.That(cb.Length, Is.EqualTo(1));
            Assert.That(cb.ReadLength, Is.EqualTo(1));
            Assert.That(cb.End, Is.EqualTo(25));
            Assert.That(cb.Free, Is.EqualTo(49));
            Assert.That(cb.WriteLength, Is.EqualTo(25));

            // Test 5: Allocate 49 bytes
            cb.Produce(49);
            Assert.That(cb.Start, Is.EqualTo(24));
            Assert.That(cb.Length, Is.EqualTo(50));
            Assert.That(cb.ReadLength, Is.EqualTo(26));
            Assert.That(cb.End, Is.EqualTo(24));
            Assert.That(cb.Free, Is.EqualTo(0));
            Assert.That(cb.WriteLength, Is.EqualTo(0));

            // Test 6: Reset
            cb.Reset();
            Assert.That(cb.Start, Is.EqualTo(0));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cb.ReadLength, Is.EqualTo(0));
            Assert.That(cb.End, Is.EqualTo(0));
            Assert.That(cb.Free, Is.EqualTo(50));
            Assert.That(cb.WriteLength, Is.EqualTo(50));

            // Test 7: Test full wrapping around
            cb.Produce(25);
            cb.Consume(25);
            cb.Produce(50);
            Assert.That(cb.Start, Is.EqualTo(25));
            Assert.That(cb.Length, Is.EqualTo(50));
            Assert.That(cb.ReadLength, Is.EqualTo(25));
            Assert.That(cb.End, Is.EqualTo(25));
            Assert.That(cb.Free, Is.EqualTo(0));
            Assert.That(cb.WriteLength, Is.EqualTo(0));

            // Test 8: Free all data
            cb.Consume(50);
            Assert.That(cb.Start, Is.EqualTo(25));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cb.ReadLength, Is.EqualTo(0));
            Assert.That(cb.End, Is.EqualTo(25));
            Assert.That(cb.Free, Is.EqualTo(50));
            Assert.That(cb.WriteLength, Is.EqualTo(25));
        }

        [Test]
        public void CircularBuffer_Indexing()
        {
            CircularBuffer<byte> cb = new CircularBuffer<byte>(50);

            // Write into the array directly
            for (int i = 0; i < cb.Array.Length; i++) {
                cb.Array[i] = (byte)i;
            }
            cb.Produce(50);
            Assert.That(cb.Length, Is.EqualTo(50));
            Assert.That(cb.Start, Is.EqualTo(0));

            // Access the array using the indexer
            for (int i = 0; i < cb.Length; i++) {
                Assert.That(cb[i], Is.EqualTo(i));
            }

            cb.Consume(25);
            cb.Produce(25);

            // Now the start is in the middle
            Assert.That(cb.Start, Is.EqualTo(25));
            Assert.That(cb.Length, Is.EqualTo(50));
            for (int i = 0; i < cb.Length; i++) {
                Assert.That(cb[i], Is.EqualTo((i + 25) % 50), "Index {0}", i);
            }

            for (int i = 0; i < cb.Length; i++) {
                Assert.That(cb.Array[cb.ToArrayIndex(i)], Is.EqualTo((i + 25) % 50), "Index {0}", i);
            }
        }

        [Test]
        public void CircularBuffer_ReadBlock()
        {
            CircularBuffer<byte> cb = new CircularBuffer<byte>(50);

            // Move the pointer to the middle
            cb.Produce(25);
            cb.Consume(25);

            // Now allocate all space
            cb.Produce(25);
            cb.Produce(25);

            Assert.That(cb.ReadLength, Is.EqualTo(25));
            Assert.That(cb.GetReadBlock(0), Is.EqualTo(25));
            Assert.That(cb.GetReadBlock(5), Is.EqualTo(20));
            Assert.That(cb.GetReadBlock(24), Is.EqualTo(1));
            Assert.That(cb.GetReadBlock(25), Is.EqualTo(25));
            Assert.That(cb.GetReadBlock(30), Is.EqualTo(20));
            Assert.That(cb.GetReadBlock(49), Is.EqualTo(1));
            Assert.That(cb.GetReadBlock(50), Is.EqualTo(0));
        }

        [Test]
        public void CircularBuffer_Revert()
        {
            CircularBuffer<byte> cb = new CircularBuffer<byte>(50);

            // Move the pointer to the middle
            cb.Produce(25);
            cb.Consume(25);

            // Now allocate all space
            cb.Produce(25);
            cb.Produce(25);

            Assert.That(cb.Start, Is.EqualTo(25));
            Assert.That(cb.ReadLength, Is.EqualTo(25));
            Assert.That(cb.End, Is.EqualTo(25));
            Assert.That(cb.WriteLength, Is.EqualTo(0));

            cb.Revert(5);
            Assert.That(cb.Start, Is.EqualTo(25));
            Assert.That(cb.ReadLength, Is.EqualTo(25));
            Assert.That(cb.End, Is.EqualTo(20));
            Assert.That(cb.WriteLength, Is.EqualTo(5));

            cb.Revert(20);
            Assert.That(cb.Start, Is.EqualTo(25));
            Assert.That(cb.ReadLength, Is.EqualTo(25));
            Assert.That(cb.End, Is.EqualTo(0));
            Assert.That(cb.WriteLength, Is.EqualTo(25));

            cb.Revert(20);
            Assert.That(cb.Start, Is.EqualTo(25));
            Assert.That(cb.ReadLength, Is.EqualTo(5));
            Assert.That(cb.End, Is.EqualTo(30));
            Assert.That(cb.WriteLength, Is.EqualTo(20));
        }

        [Test]
        public void CircularBuffer_ReadWrite()
        {
            CircularBuffer<byte> cb = new CircularBuffer<byte>(50);
            cb.Produce(25);
            cb.Consume(25);
            cb.Produce(50);

            byte[] rd = new byte[50];
            Random r = new Random();
            r.NextBytes(rd);

            for (int i = 0; i < rd.Length; i++) {
                cb[i] = rd[i];
            }

            for (int i = 0; i < rd.Length; i++) {
                Assert.That(cb[i], Is.EqualTo(rd[i]), "Index {0} doesn't match", i);
            }
        }

        [Test]
        public void CircularBufferExt_GetStringSimple()
        {
            CircularBuffer<char> cb = new CircularBuffer<char>(15);
            for (int i = 0; i < cb.Capacity; i++) {
                cb.Array[i] = (char)('A' + i);
            }

            cb.Produce(10);
            Assert.That(cb.GetString(), Is.EqualTo("ABCDEFGHIJ"));
            cb.Consume(5);
            Assert.That(cb.GetString(), Is.EqualTo("FGHIJ"));
            cb.Produce(8);
            Assert.That(cb.GetString(), Is.EqualTo("FGHIJKLMNOABC"));
            cb.Consume(13);
            Assert.That(cb.GetString(), Is.EqualTo(""));

            cb = null;
            Assert.That(cb.GetString(), Is.Null);
        }

        [Test]
        public void CircularBufferExt_GetStringLength()
        {
            CircularBuffer<char> cb = new CircularBuffer<char>(15);
            for (int i = 0; i < cb.Capacity; i++) {
                cb.Array[i] = (char)('A' + i);
            }

            cb.Produce(10);
            Assert.That(cb.GetString(10), Is.EqualTo("ABCDEFGHIJ"));
            Assert.That(cb.GetString(20), Is.EqualTo("ABCDEFGHIJ"));
            Assert.That(cb.GetString(5), Is.EqualTo("ABCDE"));
            Assert.That(cb.GetString(1), Is.EqualTo("A"));
            Assert.That(cb.GetString(0), Is.EqualTo(""));
            cb.Consume(5);
            Assert.That(cb.GetString(10), Is.EqualTo("FGHIJ"));
            Assert.That(cb.GetString(5), Is.EqualTo("FGHIJ"));
            Assert.That(cb.GetString(3), Is.EqualTo("FGH"));
            Assert.That(cb.GetString(0), Is.EqualTo(""));
            cb.Produce(8);
            Assert.That(cb.GetString(13), Is.EqualTo("FGHIJKLMNOABC"));
            Assert.That(cb.GetString(5), Is.EqualTo("FGHIJ"));
            Assert.That(cb.GetString(1), Is.EqualTo("F"));
            Assert.That(cb.GetString(0), Is.EqualTo(""));
            cb.Consume(13);
            Assert.That(cb.GetString(0), Is.EqualTo(""));
            Assert.That(cb.GetString(15), Is.EqualTo(""));
            Assert.That(cb.GetString(20), Is.EqualTo(""));

            cb = null;
            Assert.That(cb.GetString(5), Is.Null);
            Assert.That(cb.GetString(0), Is.Null);
            Assert.That(cb.GetString(15), Is.Null);
            Assert.That(cb.GetString(20), Is.Null);
        }

        [Test]
        public void CircularBufferExt_GetStringOffsetLength()
        {
            CircularBuffer<char> cb = new CircularBuffer<char>(15);
            for (int i = 0; i < cb.Capacity; i++) {
                cb.Array[i] = (char)('A' + i);
            }

            cb.Produce(10);
            Assert.That(cb.GetString(0, 10), Is.EqualTo("ABCDEFGHIJ"));
            Assert.That(cb.GetString(0, 20), Is.EqualTo("ABCDEFGHIJ"));
            Assert.That(cb.GetString(0, 0), Is.EqualTo(""));
            Assert.That(cb.GetString(5, 5), Is.EqualTo("FGHIJ"));
            Assert.That(cb.GetString(1, 9), Is.EqualTo("BCDEFGHIJ"));
            Assert.That(cb.GetString(1, 7), Is.EqualTo("BCDEFGH"));
            Assert.That(cb.GetString(5, 0), Is.EqualTo(""));
            cb.Consume(5);
            Assert.That(cb.GetString(0, 10), Is.EqualTo("FGHIJ"));
            Assert.That(cb.GetString(0, 5), Is.EqualTo("FGHIJ"));
            Assert.That(cb.GetString(2, 3), Is.EqualTo("HIJ"));
            Assert.That(cb.GetString(5, 0), Is.EqualTo(""));
            Assert.That(cb.GetString(5, 1), Is.EqualTo(""));
            cb.Produce(8);
            Assert.That(cb.GetString(0, 13), Is.EqualTo("FGHIJKLMNOABC"));
            Assert.That(cb.GetString(0, 5), Is.EqualTo("FGHIJ"));
            Assert.That(cb.GetString(5, 13), Is.EqualTo("KLMNOABC"));
            Assert.That(cb.GetString(5, 8), Is.EqualTo("KLMNOABC"));
            Assert.That(cb.GetString(5, 5), Is.EqualTo("KLMNO"));
            Assert.That(cb.GetString(10, 3), Is.EqualTo("ABC"));
            cb.Consume(13);
            Assert.That(cb.GetString(0, 0), Is.EqualTo(""));
            Assert.That(cb.GetString(5, 15), Is.EqualTo(""));
            Assert.That(cb.GetString(10, 20), Is.EqualTo(""));
            Assert.That(cb.GetString(10, 1), Is.EqualTo(""));

            cb = null;
            Assert.That(cb.GetString(0, 5), Is.Null);
            Assert.That(cb.GetString(10, 0), Is.Null);
            Assert.That(cb.GetString(5, 15), Is.Null);
            Assert.That(cb.GetString(2, 20), Is.Null);
        }

        [Test]
        public void CircularBuffer_AppendArray()
        {
            CircularBuffer<char> cb = new CircularBuffer<char>(20);
            cb.Produce(10);
            cb.Consume(9);

            Assert.That(cb.Length, Is.EqualTo(1));
            Assert.That(cb.Free, Is.EqualTo(19));

            cb.Append("ABCDEFGHIJKLMN".ToCharArray(), 0, 14);
            Assert.That(cb.Length, Is.EqualTo(15));
            Assert.That(cb.Free, Is.EqualTo(5));

            Assert.That(cb.GetString(1, 14), Is.EqualTo("ABCDEFGHIJKLMN"));
            cb.Consume(15);
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cb.Free, Is.EqualTo(20));

            cb.Append("12345678901234567890".ToCharArray());
            Assert.That(cb.Free, Is.EqualTo(0));
            Assert.That(cb.Length, Is.EqualTo(20));
            Assert.That(cb.GetString(), Is.EqualTo("12345678901234567890"));
        }

        [Test]
        public void CircularBuffer_AppendBuffer()
        {
            CircularBuffer<char> cb1 = new CircularBuffer<char>(20);
            CircularBuffer<char> cb2 = new CircularBuffer<char>(20);

            // Read is one chunk, write is one chunk
            cb2.Produce(3);
            cb2.Append("123456789012345".ToCharArray());
            cb2.Consume(3);

            cb1.Append(cb2);
            Assert.That(cb1.Length, Is.EqualTo(15));
            Assert.That(cb1.Free, Is.EqualTo(5));
            Assert.That(cb1.GetString(), Is.EqualTo("123456789012345"));

            // Write is one chunk, but read is two chunks
            cb1.Reset();
            cb2.Reset();
            cb2.Produce(15);
            cb2.Consume(14);
            cb2.Append("123456789012345".ToCharArray());
            cb2.Consume(1);

            cb1.Append(cb2);
            Assert.That(cb1.Length, Is.EqualTo(15));
            Assert.That(cb1.Free, Is.EqualTo(5));
            Assert.That(cb1.GetString(), Is.EqualTo("123456789012345"));

            // Write is two chunks, read is one chunk
            cb1.Reset();
            cb2.Reset();
            cb1.Produce(10);
            cb1.Consume(9);
            cb2.Append("123456789012345".ToCharArray());

            cb1.Append(cb2);
            cb1.Consume(1);
            Assert.That(cb1.Length, Is.EqualTo(15));
            Assert.That(cb1.Free, Is.EqualTo(5));
            Assert.That(cb1.GetString(), Is.EqualTo("123456789012345"));

            // Write is two chunks, read is two chunks, readlength < writelength
            cb1.Reset();
            cb2.Reset();
            cb1.Produce(10);
            cb1.Consume(9);
            cb2.Produce(15);
            cb2.Consume(14);
            cb2.Append("123456789012345".ToCharArray());
            cb2.Consume(1);

            cb1.Append(cb2);
            cb1.Consume(1);
            Assert.That(cb1.Length, Is.EqualTo(15));
            Assert.That(cb1.Free, Is.EqualTo(5));
            Assert.That(cb1.GetString(), Is.EqualTo("123456789012345"));

            // Write is two chunks, read is two chunks, readlength > writelength
            cb1.Reset();
            cb2.Reset();
            cb1.Produce(10);
            cb1.Consume(9);
            cb2.Produce(7);
            cb2.Consume(6);
            cb2.Append("123456789012345".ToCharArray());
            cb2.Consume(1);

            cb1.Append(cb2);
            cb1.Consume(1);
            Assert.That(cb1.Length, Is.EqualTo(15));
            Assert.That(cb1.Free, Is.EqualTo(5));
            Assert.That(cb1.GetString(), Is.EqualTo("123456789012345"));
        }

        [Test]
        public void CircularBuffer_AppendWithOffsetBoundaries()
        {
            CircularBuffer<char> cb = new CircularBuffer<char>(20);

            cb.Append("abcdefghijklmno".ToCharArray());
            cb.Consume(14);
            Assert.That(cb.Length, Is.EqualTo(1));
            Assert.That(cb.GetString(), Is.EqualTo("o"));

            cb.Append("pqrstuvwxy".ToCharArray(), 3, 7);
            Assert.That(cb.Length, Is.EqualTo(8));
            Assert.That(cb.GetString(), Is.EqualTo("ostuvwxy"));
        }

        [Test]
        public void CircularBuffer_ConstructorArray()
        {
            byte[] m = {
                0x80, 0x00, 0x2F,
                0x11, 0x40, 0x2B, 0x00, 0xCD, 0xC0, 0x27, 0x90,
                0x22, 0x30, 0x02, 0x00, 0x02, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x94, 0x00, 0x6D, 0x6F, 0x73, 0x74,
                0x20, 0x74, 0x65, 0x72, 0x6D, 0x69, 0x6E, 0x61,
                0x6C, 0x20, 0x73, 0x74, 0x61, 0x74, 0x75, 0x73,
                0x20, 0x3D, 0x20, 0x31, 0x30, 0x29, 0x00,
                0xA6, 0x73 };

            CircularBuffer<byte> cb1 = new CircularBuffer<byte>(m);
            Assert.That(cb1.Length, Is.EqualTo(m.Length));
            Assert.That(cb1.Free, Is.EqualTo(0));
            Assert.That(cb1.Start, Is.EqualTo(0));
            Assert.That(cb1[0], Is.EqualTo(0x80));

            CircularBuffer<byte> cb2 = new CircularBuffer<byte>(m, 10);
            Assert.That(cb2.Length, Is.EqualTo(10));
            Assert.That(cb2.Free, Is.EqualTo(m.Length - 10));
            Assert.That(cb2.Start, Is.EqualTo(0));
            Assert.That(cb2[0], Is.EqualTo(0x80));

            CircularBuffer<byte> cb3 = new CircularBuffer<byte>(m, 15, 10);
            Assert.That(cb3.Length, Is.EqualTo(10));
            Assert.That(cb3.Free, Is.EqualTo(m.Length - 10));
            Assert.That(cb3.Start, Is.EqualTo(15));
            Assert.That(cb3[0], Is.EqualTo(0x02));
        }

        [Test]
        public void CircularBufferExt_DecoderConvertSourceLengthZeroDestCharArray()
        {
            // On Mono when using ISO-8859-15, a source length of zero causes problems.
            Decoder d = Encoding.GetEncoding("ISO-8859-15").GetDecoder();
            CircularBuffer<byte> cb = new CircularBuffer<byte>(100);
            char[] outc = new char[100];

            d.Convert(cb, outc, 0, outc.Length, true, out int bu, out int cu, out bool completed);
            Assert.That(bu, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(0));
            Assert.That(completed, Is.True);
        }

        [Test]
        public void CircularBufferExt_DecoderConvertSourceLengthZeroDestCharBuffer()
        {
            // On Mono when using ISO-8859-15, a source length of zero causes problems.
            Decoder d = Encoding.GetEncoding("ISO-8859-15").GetDecoder();
            CircularBuffer<byte> cb = new CircularBuffer<byte>(100);
            CircularBuffer<char> cc = new CircularBuffer<char>(100);

            d.Convert(cb, cc, cc.Length, true, out int bu, out int cu, out bool completed);
            Assert.That(bu, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(0));
            Assert.That(completed, Is.True);
        }

        [Test]
        public void CircularBufferExt_DecoderConvertSourceLengthZeroDestCharBuffer2()
        {
            // On Mono when using ISO-8859-15, a source length of zero causes problems.
            Decoder d = Encoding.GetEncoding("ISO-8859-15").GetDecoder();
            CircularBuffer<byte> cb = new CircularBuffer<byte>(100);
            CircularBuffer<char> cc = new CircularBuffer<char>(100);

            d.Convert(cb, cc, true, out int bu, out int cu, out bool completed);
            Assert.That(bu, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(0));
            Assert.That(completed, Is.True);
        }

        [Test]
        public void CircularBufferExt_DecoderConvertSourceArrayZeroDestCharBuffer()
        {
            // On Mono when using ISO-8859-15, a source length of zero causes problems.
            Decoder d = Encoding.GetEncoding("ISO-8859-15").GetDecoder();
            byte[] sb = new byte[100];
            CircularBuffer<char> cc = new CircularBuffer<char>(100);

            d.Convert(sb, 0, 0, cc, true, out int bu, out int cu, out bool completed);
            Assert.That(bu, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(0));
            Assert.That(completed, Is.True);
        }

        [Test]
        public void CircularBufferExt_EncoderConvertSourceZero()
        {
            // On Mono when using ISO-8859-15, a source length of zero causes problems.
            Encoder e = Encoding.GetEncoding("ISO-8859-15").GetEncoder();
            char[] sc = new char[100];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(100);

            e.Convert(sc, 0, 0, cb, true, out int cu, out int bu, out bool completed);
            Assert.That(cu, Is.EqualTo(0));
            Assert.That(bu, Is.EqualTo(0));
            Assert.That(completed, Is.True);
        }

        /// <summary>
        /// Check converting a byte array to a char array with convert works.
        /// </summary>
        /// <remarks>
        /// This test places one byte of the Euro symbol at the end of the byte array that wraps around, to ensure
        /// multibyte arrays are handled correctly.
        /// </remarks>
        [Test]
        public void CircularBufferExt_DecoderConvert1_Boundaries()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[28];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);

            // Based on the test "Decoder_Boundaries1"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, c, 0, c.Length, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(m.Length));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(c.Length));
            Assert.That(new string(c), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN"));
        }

        /// <summary>
        /// Check converting a byte array to a char array with convert works.
        /// </summary>
        /// <remarks>
        /// This test places one byte of the Euro symbol at the end of the byte array that wraps around, to ensure
        /// multibyte arrays are handled correctly.
        /// </remarks>
        [Test]
        public void CircularBufferExt_DecoderConvert1_BoundariesFlush()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[28];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);

            // Based on the test "Decoder_Boundaries2"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, c, 0, c.Length, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(m.Length));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(c.Length));
            Assert.That(new string(c), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN"));
        }

        /// <summary>
        /// Check converting a byte array to a char array with convert works.
        /// </summary>
        /// <remarks>
        /// The test places the Euro symbol at the end of the byte array and wraps over to the beginning. There is
        /// insufficient bytes in the char array to convert everything, but enough to convert the Euro symbol.
        /// </remarks>
        [Test]
        public void CircularBufferExt_DecoderConvert1_InsufficientCharSpace()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[20];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);

            // Based on the test "Decoder_InsufficientCharSpace"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, c, 0, c.Length, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(22));
            Assert.That(cb.Length, Is.EqualTo(8));
            Assert.That(cu, Is.EqualTo(c.Length));
            Assert.That(new string(c), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEF"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert1_InsufficientCharSpaceFlush()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[20];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);

            // Based on the test "Decoder_InsufficientCharSpaceFlush"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, c, 0, c.Length, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(22));
            Assert.That(cb.Length, Is.EqualTo(8));
            Assert.That(cu, Is.EqualTo(c.Length));
            Assert.That(new string(c), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEF"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert1_InsufficientCharSpaceMbcs1()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[13];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);

            // Based on the test "Decoder_InsufficientCharSpaceMbcs1"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, c, 0, c.Length, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(15));
            Assert.That(cb.Length, Is.EqualTo(15));
            Assert.That(cu, Is.EqualTo(c.Length));
            Assert.That(new string(c), Is.EqualTo("OPQRSTUVWXYZ€"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert1_IncompleteBuff()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 13);

            // Based on the test "Decoder_IncompleteBuff"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, c, 0, c.Length, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(13));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(12));
            Assert.That(new string(c, 0, cu), Is.EqualTo("OPQRSTUVWXYZ"));

            cb.Produce(17);
            d.Convert(cb, c, 0, c.Length, false, out bu, out cu, out complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(17));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(16));
            Assert.That(new string(c, 0, 16), Is.EqualTo("€@ABCDEFGHIJKLMN"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert1_IncompleteBuffFlush()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 14);

            // Based on the test "Decoder_IncompleteBuffFlush"
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            d.Convert(cb, c, 0, c.Length, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(14));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(13));
            Assert.That(new string(c, 0, 13), Is.EqualTo("OPQRSTUVWXYZ."));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert1_IncompleteBuffFlush2()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 14);

            // Based on the test "Decoder_IncompleteBuffFlush2"
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            d.Convert(cb, c, 0, 12, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(12));
            Assert.That(cu, Is.EqualTo(12));
            Assert.That(new string(c, 0, 12), Is.EqualTo("OPQRSTUVWXYZ"));
            Assert.That(cb.Length, Is.EqualTo(2));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert1_Utf16Chars1()
        {
            byte[] m = {
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 16);

            // Based on the test "Decoder_Utf16Chars1"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, c, 0, 14, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(16));
            Assert.That(cu, Is.EqualTo(14));
            Assert.That(new string(c, 0, 14), Is.EqualTo("OPQRSTUVWXYZ\uDB40\uDC84"));
            Assert.That(cb.Length, Is.EqualTo(0));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert1_Utf16Chars2()
        {
            byte[] m = {
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 16);

            // Based on the test "Decoder_Utf16Chars2"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, c, 0, 13, false, out _, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(cu, Is.EqualTo(12));
            Assert.That(new string(c, 0, 12), Is.EqualTo("OPQRSTUVWXYZ"));

            // This particular test is hard. The decoder consumes 12 bytes, but our function consumes more (because the
            // 4-bytes cross a boundary). The decoder needs to see all four bytes to decide not to convert it. There is
            // nothing in the documentation to say that the decoder should behave this way. So we can't simulate the
            // original behavior in this case.
        }

        [Test]
        public void CircularBufferExt_DecoderConvert1_Utf16Chars3()
        {
            byte[] m = {
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };

            // Based on the test "Decoder_Utf16Chars3"

            // The behavior isn't the same due to non-documented behavior. MS documentation doesn't say if an
            // exception should occur or not and it has slightly inconsistent behavior if we sent 3 bytes (of 4), or 4
            // bytes at once.

            Decoder d = Encoding.UTF8.GetDecoder();
            int cu;
            int bu;
            bool complete;
            bool exception = false;
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 28, 4);
            try {
                // An exception might be raised to say that the data can't be converted to fit in the output memory
                // buffer. But then nothing should be consumed. Instead, it may be bytes are consumed and no exception,
                // or no bytes are consumed and an exception.
                d.Convert(cb, c, 0, 1, false, out bu, out cu, out complete);
                Assert.That(bu, Is.Not.EqualTo(0));          // If no exception, then bytes must be consumed.
                Assert.That(cb.Length, Is.EqualTo(4 - bu));  // And that the buffer is reduced by the correct amount.
                Assert.That(complete, Is.False);
            } catch (ArgumentException e) {
                if (e.ParamName == null || !e.ParamName.Equals("chars")) throw;
                exception = true;
                cu = -1;
            }

            if (!exception) {
                // The conversion must be a single Unicode character, which is two UTF-16. As the previous test allowed
                // only 1 UTF16 char, it must be zero if no exception was raised.
                Assert.That(cu, Is.EqualTo(0));
                Assert.That(() => {
                    d.Convert(cb, c, 0, 1, false, out _, out _, out _);
                }, Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("chars"));
            }

            // Second test shows that after a decoder reset, the results are consistent.

            exception = false;
            cb = new CircularBuffer<byte>(m, 28, 4);
            d.Reset();
            try {
                d.Convert(cb, c, 0, 1, true, out bu, out cu, out complete);
                Assert.That(bu, Is.Not.EqualTo(0));          // If no exception, then bytes must be consumed.
                Assert.That(cb.Length, Is.EqualTo(4 - bu));  // And that the buffer is reduced by the correct amount.
                Assert.That(complete, Is.False);
            } catch (ArgumentException e) {
                if (e.ParamName == null || !e.ParamName.Equals("chars")) throw;
                exception = true;
                cu = -1;
            }

            if (!exception) {
                Assert.That(cu, Is.EqualTo(0));
                Assert.That(() => {
                    d.Convert(cb, c, 0, 1, false, out _, out _, out _);
                }, Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("chars"));
            }

            d.Convert(cb, c, 0, 2, true, out _, out cu, out complete);
            Assert.That(complete, Is.True);
            Assert.That(cu, Is.EqualTo(2));
            Assert.That(cb.Length, Is.EqualTo(0));  // Show that all data was now consumed
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_Boundaries()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[28];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            // Based on the test "Decoder_Boundaries"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, cc.Free, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(m.Length));
            Assert.That(cu, Is.EqualTo(cc.Capacity));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_BoundariesFlush()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[28];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            // Based on the test "Decoder_Boundaries"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, cc.Free, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(m.Length));
            Assert.That(cu, Is.EqualTo(cc.Capacity));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_InsufficientCharSpace()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[20];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            // Based on the test "Decoder_InsufficientCharSpace"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, cc.Free, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(22));
            Assert.That(cu, Is.EqualTo(cc.Capacity));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEF"));
            Assert.That(cb.Length, Is.EqualTo(8));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_InsufficientCharSpaceFlush()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[20];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            // Based on the test "Decoder_InsufficientCharSpaceFlush"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, cc.Free, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(22));
            Assert.That(cu, Is.EqualTo(cc.Capacity));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEF"));
            Assert.That(cb.Length, Is.EqualTo(8));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_InsufficientCharSpaceMbcs1()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[13];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            // Based on the test "Decoder_InsufficientCharSpaceMbcs1"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, cc.Free, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(15));
            Assert.That(cu, Is.EqualTo(cc.Capacity));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_IncompleteBuff()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 13);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            // Based on the test "Decoder_IncompleteBuff"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, cc.Free, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(13));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(12));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ"));
            cc.Consume(cu);

            cb.Produce(17);
            d.Convert(cb, cc, cc.Free, false, out bu, out cu, out complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(17));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(16));
            Assert.That(cc.GetString(), Is.EqualTo("€@ABCDEFGHIJKLMN"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_IncompleteBuffFlush()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 14);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            // Based on the test "Decoder_IncompleteBuffFlush"
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            d.Convert(cb, cc, cc.Free, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(14));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(13));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ."));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_IncompleteBuffFlush2()
        {
            byte[] m = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 14);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            // Based on the test "Decoder_IncompleteBuffFlush2"
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            d.Convert(cb, cc, 12, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(12));
            Assert.That(cu, Is.EqualTo(12));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ"));
            Assert.That(cb.Length, Is.EqualTo(2));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_Utf16Chars1a()
        {
            byte[] m = {
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 16);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            // Based on the test "Decoder_Utf16Chars1"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, 14, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(16));
            Assert.That(cu, Is.EqualTo(14));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ\uDB40\uDC84"));
            Assert.That(cb.Length, Is.EqualTo(0));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_Utf16Chars1b()
        {
            byte[] m = {
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 16);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 17, 0);

            // Based on the test "Decoder_Utf16Chars1"
            // - We force the 2 chars to wrap
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, 14, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(16));
            Assert.That(cu, Is.EqualTo(14));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ\uDB40\uDC84"));
            Assert.That(cb.Length, Is.EqualTo(0));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_Utf16Chars2()
        {
            byte[] m = {
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 16);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            // Based on the test "Decoder_Utf16Chars2"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, 13, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(cu, Is.EqualTo(12));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ"));
            Assert.That(cb.Length, Is.EqualTo(16 - bu));

            // This particular test is hard. The decoder consumes 12 bytes, but our function consumes more (because the
            // 4-bytes cross a boundary). The decoder needs to see all four bytes to decide not to convert it. There is
            // nothing in the documentation to say that the decoder should behave this way. So we can't simulate the
            // original behavior in this case.
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_Utf16Chars3()
        {
            byte[] m = {
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };

            // Based on the test "Decoder_Utf16Chars3"

            // The behavior isn't the same due to non-documented behavior. MS documentation doesn't say if an
            // exception should occur or not and it has slightly inconsistent behavior if we sent 3 bytes (of 4), or 4
            // bytes at once.

            int bu;
            int cu;
            bool complete;
            bool exception = false;
            Decoder d = Encoding.UTF8.GetDecoder();
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 28, 4);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);
            try {
                // An exception might be raised to say that the data can't be converted to fit in the output memory
                // buffer. But then nothing should be consumed. Instead, it may be bytes are consumed and no exception,
                // or no bytes are consumed and an exception.
                d.Convert(cb, cc, 1, false, out bu, out cu, out complete);
                Assert.That(bu, Is.Not.EqualTo(0));          // If no exception, then bytes must be consumed.
                Assert.That(cb.Length, Is.EqualTo(4 - bu));  // And that the buffer is reduced by the correct amount.
                Assert.That(complete, Is.False);
            } catch (ArgumentException e) {
                if (e.ParamName == null || !e.ParamName.Equals("chars")) throw;
                exception = true;
                cu = -1;
            }
            if (!exception) {
                // The conversion must be a single Unicode character, which is two UTF-16. As the previous test allowed
                // only 1 UTF16 char, it must be zero if no exception was raised.
                Assert.That(cu, Is.EqualTo(0));
                Assert.That(() => {
                    d.Convert(cb, cc, 1, false, out _, out _, out _);
                }, Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("chars"));
            }

            // Second test shows that after a decoder reset, the results are consistent.

            exception = false;
            cb = new CircularBuffer<byte>(m, 28, 4);
            d.Reset();
            try {
                d.Convert(cb, cc, 1, true, out bu, out cu, out complete);
                Assert.That(bu, Is.Not.EqualTo(0));          // If no exception, then bytes must be consumed.
                Assert.That(cb.Length, Is.EqualTo(4 - bu));  // And that the buffer is reduced by the correct amount.
                Assert.That(complete, Is.False);
            } catch (ArgumentException e) {
                if (e.ParamName == null || !e.ParamName.Equals("chars")) throw;
                exception = true;
                cu = -1;
            }
            if (!exception) {
                Assert.That(cu, Is.EqualTo(0));
                Assert.That(() => {
                    d.Convert(cb, cc, 1, false, out _, out _, out _);
                }, Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("chars"));
            }

            d.Convert(cb, cc, 2, true, out _, out cu, out complete);
            Assert.That(complete, Is.True);
            Assert.That(cu, Is.EqualTo(2));
            Assert.That(cb.Length, Is.EqualTo(0));  // Show that all data was now consumed
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_CircularFull()
        {
            byte[] b = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(b, 16, b.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 28, 0);

            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, cc.Capacity, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(b.Length));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(28));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_MultiChar1()
        {
            byte[] b = {
                0xF3, 0xA0, 0x82, 0x84, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40, 0x41,
                0x42, 0x43, 0x44, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(b, 0, b.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 29, 0);

            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, cc.Capacity, false, out int bu, out int cu, out _);
            Assert.That(cu, Is.EqualTo(30));
            Assert.That(bu, Is.EqualTo(32));
            Assert.That(cc.Free, Is.EqualTo(0));
            Assert.That(cc.GetString(), Is.EqualTo("\uDB40\uDC840123456789@ABCDIJKLMNOPQRSTU"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert2_MultiChar2()
        {
            byte[] b = {
                0xF3, 0xA0, 0x82, 0x84, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40, 0x41,
                0x42, 0x43, 0x44, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55
            };
            char[] c = new char[29];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(b, 4, b.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 0, 0);

            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, cc.Capacity, false, out int bu, out int cu, out bool complete);

            // The character buffer doesn't contain enough space to capture the last two-byte character. Just like the
            // real decoder, it should capture as much as possible and return without an error.
            Assert.That(complete, Is.False);
            Assert.That(cu, Is.EqualTo(28));
            Assert.That(bu, Is.EqualTo(28));
            Assert.That(cc.Free, Is.EqualTo(1));
            Assert.That(cc.GetString(), Is.EqualTo("0123456789@ABCDIJKLMNOPQRSTU"));

            // There are no bytes to convert, an exception should be raised like the real decoder in a char[].
            Assert.That(() => {
                d.Convert(cb, cc, cc.Free, false, out bu, out cu, out complete);
            }, Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("chars"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert3_BoundariesWithFlush()
        {
            Decoder d = Encoding.UTF8.GetDecoder();
            byte[] m = { 0x41, 0xE2, 0x82, 0xAC };
            CircularBuffer<byte> cb;
            CircularBuffer<char> cc;

            cb = new CircularBuffer<byte>(m, 0, 3);
            cc = new CircularBuffer<char>(20);
            d.Convert(cb, cc, true, out int bu, out int cu, out bool complete);
            Assert.That(bu, Is.EqualTo(3));
            Assert.That(cu, Is.EqualTo(2));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cc.Length, Is.EqualTo(2));
            Assert.That(cc[0], Is.EqualTo('A'));
            Assert.That(cc[1], Is.EqualTo((char)0xFFFD));
            Assert.That(complete, Is.True);

            cb = new CircularBuffer<byte>(m, 0, 3);
            cc = new CircularBuffer<char>(20);
            cc.Produce(19);
            cc.Consume(18);
            d.Convert(cb, cc, true, out bu, out cu, out complete);
            cc.Consume(1);
            Assert.That(bu, Is.EqualTo(3));
            Assert.That(cu, Is.EqualTo(2));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cc.Length, Is.EqualTo(2));
            Assert.That(cc[0], Is.EqualTo('A'));
            Assert.That(cc[1], Is.EqualTo((char)0xFFFD));
            Assert.That(complete, Is.True);
        }

        [Test]
        public void CircularBufferExt_DecoderConvert3_CircularFull2()
        {
            byte[] b = {
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(b, 16, b.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 28, 0);

            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(cb, cc, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(b.Length));
            Assert.That(cb.Length, Is.EqualTo(0));
            Assert.That(cu, Is.EqualTo(28));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_Boundaries1()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            CircularBuffer<char> cc = new CircularBuffer<char>(28);

            // Based on the test "Decoder_Boundaries"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(m, 0, m.Length, cc, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(m.Length));
            Assert.That(cu, Is.EqualTo(28));
            Assert.That(cc.Length, Is.EqualTo(28));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_Boundaries2()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[28];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 20, 0);

            // Based on the test "Decoder_Boundaries"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(m, 0, m.Length, cc, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(m.Length));
            Assert.That(cu, Is.EqualTo(28));
            Assert.That(cc.Length, Is.EqualTo(28));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_BoundariesFlush()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[28];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 20, 0);

            // Based on the test "Decoder_BoundariesFlush"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(m, 0, m.Length, cc, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(m.Length));
            Assert.That(cu, Is.EqualTo(28));
            Assert.That(cc.Length, Is.EqualTo(28));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_InsufficientCharSpace()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[20];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 15, 0);

            // Based on the test "Decoder_InsufficientCharSpace"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(m, 0, m.Length, cc, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(22));
            Assert.That(cu, Is.EqualTo(20));
            Assert.That(cc.Length, Is.EqualTo(20));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEF"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_InsufficientCharSpaceFlush()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[20];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 15, 0);

            // Based on the test "Decoder_InsufficientCharSpaceFlush"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(m, 0, m.Length, cc, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(22));
            Assert.That(cu, Is.EqualTo(20));
            Assert.That(cc.Length, Is.EqualTo(20));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€@ABCDEF"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_InsufficientCharSpaceMbcs1()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[13];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 4, 0);

            // Based on the test "Decoder_InsufficientCharSpaceMbcs1"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(m, 0, m.Length, cc, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(15));
            Assert.That(cu, Is.EqualTo(13));
            Assert.That(cc.Length, Is.EqualTo(13));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ€"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_IncompleteBuff()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[30];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 20, 0);

            // Based on the test "Decoder_IncompleteBuff"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(m, 0, 13, cc, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(13));
            Assert.That(cu, Is.EqualTo(12));
            Assert.That(cc.Length, Is.EqualTo(12));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ"));
            cc.Consume(12);

            d.Convert(m, 13, 17, cc, false, out bu, out cu, out complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(17));
            Assert.That(cu, Is.EqualTo(16));
            Assert.That(cc.GetString(), Is.EqualTo("€@ABCDEFGHIJKLMN"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_IncompleteBuffFlush()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82, 0xAC, 0x40,
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[30];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 20, 0);

            // Based on the test "Decoder_IncompleteBuffFlush"
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            d.Convert(m, 0, 14, cc, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(14));
            Assert.That(cu, Is.EqualTo(13));
            Assert.That(cc.Length, Is.EqualTo(13));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ."));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_IncompleteBuffFlush2()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82, 0xAC, 0x40,
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[12];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 11, 0);

            // Based on the test "Decoder_IncompleteBuffFlush2"
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            d.Convert(m, 0, 14, cc, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(12));
            Assert.That(cu, Is.EqualTo(12));
            Assert.That(cc.Length, Is.EqualTo(12));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_Utf16Chars1()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0, 0x82, 0x84,
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[14];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 11, 0);

            // Based on the test "Decoder_Utf16Chars1"
            Decoder d = Encoding.UTF8.GetDecoder();
            d.Convert(m, 0, 16, cc, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(16));
            Assert.That(cu, Is.EqualTo(14));
            Assert.That(cc.Length, Is.EqualTo(14));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ\uDB40\uDC84"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_Utf16Chars2()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0, 0x82, 0x84,
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[13];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 11, 0);

            // Based on the test "Decoder_Utf16Chars2"
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            d.Convert(m, 0, 16, cc, false, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(12));
            Assert.That(cu, Is.EqualTo(12));
            Assert.That(cc.Length, Is.EqualTo(12));
            Assert.That(cc.GetString(), Is.EqualTo("OPQRSTUVWXYZ"));
        }

        [Test]
        public void CircularBufferExt_DecoderConvert4_Utf16Chars3()
        {
            byte[] m = {
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0, 0x82, 0x84,
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[1];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 0, 0);

            // Based on the test "Decoder_Utf16Chars3"

            // We expect this to fail, as a two-char Unicode character doesn't fit in one byte
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            Assert.That(() => {
                d.Convert(m, 12, 10, cc, false, out _, out _, out _);
            }, Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("chars"));

            // We expect this to fail, as a two-char Unicode character doesn't fit in one byte
            Assert.That(() => {
                d.Convert(m, 12, 10, cc, false, out _, out _, out _);
            }, Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("chars"));

            c = new char[2];
            cc = new CircularBuffer<char>(c, 1, 0);
            d.Convert(m, 12, 4, cc, true, out int bu, out int cu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(4));
            Assert.That(cu, Is.EqualTo(2));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert()
        {
            char[] c = { 'A', 'B', '€', 'C' };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(20);

            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, 3, cb, false, out int cu, out int bu, out bool complete);
            Assert.That(cu, Is.EqualTo(3));
            Assert.That(bu, Is.EqualTo(5));
            Assert.That(cb.Length, Is.EqualTo(5));
            Assert.That(complete, Is.True);

            e.Reset();
            cb.Reset();
            cb.Produce(17);
            cb.Consume(17);
            e.Convert(c, 0, 4, cb, false, out cu, out bu, out complete);
            Assert.That(cu, Is.EqualTo(4));
            Assert.That(bu, Is.EqualTo(6));
            Assert.That(cb.Length, Is.EqualTo(6));
            Assert.That(complete, Is.True);
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Full1()
        {
            char[] c = { 'A', 'B', '€', 'C' };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(20);

            cb.Produce(17);
            cb.Consume(3);
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, 4, cb, false, out int cu, out int bu, out bool complete);
            Assert.That(cu, Is.EqualTo(4));
            Assert.That(bu, Is.EqualTo(6));
            Assert.That(cb.Length, Is.EqualTo(20));
            Assert.That(complete, Is.True);
            Assert.That(cb[14], Is.EqualTo(0x41));
            Assert.That(cb[15], Is.EqualTo(0x42));
            Assert.That(cb[16], Is.EqualTo(0xE2));
            Assert.That(cb[17], Is.EqualTo(0x82));
            Assert.That(cb[18], Is.EqualTo(0xAC));
            Assert.That(cb[19], Is.EqualTo(0x43));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Full2()
        {
            char[] c = { 'A', 'B', '€', 'C' };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(20);

            cb.Produce(14);
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, 4, cb, false, out int cu, out int bu, out bool complete);
            Assert.That(cu, Is.EqualTo(4));
            Assert.That(bu, Is.EqualTo(6));
            Assert.That(cb.Length, Is.EqualTo(20));
            Assert.That(complete, Is.True);
            Assert.That(cb[14], Is.EqualTo(0x41));
            Assert.That(cb[15], Is.EqualTo(0x42));
            Assert.That(cb[16], Is.EqualTo(0xE2));
            Assert.That(cb[17], Is.EqualTo(0x82));
            Assert.That(cb[18], Is.EqualTo(0xAC));
            Assert.That(cb[19], Is.EqualTo(0x43));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Overfill()
        {
            Encoder e = Encoding.UTF8.GetEncoder();
            char[] c = { 'A', 'B', '€', 'C' };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(20);

            for (int i = 15; i < 20; i++) {
                Console.WriteLine("With {0} bytes free", 20 - i);
                e.Reset();
                cb.Reset();
                cb.Produce(i);
                e.Convert(c, 0, 4, cb, false, out int cu, out int bu, out bool complete);
                Console.WriteLine("  cu={0}; bu={1}", cu, bu);
                Assert.That(complete, Is.False);
                Assert.That(bu <= 20 - i, Is.True);
            }
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Boundaries()
        {
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[24];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            // Based on the test "Encoder_Boundaries"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, false, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(24));
            Assert.That(cu, Is.EqualTo(22));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Boundaries2()
        {
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[24];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 12, 0);

            // Based on the test "Encoder_Boundaries", but ensures the MBCS character is properly wrapped
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, false, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(24));
            Assert.That(cu, Is.EqualTo(22));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_BoundariesFlush()
        {
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[24];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, true, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(24));
            Assert.That(cu, Is.EqualTo(22));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Boundaries2Flush()
        {
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[24];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 12, 0);

            // Based on the test "Encoder_BoundariesFlush", but ensures the MBCS character is properly wrapped
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, true, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(24));
            Assert.That(cu, Is.EqualTo(22));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_InsufficientByteSpace()
        {
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[20];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 10, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, false, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(20));
            Assert.That(cu, Is.EqualTo(18));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_InsufficientByteSpaceFlush()
        {
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[20];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 10, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, true, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(20));
            Assert.That(cu, Is.EqualTo(18));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_InsufficientByteSpace2()
        {
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[16];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 10, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, false, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(15));
            Assert.That(cu, Is.EqualTo(15));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_InsufficientByteSpace2Flush()
        {
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[16];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 10, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, true, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(15));
            Assert.That(cu, Is.EqualTo(15));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Utf16Chars1()
        {
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[16];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, true, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(16));
            Assert.That(cu, Is.EqualTo(14));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Utf16Chars1a()
        {
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[16];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 3, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, true, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(16));
            Assert.That(cu, Is.EqualTo(14));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Utf16Chars2a()
        {
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[15];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, true, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(12));
            Assert.That(cu, Is.EqualTo(12));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Utf16Chars2b()
        {
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[15];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 2, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 0, c.Length, cb, true, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.False);
            Assert.That(bu, Is.EqualTo(12));
            Assert.That(cu, Is.EqualTo(12));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Utf16Chars3a()
        {
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[4];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 12, 2, cb, true, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(4));
            Assert.That(cu, Is.EqualTo(2));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Utf16Chars3b()
        {
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[4];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 2, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            e.Convert(c, 12, 2, cb, true, out int cu, out int bu, out bool complete);
            Assert.That(complete, Is.True);
            Assert.That(bu, Is.EqualTo(4));
            Assert.That(cu, Is.EqualTo(2));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Utf16Chars4a()
        {
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[3];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            Assert.That(() => {
                e.Convert(c, 12, 2, cb, true, out _, out _, out _);
            }, Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("bytes"));
        }

        [Test]
        public void CircularBufferExt_EncoderConvert_Utf16Chars4b()
        {
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[3];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 1, 0);

            // Based on the test "Encoder_BoundariesFlush"
            Encoder e = Encoding.UTF8.GetEncoder();
            Assert.That(() => {
                e.Convert(c, 12, 2, cb, true, out _, out _, out _);
            }, Throws.TypeOf<ArgumentException>().With.Property("ParamName").EqualTo("bytes"));
        }
    }
}
