// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.Datastructures.CircularBufferTest
{
    using System;
    using System.Text;
    using NUnit.Framework;
    using Datastructures;

    [TestFixture]
    public class CircularBufferTest
    {
        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBuffer_ProduceConsume()
        {
            CircularBuffer<byte> cb = new CircularBuffer<byte>(50);
            Assert.AreEqual(50, cb.Capacity);

            // Initial state
            Assert.AreEqual(0, cb.Start);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(0, cb.ReadLength);
            Assert.AreEqual(0, cb.End);
            Assert.AreEqual(50, cb.Free);
            Assert.AreEqual(50, cb.WriteLength);

            // Test 1: Allocate 50 bytes
            cb.Produce(50);
            Assert.AreEqual(0, cb.Start);
            Assert.AreEqual(50, cb.Length);
            Assert.AreEqual(50, cb.ReadLength);
            Assert.AreEqual(0, cb.End);
            Assert.AreEqual(0, cb.Free);
            Assert.AreEqual(0, cb.WriteLength);

            // Test 2: Free 50 bytes
            cb.Consume(50);
            Assert.AreEqual(0, cb.Start);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(0, cb.ReadLength);
            Assert.AreEqual(0, cb.End);
            Assert.AreEqual(50, cb.Free);
            Assert.AreEqual(50, cb.WriteLength);

            // Test 3: Allocate 25 bytes
            cb.Produce(25);
            Assert.AreEqual(0, cb.Start);
            Assert.AreEqual(25, cb.Length);
            Assert.AreEqual(25, cb.ReadLength);
            Assert.AreEqual(25, cb.End);
            Assert.AreEqual(25, cb.Free);
            Assert.AreEqual(25, cb.WriteLength);

            // Test 4: Free 24 bytes
            cb.Consume(24);
            Assert.AreEqual(24, cb.Start);
            Assert.AreEqual(1, cb.Length);
            Assert.AreEqual(1, cb.ReadLength);
            Assert.AreEqual(25, cb.End);
            Assert.AreEqual(49, cb.Free);
            Assert.AreEqual(25, cb.WriteLength);

            // Test 5: Alocate 49 bytes
            cb.Produce(49);
            Assert.AreEqual(24, cb.Start);
            Assert.AreEqual(50, cb.Length);
            Assert.AreEqual(26, cb.ReadLength);
            Assert.AreEqual(24, cb.End);
            Assert.AreEqual(0, cb.Free);
            Assert.AreEqual(0, cb.WriteLength);

            // Test 6: Reset
            cb.Reset();
            Assert.AreEqual(0, cb.Start);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(0, cb.ReadLength);
            Assert.AreEqual(0, cb.End);
            Assert.AreEqual(50, cb.Free);
            Assert.AreEqual(50, cb.WriteLength);

            // Test 7: Test full wrapping around
            cb.Produce(25);
            cb.Consume(25);
            cb.Produce(50);
            Assert.AreEqual(25, cb.Start);
            Assert.AreEqual(50, cb.Length);
            Assert.AreEqual(25, cb.ReadLength);
            Assert.AreEqual(25, cb.End);
            Assert.AreEqual(0, cb.Free);
            Assert.AreEqual(0, cb.WriteLength);

            // Test 8: Free all data
            cb.Consume(50);
            Assert.AreEqual(25, cb.Start);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(0, cb.ReadLength);
            Assert.AreEqual(25, cb.End);
            Assert.AreEqual(50, cb.Free);
            Assert.AreEqual(25, cb.WriteLength);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBuffer_Indexing()
        {
            CircularBuffer<byte> cb = new CircularBuffer<byte>(50);

            // Write into the array directly
            for (int i = 0; i < cb.Array.Length; i++) {
                cb.Array[i] = (byte)i;
            }
            cb.Produce(50);
            Assert.AreEqual(50, cb.Length);
            Assert.AreEqual(0, cb.Start);

            // Access the array using the indexer
            for (int i = 0; i < cb.Length; i++) {
                Assert.AreEqual(i, cb[i]);
            }

            cb.Consume(25);
            cb.Produce(25);

            // Now the start is in the middle
            Assert.AreEqual(25, cb.Start);
            Assert.AreEqual(50, cb.Length);
            for (int i = 0; i < cb.Length; i++) {
                Assert.AreEqual((i + 25) % 50, cb[i], "Index " + i.ToString());
            }

            for (int i = 0; i < cb.Length; i++) {
                Assert.AreEqual((i + 25) % 50, cb.Array[cb.ToArrayIndex(i)], "Index " + i.ToString());
            }
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBuffer_ReadBlock()
        {
            CircularBuffer<byte> cb = new CircularBuffer<byte>(50);
            
            // Move the pointer to the middle
            cb.Produce(25);
            cb.Consume(25);

            // Now allocate all space
            cb.Produce(25);
            cb.Produce(25);

            Assert.AreEqual(25, cb.ReadLength);
            Assert.AreEqual(25, cb.GetReadBlock(0));
            Assert.AreEqual(20, cb.GetReadBlock(5));
            Assert.AreEqual(1, cb.GetReadBlock(24));
            Assert.AreEqual(25, cb.GetReadBlock(25));
            Assert.AreEqual(20, cb.GetReadBlock(30));
            Assert.AreEqual(1, cb.GetReadBlock(49));
            Assert.AreEqual(0, cb.GetReadBlock(50));
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBuffer_Revert()
        {
            CircularBuffer<byte> cb = new CircularBuffer<byte>(50);

            // Move the pointer to the middle
            cb.Produce(25);
            cb.Consume(25);

            // Now allocate all space
            cb.Produce(25);
            cb.Produce(25);

            Assert.AreEqual(25, cb.Start);
            Assert.AreEqual(25, cb.ReadLength);
            Assert.AreEqual(25, cb.End);
            Assert.AreEqual(0, cb.WriteLength);

            cb.Revert(5);
            Assert.AreEqual(25, cb.Start);
            Assert.AreEqual(25, cb.ReadLength);
            Assert.AreEqual(20, cb.End);
            Assert.AreEqual(5, cb.WriteLength);

            cb.Revert(20);
            Assert.AreEqual(25, cb.Start);
            Assert.AreEqual(25, cb.ReadLength);
            Assert.AreEqual(0, cb.End);
            Assert.AreEqual(25, cb.WriteLength);

            cb.Revert(20);
            Assert.AreEqual(25, cb.Start);
            Assert.AreEqual(5, cb.ReadLength);
            Assert.AreEqual(30, cb.End);
            Assert.AreEqual(20, cb.WriteLength);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
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
                Assert.AreEqual(rd[i], cb[i], "Index " + i.ToString() + " doesn't match");
            }
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_GetStringSimple()
        {
            CircularBuffer<char> cb = new CircularBuffer<char>(15);
            for (int i = 0; i < cb.Capacity; i++) {
                cb.Array[i] = (char)((int)'A' + i);
            }

            cb.Produce(10);
            Assert.AreEqual("ABCDEFGHIJ", cb.GetString());
            cb.Consume(5);
            Assert.AreEqual("FGHIJ", cb.GetString());
            cb.Produce(8);
            Assert.AreEqual("FGHIJKLMNOABC", cb.GetString());
            cb.Consume(13);
            Assert.AreEqual("", cb.GetString());

            cb = null;
            Assert.IsNull(cb.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_GetStringLength()
        {
            CircularBuffer<char> cb = new CircularBuffer<char>(15);
            for (int i = 0; i < cb.Capacity; i++) {
                cb.Array[i] = (char)((int)'A' + i);
            }

            cb.Produce(10);
            Assert.AreEqual("ABCDEFGHIJ", cb.GetString(10));
            Assert.AreEqual("ABCDEFGHIJ", cb.GetString(20));
            Assert.AreEqual("ABCDE", cb.GetString(5));
            Assert.AreEqual("A", cb.GetString(1));
            Assert.AreEqual("", cb.GetString(0));
            cb.Consume(5);
            Assert.AreEqual("FGHIJ", cb.GetString(10));
            Assert.AreEqual("FGHIJ", cb.GetString(5));
            Assert.AreEqual("FGH", cb.GetString(3));
            Assert.AreEqual("", cb.GetString(0));
            cb.Produce(8);
            Assert.AreEqual("FGHIJKLMNOABC", cb.GetString(13));
            Assert.AreEqual("FGHIJ", cb.GetString(5));
            Assert.AreEqual("F", cb.GetString(1));
            Assert.AreEqual("", cb.GetString(0));
            cb.Consume(13);
            Assert.AreEqual("", cb.GetString(0));
            Assert.AreEqual("", cb.GetString(15));
            Assert.AreEqual("", cb.GetString(20));

            cb = null;
            Assert.IsNull(cb.GetString(5));
            Assert.IsNull(cb.GetString(0));
            Assert.IsNull(cb.GetString(15));
            Assert.IsNull(cb.GetString(20));
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_GetStringOffsetLength()
        {
            CircularBuffer<char> cb = new CircularBuffer<char>(15);
            for (int i = 0; i < cb.Capacity; i++) {
                cb.Array[i] = (char)((int)'A' + i);
            }

            cb.Produce(10);
            Assert.AreEqual("ABCDEFGHIJ", cb.GetString(0, 10));
            Assert.AreEqual("ABCDEFGHIJ", cb.GetString(0, 20));
            Assert.AreEqual("", cb.GetString(0, 0));
            Assert.AreEqual("FGHIJ", cb.GetString(5, 5));
            Assert.AreEqual("BCDEFGHIJ", cb.GetString(1, 9));
            Assert.AreEqual("BCDEFGH", cb.GetString(1, 7));
            Assert.AreEqual("", cb.GetString(5, 0));
            cb.Consume(5);
            Assert.AreEqual("FGHIJ", cb.GetString(0, 10));
            Assert.AreEqual("FGHIJ", cb.GetString(0, 5));
            Assert.AreEqual("HIJ", cb.GetString(2, 3));
            Assert.AreEqual("", cb.GetString(5, 0));
            Assert.AreEqual("", cb.GetString(5, 1));
            cb.Produce(8);
            Assert.AreEqual("FGHIJKLMNOABC", cb.GetString(0, 13));
            Assert.AreEqual("FGHIJ", cb.GetString(0, 5));
            Assert.AreEqual("KLMNOABC", cb.GetString(5, 13));
            Assert.AreEqual("KLMNOABC", cb.GetString(5, 8));
            Assert.AreEqual("KLMNO", cb.GetString(5, 5));
            Assert.AreEqual("ABC", cb.GetString(10, 3));
            cb.Consume(13);
            Assert.AreEqual("", cb.GetString(0, 0));
            Assert.AreEqual("", cb.GetString(5, 15));
            Assert.AreEqual("", cb.GetString(10, 20));
            Assert.AreEqual("", cb.GetString(10, 1));

            cb = null;
            Assert.IsNull(cb.GetString(0, 5));
            Assert.IsNull(cb.GetString(10, 0));
            Assert.IsNull(cb.GetString(5, 15));
            Assert.IsNull(cb.GetString(2, 20));
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBuffer_AppendArray()
        {
            CircularBuffer<char> cb = new CircularBuffer<char>(20);
            cb.Produce(10);
            cb.Consume(9);

            Assert.AreEqual(1, cb.Length);
            Assert.AreEqual(19, cb.Free);

            cb.Append("ABCDEFGHIJKLMN".ToCharArray(), 0, 14);
            Assert.AreEqual(15, cb.Length);
            Assert.AreEqual(5, cb.Free);

            Assert.AreEqual("ABCDEFGHIJKLMN", cb.GetString(1, 14));
            cb.Consume(15);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(20, cb.Free);

            cb.Append("12345678901234567890".ToCharArray());
            Assert.AreEqual(0, cb.Free);
            Assert.AreEqual(20, cb.Length);
            Assert.AreEqual("12345678901234567890", cb.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBuffer_AppendBuffer()
        {
            CircularBuffer<char> cb1 = new CircularBuffer<char>(20);
            CircularBuffer<char> cb2 = new CircularBuffer<char>(20);

            // Read is one chunk, write is one chunk
            cb2.Produce(3);
            cb2.Append("123456789012345".ToCharArray());
            cb2.Consume(3);

            cb1.Append(cb2);
            Assert.AreEqual(15, cb1.Length);
            Assert.AreEqual(5, cb1.Free);
            Assert.AreEqual("123456789012345", cb1.GetString());

            // Write is one chunk, but read is two chunks
            cb1.Reset();
            cb2.Reset();
            cb2.Produce(15);
            cb2.Consume(14);
            cb2.Append("123456789012345".ToCharArray());
            cb2.Consume(1);
            
            cb1.Append(cb2);
            Assert.AreEqual(15, cb1.Length);
            Assert.AreEqual(5, cb1.Free);
            Assert.AreEqual("123456789012345", cb1.GetString());

            // Write is two chunks, read is one chunk
            cb1.Reset();
            cb2.Reset();
            cb1.Produce(10);
            cb1.Consume(9);
            cb2.Append("123456789012345".ToCharArray());
            
            cb1.Append(cb2);
            cb1.Consume(1);
            Assert.AreEqual(15, cb1.Length);
            Assert.AreEqual(5, cb1.Free);
            Assert.AreEqual("123456789012345", cb1.GetString());

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
            Assert.AreEqual(15, cb1.Length);
            Assert.AreEqual(5, cb1.Free);
            Assert.AreEqual("123456789012345", cb1.GetString());

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
            Assert.AreEqual(15, cb1.Length);
            Assert.AreEqual(5, cb1.Free);
            Assert.AreEqual("123456789012345", cb1.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
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
            Assert.AreEqual(m.Length, cb1.Length);
            Assert.AreEqual(0, cb1.Free);
            Assert.AreEqual(0, cb1.Start);
            Assert.AreEqual(0x80, cb1[0]);

            CircularBuffer<byte> cb2 = new CircularBuffer<byte>(m, 10);
            Assert.AreEqual(10, cb2.Length);
            Assert.AreEqual(m.Length - 10, cb2.Free);
            Assert.AreEqual(0, cb2.Start);
            Assert.AreEqual(0x80, cb2[0]);

            CircularBuffer<byte> cb3 = new CircularBuffer<byte>(m, 15, 10);
            Assert.AreEqual(10, cb3.Length);
            Assert.AreEqual(m.Length - 10, cb3.Free);
            Assert.AreEqual(15, cb3.Start);
            Assert.AreEqual(0x02, cb3[0]);
        }

        /// <summary>
        /// Check converting a byte array to a char array with convert works
        /// </summary>
        /// <remarks>
        /// This test places one byte of the Euro symbol at the end of the byte array
        /// that wraps around, to ensure multibyte arrays are handled correctly.
        /// </remarks>
        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_Boundaries()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();

            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[28];

            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            int bu;
            int cu;
            bool complete;
            
            // Based on the test "Decoder_Boundaries1"
            d.Convert(cb, c, 0, c.Length, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(m.Length, bu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(c.Length, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN", new string(c));
        }

        /// <summary>
        /// Check converting a byte array to a char array with convert works
        /// </summary>
        /// <remarks>
        /// This test places one byte of the Euro symbol at the end of the byte array
        /// that wraps around, to ensure multibyte arrays are handled correctly.
        /// </remarks>
        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_BoundariesFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();

            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[28];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Boundaries2"
            d.Convert(cb, c, 0, c.Length, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(m.Length, bu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(c.Length, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN", new string(c));
        }

        /// <summary>
        /// Check converting a byte array to a char array with convert works
        /// </summary>
        /// <remarks>
        /// The test places the Euro symbol at the end of the byte array and
        /// wraps over to the beginning. There is insufficient bytes in the
        /// char array to convert everything, but enough to convert the Euro
        /// symbol
        /// </remarks>
        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_InsufficientCharSpace()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();

            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[20];

            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_InsufficientCharSpace"
            d.Convert(cb, c, 0, c.Length, false, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(22, bu);
            Assert.AreEqual(8, cb.Length);
            Assert.AreEqual(c.Length, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEF", new string(c));
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_InsufficientCharSpaceFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();

            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[20];

            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_InsufficientCharSpaceFlush"
            d.Convert(cb, c, 0, c.Length, true, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(22, bu);
            Assert.AreEqual(8, cb.Length);
            Assert.AreEqual(c.Length, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEF", new string(c));
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_InsufficientCharSpaceMbcs1()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();

            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[13];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_InsufficientCharSpaceMbcs1"
            d.Convert(cb, c, 0, c.Length, false, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(15, bu);
            Assert.AreEqual(15, cb.Length);
            Assert.AreEqual(c.Length, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€", new string(c));
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_IncompleteBuff()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();

            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 13);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_IncompleteBuff"
            d.Convert(cb, c, 0, c.Length, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(13, bu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(12, cu);
            Assert.AreEqual("OPQRSTUVWXYZ", new string(c, 0, cu));

            cb.Produce(17);
            d.Convert(cb, c, 0, c.Length, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(17, bu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(16, cu);
            Assert.AreEqual("€@ABCDEFGHIJKLMN", new string(c, 0, 16));
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_IncompleteBuffFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 14);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_IncompleteBuffFlush"
            d.Convert(cb, c, 0, c.Length, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(14, bu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(13, cu);
            Assert.AreEqual("OPQRSTUVWXYZ.", new string(c, 0, 13));
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_IncompleteBuffFlush2()
        {
            int bu;
            int cu;
            bool complete;
            char[] c = new char[30];

            // Test the decoder on the circular buffer
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 14);

            // Based on the test "Decoder_IncompleteBuffFlush2"
            d.Convert(cb, c, 0, 12, true, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(12, bu);
            Assert.AreEqual(12, cu);
            Assert.AreEqual("OPQRSTUVWXYZ", new string(c, 0, 12));
            Assert.AreEqual(2, cb.Length);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_Utf16Chars1()
        {
            int bu;
            int cu;
            bool complete;
            char[] c = new char[30];

            // Test the decoder on the circular buffer
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 16);

            // Based on the test "Decoder_Utf16Chars1"
            d.Convert(cb, c, 0, 14, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(16, bu);
            Assert.AreEqual(14, cu);
            Assert.AreEqual("OPQRSTUVWXYZ\uDB40\uDC84", new string(c, 0, 14));
            Assert.AreEqual(0, cb.Length);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_Utf16Chars2()
        {
            int bu;
            int cu;
            bool complete;
            char[] c = new char[30];

            // Test the decoder on the circular buffer
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 16);

            // Based on the test "Decoder_Utf16Chars2"
            d.Convert(cb, c, 0, 13, false, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            //Assert.AreEqual(12, bu);
            Assert.AreEqual(12, cu);
            Assert.AreEqual("OPQRSTUVWXYZ", new string(c, 0, 12));
            //Assert.AreEqual(4, cb.Length);

            // This particular test is hard. The decoder consumes 12 bytes, but our function
            // consumes more (because the 4-bytes cross a boundary). The decoder needs to see
            // all four bytes to decide not to convert it. There is nothing in the documentation
            // to say that the decoder should behave this way. So we can't simulate the original
            // behaviour in this case.
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert1_Utf16Chars3()
        {
            int bu;
            int cu;
            bool complete;
            char[] c = new char[30];

            // Test the decoder on the circular buffer
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 28, 4);
            bool exception;

            // Based on the test "Decoder_Utf16Chars3"

            // The behaviour isn't the same due to non-documented behaviour. MS documentation
            // doesn't say if an exception should occur or not and it has slightly inconsistent
            // behaviour if we sent 3 bytes (of 4), or 4 bytes at once.

            exception = false;
            try {
                d.Convert(cb, c, 0, 1, false, out bu, out cu, out complete);
            } catch (System.ArgumentException e) {
                if (!e.ParamName.Equals("chars")) throw;
                exception = true;
                cu = -1;
            }
            //Assert.IsTrue(exception);
            if (!exception) {
                Assert.AreEqual(0, cu);
                try {
                    d.Convert(cb, c, 0, 1, false, out bu, out cu, out complete);
                } catch (System.ArgumentException e) {
                    if (!e.ParamName.Equals("chars")) throw;
                    exception = true;
                    cu = -1;
                }
                Assert.IsTrue(exception);
            }

            cb = new CircularBuffer<byte>(m, 28, 4);
            d.Reset();
            exception = false;
            try {
                d.Convert(cb, c, 0, 1, true, out bu, out cu, out complete);
            } catch (System.ArgumentException e) {
                if (!e.ParamName.Equals("chars")) throw;
                exception = true;
                cu = -1;
            }
            //Assert.IsTrue(exception);
            if (!exception) {
                Assert.AreEqual(0, cu);
                try {
                    d.Convert(cb, c, 0, 1, false, out bu, out cu, out complete);
                } catch (System.ArgumentException e) {
                    if (!e.ParamName.Equals("chars")) throw;
                    exception = true;
                    cu = -1;
                }
                Assert.IsTrue(exception);
            }

            d.Convert(cb, c, 0, 2, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            //Assert.AreEqual(4, bu);
            Assert.AreEqual(2, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_Boundaries()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[28];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Boundaries"
            d.Convert(cb, cc, cc.Free, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(m.Length, bu);
            Assert.AreEqual(cc.Capacity, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_BoundariesFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[28];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Boundaries"
            d.Convert(cb, cc, cc.Free, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(m.Length, bu);
            Assert.AreEqual(cc.Capacity, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_InsufficientCharSpace()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[20];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_InsufficientCharSpace"
            d.Convert(cb, cc, cc.Free, false, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(22, bu);
            Assert.AreEqual(cc.Capacity, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEF", cc.GetString());
            Assert.AreEqual(8, cb.Length);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_InsufficientCharSpaceFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[20];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_InsufficientCharSpaceFlush"
            d.Convert(cb, cc, cc.Free, true, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(22, bu);
            Assert.AreEqual(cc.Capacity, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEF", cc.GetString());
            Assert.AreEqual(8, cb.Length);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_InsufficientCharSpaceMbcs1()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[13];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, m.Length);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_InsufficientCharSpaceMbcs1"
            d.Convert(cb, cc, cc.Free, false, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(15, bu);
            Assert.AreEqual(cc.Capacity, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_IncompleteBuff()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 13);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_IncompleteBuff"
            d.Convert(cb, cc, cc.Free, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(13, bu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(12, cu);
            Assert.AreEqual("OPQRSTUVWXYZ", cc.GetString());
            cc.Consume(cu);

            cb.Produce(17);
            d.Convert(cb, cc, cc.Free, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(17, bu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(16, cu);
            Assert.AreEqual("€@ABCDEFGHIJKLMN", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_IncompleteBuffFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 14);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_IncompleteBuffFlush"
            d.Convert(cb, cc, cc.Free, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(14, bu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(13, cu);
            Assert.AreEqual("OPQRSTUVWXYZ.", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_IncompleteBuffFlush2()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 14);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_IncompleteBuffFlush2"
            d.Convert(cb, cc, 12, true, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(12, bu);
            Assert.AreEqual(12, cu);
            Assert.AreEqual("OPQRSTUVWXYZ", cc.GetString());
            Assert.AreEqual(2, cb.Length);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_Utf16Chars1a()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 16);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Utf16Chars1"
            d.Convert(cb, cc, 14, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(16, bu);
            Assert.AreEqual(14, cu);
            Assert.AreEqual("OPQRSTUVWXYZ\uDB40\uDC84", cc.GetString());
            Assert.AreEqual(0, cb.Length);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_Utf16Chars1b()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 16);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 17, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Utf16Chars1"
            // - We force the 2 chars to wrap
            d.Convert(cb, cc, 14, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(16, bu);
            Assert.AreEqual(14, cu);
            Assert.AreEqual("OPQRSTUVWXYZ\uDB40\uDC84", cc.GetString());
            Assert.AreEqual(0, cb.Length);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_Utf16Chars2()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 16, 16);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Utf16Chars2"
            d.Convert(cb, cc, 13, false, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            //Assert.AreEqual(12, bu);
            Assert.AreEqual(12, cu);
            Assert.AreEqual("OPQRSTUVWXYZ", cc.GetString());
            //Assert.AreEqual(4, cb.Length);

            // This particular test is hard. The decoder consumes 12 bytes, but our function
            // consumes more (because the 4-bytes cross a boundary). The decoder needs to see
            // all four bytes to decide not to convert it. There is nothing in the documentation
            // to say that the decoder should behave this way. So we can't simulate the original
            // behaviour in this case.
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_Utf16Chars3()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x82, 0x84, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0
            };
            char[] c = new char[30];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 28, 4);
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 5, 0);

            bool exception;
            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Utf16Chars3"

            // The behaviour isn't the same due to non-documented behaviour. MS documentation
            // doesn't say if an exception should occur or not and it has slightly inconsistent
            // behaviour if we sent 3 bytes (of 4), or 4 bytes at once.

            exception = false;
            try {
                d.Convert(cb, cc, 1, false, out bu, out cu, out complete);
            } catch (System.ArgumentException e) {
                if (!e.ParamName.Equals("chars")) throw;
                exception = true;
                cu = -1;
            }
            //Assert.IsTrue(exception);
            if (!exception) {
                Assert.AreEqual(0, cu);
                try {
                    d.Convert(cb, cc, 1, false, out bu, out cu, out complete);
                } catch (System.ArgumentException e) {
                    if (!e.ParamName.Equals("chars")) throw;
                    exception = true;
                    cu = -1;
                }
                Assert.IsTrue(exception);
            }

            cb = new CircularBuffer<byte>(m, 28, 4);
            d.Reset();
            exception = false;
            try {
                d.Convert(cb, cc, 1, true, out bu, out cu, out complete);
            } catch (System.ArgumentException e) {
                if (!e.ParamName.Equals("chars")) throw;
                exception = true;
                cu = -1;
            }
            //Assert.IsTrue(exception);
            if (!exception) {
                Assert.AreEqual(0, cu);
                try {
                    d.Convert(cb, cc, 1, false, out bu, out cu, out complete);
                } catch (System.ArgumentException e) {
                    if (!e.ParamName.Equals("chars")) throw;
                    exception = true;
                    cu = -1;
                }
                Assert.IsTrue(exception);
            }

            d.Convert(cb, cc, 2, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            //Assert.AreEqual(4, bu);
            Assert.AreEqual(2, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_CircularFull()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();

            byte[] b = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(b, 16, b.Length);

            char[] c = new char[30];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 28, 0);

            int bu;
            int cu;
            bool complete;
            d.Convert(cb, cc, cc.Capacity, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(b.Length, bu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(28, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_MultiChar1()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            
            byte[] b = { 
                0xF3, 0xA0, 0x82, 0x84, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40, 0x41,
                0x42, 0x43, 0x44, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55
            };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(b, 0, b.Length);

            char[] c = new char[30];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 29, 0);
            int bu;
            int cu;
            bool complete;
            d.Convert(cb, cc, cc.Capacity, false, out bu, out cu, out complete);

            //Assert.IsFalse(complete);
            Assert.AreEqual(30, cu);
            Assert.AreEqual(32, bu);
            Assert.AreEqual(0, cc.Free);
            Assert.AreEqual("\uDB40\uDC840123456789@ABCDIJKLMNOPQRSTU", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert2_MultiChar2()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();

            byte[] b = { 
                0xF3, 0xA0, 0x82, 0x84, 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x40, 0x41,
                0x42, 0x43, 0x44, 0x49, 0x4a, 0x4b, 0x4c, 0x4d, 0x4e, 0x4f, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55
            };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(b, 4, b.Length);

            char[] c = new char[29];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 0, 0);
            int bu;
            int cu;
            bool complete;
            d.Convert(cb, cc, cc.Capacity, false, out bu, out cu, out complete);

            // The character buffer doesn't contain enough space to capture the last two-byte character. Just like
            // the real decoder, it should capture as much as possible and return without an error.
            Assert.IsFalse(complete);
            Assert.AreEqual(28, cu);
            Assert.AreEqual(28, bu);
            Assert.AreEqual(1, cc.Free);
            Assert.AreEqual("0123456789@ABCDIJKLMNOPQRSTU", cc.GetString());

            // There are no bytes to convert, an exception should be raised like the real decoder in a char[].
            try {
                d.Convert(cb, cc, cc.Free, false, out bu, out cu, out complete);
            } catch (System.ArgumentException e) {
                if (!e.ParamName.Equals("chars")) throw;
            }
        }
        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert3_BoundariesWithFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();

            byte[] m = { 0x41, 0xE2, 0x82, 0xAC };
            CircularBuffer<byte> cb;
            CircularBuffer<char> cc;
            int bu;
            int cu;
            bool complete;

            cb = new CircularBuffer<byte>(m, 0, 3);
            cc = new CircularBuffer<char>(20);
            d.Convert(cb, cc, true, out bu, out cu, out complete);
            Assert.AreEqual(3, bu);
            Assert.AreEqual(2, cu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(2, cc.Length);
            Assert.AreEqual('A', cc[0]);
            Assert.AreEqual((char)0xFFFD, cc[1]);
            Assert.IsTrue(complete);

            cb = new CircularBuffer<byte>(m, 0, 3);
            cc = new CircularBuffer<char>(20);
            cc.Produce(19);
            cc.Consume(18);
            d.Convert(cb, cc, true, out bu, out cu, out complete);
            cc.Consume(1);
            Assert.AreEqual(3, bu);
            Assert.AreEqual(2, cu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(2, cc.Length);
            Assert.AreEqual('A', cc[0]);
            Assert.AreEqual((char)0xFFFD, cc[1]);
            Assert.IsTrue(complete);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert3_CircularFull2()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();

            byte[] b = { 
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E,
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82
            };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(b, 16, b.Length);

            char[] c = new char[30];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 28, 0);

            int bu;
            int cu;
            bool complete;
            d.Convert(cb, cc, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(b.Length, bu);
            Assert.AreEqual(0, cb.Length);
            Assert.AreEqual(28, cu);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_Boundaries1()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            CircularBuffer<char> cc = new CircularBuffer<char>(28);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Boundaries"
            d.Convert(m, 0, m.Length, cc, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(m.Length, bu);
            Assert.AreEqual(28, cu);
            Assert.AreEqual(28, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_Boundaries2()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[28];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 20, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Boundaries"
            d.Convert(m, 0, m.Length, cc, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(m.Length, bu);
            Assert.AreEqual(28, cu);
            Assert.AreEqual(28, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_BoundariesFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[28];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 20, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_BoundariesFlush"
            d.Convert(m, 0, m.Length, cc, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(m.Length, bu);
            Assert.AreEqual(28, cu);
            Assert.AreEqual(28, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEFGHIJKLMN", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_InsufficientCharSpace()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[20];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 15, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_InsufficientCharSpace"
            d.Convert(m, 0, m.Length, cc, false, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(22, bu);
            Assert.AreEqual(20, cu);
            Assert.AreEqual(20, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEF", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_InsufficientCharSpaceFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[20];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 15, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_InsufficientCharSpaceFlush"
            d.Convert(m, 0, m.Length, cc, true, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(22, bu);
            Assert.AreEqual(20, cu);
            Assert.AreEqual(20, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ€@ABCDEF", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_InsufficientCharSpaceMbcs1()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[13];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 4, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_InsufficientCharSpaceMbcs1"
            d.Convert(m, 0, m.Length, cc, true, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(15, bu);
            Assert.AreEqual(13, cu);
            Assert.AreEqual(13, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ€", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_IncompleteBuff()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82,
                0xAC, 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46,
                0x47, 0x48, 0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[30];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 20, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_IncompleteBuff"
            d.Convert(m, 0, 13, cc, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(13, bu);
            Assert.AreEqual(12, cu);
            Assert.AreEqual(12, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ", cc.GetString());
            cc.Consume(12);

            d.Convert(m, 13, 17, cc, false, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(17, bu);
            Assert.AreEqual(16, cu);
            Assert.AreEqual("€@ABCDEFGHIJKLMN", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_IncompleteBuffFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82, 0xAC, 0x40,
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[30];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 20, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_IncompleteBuffFlush"
            d.Convert(m, 0, 14, cc, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(14, bu);
            Assert.AreEqual(13, cu);
            Assert.AreEqual(13, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ.", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_IncompleteBuffFlush2()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xE2, 0x82, 0xAC, 0x40,
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[12];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 11, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_IncompleteBuffFlush2"
            d.Convert(m, 0, 14, cc, true, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(12, bu);
            Assert.AreEqual(12, cu);
            Assert.AreEqual(12, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_Utf16Chars1()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0, 0x82, 0x84,
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[14];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 11, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Utf16Chars1"
            d.Convert(m, 0, 16, cc, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(16, bu);
            Assert.AreEqual(14, cu);
            Assert.AreEqual(14, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ\uDB40\uDC84", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_Utf16Chars2()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0, 0x82, 0x84,
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[13];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 11, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Decoder_Utf16Chars2"
            d.Convert(m, 0, 16, cc, false, out bu, out cu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(12, bu);
            Assert.AreEqual(12, cu);
            Assert.AreEqual(12, cc.Length);
            Assert.AreEqual("OPQRSTUVWXYZ", cc.GetString());
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_DecoderConvert4_Utf16Chars3()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8", new EncoderReplacementFallback("."), new DecoderReplacementFallback("."));
            Decoder d = enc.GetDecoder();
            byte[] m = { 
                0x4F, 0x50, 0x51, 0x52, 0x53, 0x54, 0x55, 0x56,
                0x57, 0x58, 0x59, 0x5A, 0xF3, 0xA0, 0x82, 0x84,
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4A, 0x4B, 0x4C, 0x4D, 0x4E
            };
            char[] c = new char[1];
            CircularBuffer<char> cc = new CircularBuffer<char>(c, 0, 0);

            int bu;
            int cu;
            bool complete;
            bool exception;

            // Based on the test "Decoder_Utf16Chars3"

            // We expect this to fail, as a two-char Unicode character doesn't fit in one byte
            exception = false;
            try {
                d.Convert(m, 12, 10, cc, false, out bu, out cu, out complete);
            } catch (System.ArgumentException e) {
                if (!e.ParamName.Equals("chars")) throw;
                exception = true;
            }
            Assert.IsTrue(exception);

            // We expect this to fail, as a two-char Unicode character doesn't fit in one byte
            exception = false;
            try {
                d.Convert(m, 12, 10, cc, true, out bu, out cu, out complete);
            } catch (System.ArgumentException e) {
                if (!e.ParamName.Equals("chars")) throw;
                exception = true;
            }
            Assert.IsTrue(exception);

            c = new char[2];
            cc = new CircularBuffer<char>(c, 1, 0);
            d.Convert(m, 12, 4, cc, true, out bu, out cu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(4, bu);
            Assert.AreEqual(2, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();

            char[] c = { 'A', 'B', '€', 'C' };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(20);
            int bu;
            int cu;
            bool complete;

            e.Convert(c, 0, 3, cb, false, out cu, out bu, out complete);
            Assert.AreEqual(3, cu);
            Assert.AreEqual(5, bu);
            Assert.AreEqual(5, cb.Length);
            Assert.IsTrue(complete);

            e.Reset();
            cb.Reset();
            cb.Produce(17);
            cb.Consume(17);
            e.Convert(c, 0, 4, cb, false, out cu, out bu, out complete);
            Assert.AreEqual(4, cu);
            Assert.AreEqual(6, bu);
            Assert.AreEqual(6, cb.Length);
            Assert.IsTrue(complete);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Full1()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();

            char[] c = { 'A', 'B', '€', 'C' };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(20);
            int bu;
            int cu;
            bool complete;

            cb.Produce(17);
            cb.Consume(3);
            e.Convert(c, 0, 4, cb, false, out cu, out bu, out complete);
            Assert.AreEqual(4, cu);
            Assert.AreEqual(6, bu);
            Assert.AreEqual(20, cb.Length);
            Assert.IsTrue(complete);
            Assert.AreEqual(0x41, cb[14]);
            Assert.AreEqual(0x42, cb[15]);
            Assert.AreEqual(0xE2, cb[16]);
            Assert.AreEqual(0x82, cb[17]);
            Assert.AreEqual(0xAC, cb[18]);
            Assert.AreEqual(0x43, cb[19]);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Full2()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();

            char[] c = { 'A', 'B', '€', 'C' };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(20);
            int bu;
            int cu;
            bool complete;

            cb.Produce(14);
            e.Convert(c, 0, 4, cb, false, out cu, out bu, out complete);
            Assert.AreEqual(4, cu);
            Assert.AreEqual(6, bu);
            Assert.AreEqual(20, cb.Length);
            Assert.IsTrue(complete);
            Assert.AreEqual(0x41, cb[14]);
            Assert.AreEqual(0x42, cb[15]);
            Assert.AreEqual(0xE2, cb[16]);
            Assert.AreEqual(0x82, cb[17]);
            Assert.AreEqual(0xAC, cb[18]);
            Assert.AreEqual(0x43, cb[19]);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Overfill()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();

            char[] c = { 'A', 'B', '€', 'C' };
            CircularBuffer<byte> cb = new CircularBuffer<byte>(20);
            int bu;
            int cu;
            bool complete;

            for (int i = 15; i < 20; i++) {
                System.Diagnostics.Trace.WriteLine("With " + (20 - i).ToString() + " bytes free");
                e.Reset();
                cb.Reset();
                cb.Produce(i);
                e.Convert(c, 0, 4, cb, false, out cu, out bu, out complete);
                System.Diagnostics.Trace.WriteLine("  cu=" + cu.ToString() + "; bu=" + bu.ToString());
                Assert.IsFalse(complete);
                Assert.IsTrue(bu <= 20 - i);
            }
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Boundaries()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[24];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_Boundaries"
            e.Convert(c, 0, c.Length, cb, false, out cu, out bu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(24, bu);
            Assert.AreEqual(22, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Boundaries2()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[24];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 12, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_Boundaries", but ensures the MBCS character is properly wrapped
            e.Convert(c, 0, c.Length, cb, false, out cu, out bu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(24, bu);
            Assert.AreEqual(22, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_BoundariesFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[24];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 0, c.Length, cb, true, out cu, out bu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(24, bu);
            Assert.AreEqual(22, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Boundaries2Flush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[24];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 12, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush", but ensures the MBCS character is properly wrapped
            e.Convert(c, 0, c.Length, cb, true, out cu, out bu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(24, bu);
            Assert.AreEqual(22, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_InsufficientByteSpace()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[20];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 10, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 0, c.Length, cb, false, out cu, out bu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(20, bu);
            Assert.AreEqual(18, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_InsufficientByteSpaceFlush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[20];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 10, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 0, c.Length, cb, true, out cu, out bu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(20, bu);
            Assert.AreEqual(18, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_InsufficientByteSpace2()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[16];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 10, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 0, c.Length, cb, false, out cu, out bu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(15, bu);
            Assert.AreEqual(15, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_InsufficientByteSpace2Flush()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "This is a test €100.99".ToCharArray();
            byte[] m = new byte[16];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 10, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 0, c.Length, cb, true, out cu, out bu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(15, bu);
            Assert.AreEqual(15, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Utf16Chars1()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[16];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 0, c.Length, cb, true, out cu, out bu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(16, bu);
            Assert.AreEqual(14, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Utf16Chars1a()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[16];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 3, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 0, c.Length, cb, true, out cu, out bu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(16, bu);
            Assert.AreEqual(14, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Utf16Chars2a()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[15];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 0, c.Length, cb, true, out cu, out bu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(12, bu);
            Assert.AreEqual(12, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Utf16Chars2b()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[15];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 2, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 0, c.Length, cb, true, out cu, out bu, out complete);
            Assert.IsFalse(complete);
            Assert.AreEqual(12, bu);
            Assert.AreEqual(12, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Utf16Chars3a()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[4];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 12, 2, cb, true, out cu, out bu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(4, bu);
            Assert.AreEqual(2, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Utf16Chars3b()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[4];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 2, 0);

            int bu;
            int cu;
            bool complete;

            // Based on the test "Encoder_BoundariesFlush"
            e.Convert(c, 12, 2, cb, true, out cu, out bu, out complete);
            Assert.IsTrue(complete);
            Assert.AreEqual(4, bu);
            Assert.AreEqual(2, cu);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Utf16Chars4a()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[3];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 0, 0);

            int bu;
            int cu;
            bool complete;
            bool exception;

            // Based on the test "Encoder_BoundariesFlush"
            exception = false;
            try {
                e.Convert(c, 12, 2, cb, true, out cu, out bu, out complete);
            } catch (System.ArgumentException ex) {
                if (!ex.ParamName.Equals("bytes")) throw;
                exception = true;
            }
            Assert.IsTrue(exception);
        }

        [Test]
        [Category("Datastructures/CircularBuffer")]
        public void CircularBufferExt_EncoderConvert_Utf16Chars4b()
        {
            Encoding enc = Encoding.GetEncoding("UTF-8");
            Encoder e = enc.GetEncoder();
            char[] c = "OPQRSTUVWXYZ\uDB40\uDC84".ToCharArray();
            byte[] m = new byte[3];
            CircularBuffer<byte> cb = new CircularBuffer<byte>(m, 1, 0);

            int bu;
            int cu;
            bool complete;
            bool exception;

            // Based on the test "Encoder_BoundariesFlush"
            exception = false;
            try {
                e.Convert(c, 12, 2, cb, true, out cu, out bu, out complete);
            } catch (System.ArgumentException ex) {
                if (!ex.ParamName.Equals("bytes")) throw;
                exception = true;
            }
            Assert.IsTrue(exception);
        }
    }
}
