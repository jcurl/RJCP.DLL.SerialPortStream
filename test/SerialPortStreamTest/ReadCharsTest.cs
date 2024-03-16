namespace RJCP.IO.Ports
{
    using System;
    using System.Text;
    using NUnit.Framework;
    using Serial;

    [TestFixture]
    [CancelAfter(10000)]
    public class ReadCharsTest
    {
        [Test]
        public void NewLine()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                serial.PortName = "COM";

                Assert.That(() => {
                    stream.NewLine = string.Empty;
                }, Throws.Exception.TypeOf<ArgumentException>(), "Expected exception when setting newline to empty string");

                Assert.That(() => {
                    stream.NewLine = null;
                }, Throws.Exception.TypeOf<ArgumentNullException>(), "Expected exception when setting newline to null");
            }
        }

        [Test]
        public void ReadChars()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                serial.VirtualBuffer.WriteReceivedData(send, 0, send.Length);

                char[] receive = new char[5];
                int received = stream.Read(receive, 0, receive.Length);

                Assert.That(received, Is.EqualTo(3));
                Assert.That(receive[0], Is.EqualTo('e'));
                Assert.That(receive[1], Is.EqualTo('f'));
                Assert.That(receive[2], Is.EqualTo('g'));
            }
        }

        [Test]
        public void ReadCharsWithTimeout()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 100;
                stream.Open();

                char[] receive = new char[5];
                int received = stream.Read(receive, 0, receive.Length);
                Assert.That(received, Is.EqualTo(0));

                byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                serial.VirtualBuffer.WriteReceivedData(send, 0, send.Length);

                received = stream.Read(receive, 0, receive.Length);
                Assert.That(received, Is.EqualTo(3));
                Assert.That(receive[0], Is.EqualTo('e'));
                Assert.That(receive[1], Is.EqualTo('f'));
                Assert.That(receive[2], Is.EqualTo('g'));
            }
        }

        [Test]
        public void ReadSingleChar()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                serial.VirtualBuffer.WriteReceivedData(send, 0, send.Length);

                Assert.That(stream.ReadChar(), Is.EqualTo((int)'e'));
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'f'));
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'g'));
            }
        }

        [Test]
        public void ReadSingleCharWithTimeout()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 100;
                stream.Open();

                byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                serial.VirtualBuffer.WriteReceivedData(send, 0, send.Length);

                Assert.That(stream.ReadChar(), Is.EqualTo((int)'e'));
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'f'));
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'g'));
                Assert.That(stream.ReadChar(), Is.EqualTo(-1));
            }
        }

        [Test]
        public void ReadSingleCharEuroSymbol()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                byte[] send = new byte[] { 0xE2, 0x82, 0xAC };
                serial.VirtualBuffer.WriteReceivedData(send, 0, send.Length);

                Assert.That(stream.ReadChar(), Is.EqualTo((int)'€'));
            }
        }

        [Test]
        public void ReadSingleCharEuroSymbolBytes()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] send = new byte[] { 0xE2, 0x82, 0xAC };
                serial.VirtualBuffer.WriteReceivedData(send, 0, 1);
                Assert.That(stream.ReadChar(), Is.EqualTo(-1));
                serial.VirtualBuffer.WriteReceivedData(send, 1, 1);
                Assert.That(stream.ReadChar(), Is.EqualTo(-1));
                serial.VirtualBuffer.WriteReceivedData(send, 2, 1);
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'€'));
            }
        }

        [Test]
        public void ReadSingleCharUtf32()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] send = new byte[] { 0xF3, 0xA0, 0x82, 0x84 };
                serial.VirtualBuffer.WriteReceivedData(send, 0, send.Length);

                Assert.That(stream.ReadChar(), Is.EqualTo(0xDB40));
                Assert.That(stream.ReadChar(), Is.EqualTo(0xDC84));
                Assert.That(stream.ReadChar(), Is.EqualTo(-1));
            }
        }

        [Test]
        public void ReadSingleCharUtf32Bytes()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] send = new byte[] { 0xF3, 0xA0, 0x82, 0x84 };
                serial.VirtualBuffer.WriteReceivedData(send, 0, 1);
                Assert.That(stream.ReadChar(), Is.EqualTo(-1));
                serial.VirtualBuffer.WriteReceivedData(send, 1, 1);
                Assert.That(stream.ReadChar(), Is.EqualTo(-1));
                serial.VirtualBuffer.WriteReceivedData(send, 2, 1);
                Assert.That(stream.ReadChar(), Is.EqualTo(-1));
                serial.VirtualBuffer.WriteReceivedData(send, 3, 1);
                Assert.That(stream.ReadChar(), Is.EqualTo(0xDB40));
                Assert.That(stream.ReadChar(), Is.EqualTo(0xDC84));
                Assert.That(stream.ReadChar(), Is.EqualTo(-1));
            }
        }

        [Test]
        public void ReadLine()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                byte[] line = stream.Encoding.GetBytes("TestString\n");
                serial.VirtualBuffer.WriteReceivedData(line, 0, line.Length);

                Assert.That(stream.ReadLine(), Is.EqualTo("TestString"));
            }
        }

        [Test]
        public void ReadLineBytes()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] line = stream.Encoding.GetBytes("TestString\n");

                for (int i = 0; i < line.Length - 1; i++) {
                    serial.VirtualBuffer.WriteReceivedData(line, i, 1);
                    Assert.That(() => {
                        _ = stream.ReadLine();
                    }, Throws.TypeOf<TimeoutException>());
                }

                serial.VirtualBuffer.WriteReceivedData(line, line.Length - 1, 1);
                Assert.That(stream.ReadLine(), Is.EqualTo("TestString"));
            }
        }

        [Test]
        public void ReadLineMultipleLines()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                byte[] line1 = stream.Encoding.GetBytes("TestString1\n");
                serial.VirtualBuffer.WriteReceivedData(line1, 0, line1.Length);
                byte[] line2 = stream.Encoding.GetBytes("TestString2\n");
                serial.VirtualBuffer.WriteReceivedData(line2, 0, line2.Length);

                Assert.That(stream.ReadLine(), Is.EqualTo("TestString1"));
                Assert.That(stream.ReadLine(), Is.EqualTo("TestString2"));
            }
        }

        [Test]
        public void ReadLineMultipleLinesBytes()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] line1 = stream.Encoding.GetBytes("TestString1\n");
                serial.VirtualBuffer.WriteReceivedData(line1, 0, line1.Length);

                byte[] line2 = stream.Encoding.GetBytes("TestString2\n");
                serial.VirtualBuffer.WriteReceivedData(line2, 0, 1);

                Assert.That(stream.ReadLine(), Is.EqualTo("TestString1"));

                for (int i = 1; i < line2.Length - 1; i++) {
                    serial.VirtualBuffer.WriteReceivedData(line2, i, 1);
                    Assert.That(() => {
                        _ = stream.ReadLine();
                    }, Throws.TypeOf<TimeoutException>());
                }

                serial.VirtualBuffer.WriteReceivedData(line2, line2.Length - 1, 1);
                Assert.That(stream.ReadLine(), Is.EqualTo("TestString2"));
            }
        }

        [Test]
        public void ReadToCached()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] line = stream.Encoding.GetBytes("foobar");
                serial.VirtualBuffer.WriteReceivedData(line, 0, line.Length);

                Assert.That(() => {
                    _ = stream.ReadTo("baz");
                }, Throws.TypeOf<TimeoutException>());

                Assert.That(stream.ReadTo("foo"), Is.EqualTo(""));
                Assert.That(stream.ReadTo("bar"), Is.EqualTo(""));
            }
        }

        [Test]
        public void ReadToCachedWithData()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] line = stream.Encoding.GetBytes("foobar");
                serial.VirtualBuffer.WriteReceivedData(line, 0, line.Length);

                Assert.That(() => {
                    _ = stream.ReadTo("baz");
                }, Throws.TypeOf<TimeoutException>());

                Assert.That(stream.ReadTo("bar"), Is.EqualTo("foo"));
            }
        }

        [Test]
        public void ReadToWithData()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] line = stream.Encoding.GetBytes("superfoobar");
                serial.VirtualBuffer.WriteReceivedData(line, 0, line.Length);

                Assert.That(stream.ReadTo("foo"), Is.EqualTo("super"));
                Assert.That(stream.ReadExisting(), Is.EqualTo("bar"));
            }
        }

        [Test]
        public void ReadToOverflow()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                // Send 2048 ASCII characters
                Random r = new();
                byte[] sdata = new byte[2048];
                for (int i = 0; i < sdata.Length; i++) {
                    sdata[i] = (byte)r.Next(65, 65 + 26);
                }
                serial.VirtualBuffer.WriteReceivedData(sdata, 0, sdata.Length);
                byte[] eof = stream.Encoding.GetBytes("eof");
                serial.VirtualBuffer.WriteReceivedData(eof, 0, eof.Length);
                Assert.That(serial.VirtualBuffer.ReceivedDataLength, Is.EqualTo(sdata.Length + eof.Length));

                string result = stream.ReadTo("eof");
                Assert.That(stream.BytesToRead, Is.EqualTo(0));
                Assert.That(result, Has.Length.EqualTo(1024 - eof.Length));  // The maximum line length is 1024
                int offset = sdata.Length - result.Length;
                for (int i = 0; i < result.Length; i++) {
                    Assert.That((int)result[i], Is.EqualTo(sdata[offset + i]));
                }
            }
        }

        [Test]
        public void ReadToOverflowBytes1()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                // Send 2048 ASCII characters
                Random r = new();
                byte[] sdata = new byte[2048];
                for (int i = 0; i < sdata.Length; i++) {
                    sdata[i] = (byte)r.Next(65, 65 + 26);
                }

                for (int i = 0; i < sdata.Length; i++) {
                    serial.VirtualBuffer.WriteReceivedData(sdata, i, 1);
                    Assert.That(() => {
                        _ = stream.ReadTo("eof");
                    }, Throws.TypeOf<TimeoutException>());
                }
                byte[] eof = stream.Encoding.GetBytes("eof");
                serial.VirtualBuffer.WriteReceivedData(eof, 0, eof.Length);
                Assert.That(serial.VirtualBuffer.ReceivedDataLength, Is.EqualTo(sdata.Length + eof.Length));

                string result = stream.ReadTo("eof");
                Assert.That(stream.BytesToRead, Is.EqualTo(0));
                Assert.That(result, Has.Length.EqualTo(1024 - eof.Length));  // The maximum line length is 1024
                int offset = sdata.Length - result.Length;
                for (int i = 0; i < result.Length; i++) {
                    Assert.That((int)result[i], Is.EqualTo(sdata[offset + i]));
                }
            }
        }

        [Test]
        public void ReadToOverflowBytes2()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                // Send 2048 ASCII characters
                Random r = new();
                byte[] sdata = new byte[2048];
                for (int i = 0; i < sdata.Length; i++) {
                    sdata[i] = (byte)r.Next(65, 65 + 26);
                }

                for (int i = 0; i < sdata.Length; i++) {
                    serial.VirtualBuffer.WriteReceivedData(sdata, i, 1);
                    Assert.That(() => {
                        _ = stream.ReadTo("eof");
                    }, Throws.TypeOf<TimeoutException>());
                }
                byte[] eof = stream.Encoding.GetBytes("eof");
                for (int i = 0; i < eof.Length - 1; i++) {
                    serial.VirtualBuffer.WriteReceivedData(eof, i, 1);
                    Assert.That(() => {
                        _ = stream.ReadTo("eof");
                    }, Throws.TypeOf<TimeoutException>());
                }
                serial.VirtualBuffer.WriteReceivedData(eof, eof.Length - 1, 1);
                Assert.That(serial.VirtualBuffer.ReceivedDataLength, Is.EqualTo(sdata.Length + eof.Length));

                string result = stream.ReadTo("eof");
                Assert.That(stream.BytesToRead, Is.EqualTo(0));
                Assert.That(result, Has.Length.EqualTo(1024 - eof.Length));  // The maximum line length is 1024
                int offset = sdata.Length - result.Length;
                for (int i = 0; i < result.Length; i++) {
                    Assert.That((int)result[i], Is.EqualTo(sdata[offset + i]));
                }
            }
        }

        [Test]
        public void ReadToOverflowTwice()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                // Send 2048 ASCII characters
                Random r = new();
                byte[] sdata = new byte[2048];
                for (int i = 0; i < sdata.Length; i++) {
                    sdata[i] = (byte)r.Next(65, 65 + 26);
                }
                serial.VirtualBuffer.WriteReceivedData(sdata, 0, sdata.Length);

                Assert.That(() => {
                    _ = stream.ReadTo("eof");
                }, Throws.TypeOf<TimeoutException>());

                Assert.That(() => {
                    _ = stream.ReadTo("eof");
                }, Throws.TypeOf<TimeoutException>());

                byte[] eof = stream.Encoding.GetBytes("eof");
                serial.VirtualBuffer.WriteReceivedData(eof, 0, eof.Length);
                Assert.That(serial.VirtualBuffer.ReceivedDataLength, Is.EqualTo(sdata.Length + eof.Length));

                string result = stream.ReadTo("eof");
                Assert.That(stream.BytesToRead, Is.EqualTo(0));
                Assert.That(result, Has.Length.EqualTo(1024 - eof.Length));  // The maximum line length is 1024
                int offset = sdata.Length - result.Length;
                for (int i = 0; i < result.Length; i++) {
                    Assert.That((int)result[i], Is.EqualTo(sdata[offset + i]));
                }
            }
        }

        [Test]
        public void ReadToWithMbcs()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                byte[] data = new byte[] { 0x61, 0xE2, 0x82, 0xAC, 0x40, 0x41 };
                serial.VirtualBuffer.WriteReceivedData(data, 0, data.Length);

                Assert.That(stream.ReadChar(), Is.EqualTo((int)'a'));
                Assert.That(stream.ReadByte(), Is.EqualTo((0xE2)));
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'�'));
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'�'));
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'@'));
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'A'));
            }
        }

        [Test]
        public void ReadToWithMbcs1()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] data = new byte[] { 0x61, 0xE2, 0x82, 0xAC, 0x40, 0x41 };
                serial.VirtualBuffer.WriteReceivedData(data, 0, data.Length);

                Assert.That(() => { stream.ReadLine(); }, Throws.Exception.TypeOf<TimeoutException>());

                // So now we should have data in the character cache, but we'll ready a byte.
                Assert.That(stream.ReadByte(), Is.EqualTo(0x61));
                Assert.That(stream.ReadExisting(), Is.EqualTo("€@A"));
            }
        }

        [Test]
        public void ReadToWithMbcs2()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] data = new byte[] { 0xE2, 0x82, 0xAC, 0x40, 0x41, 0x62 };
                serial.VirtualBuffer.WriteReceivedData(data, 0, data.Length);

                Assert.That(() => { stream.ReadLine(); }, Throws.Exception.TypeOf<TimeoutException>());

                // So now we should have data in the character cache, but we'll ready a byte.
                Assert.That(stream.ReadByte(), Is.EqualTo(0xE2));
                Assert.That(stream.ReadExisting(), Is.EqualTo("��@Ab"));
            }
        }

        [Test]
        public void ReadToWithMbcs3()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] data = new byte[] { 0xE2, 0x82, 0xAC, 0x40, 0x41, 0x62 };
                serial.VirtualBuffer.WriteReceivedData(data, 0, data.Length);

                Assert.That(() => { stream.ReadLine(); }, Throws.Exception.TypeOf<TimeoutException>());

                // So now we should have data in the character cache, but we'll ready a byte.
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'€'));
                Assert.That(stream.ReadByte(), Is.EqualTo(0x40));
                Assert.That(stream.ReadExisting(), Is.EqualTo("Ab"));
            }
        }

        [Test]
        public void ReadToResetWithOverflow1()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] writeData = new byte[2048];
                for (int i = 0; i < writeData.Length; i++) {
                    writeData[i] = (byte)((i % 26) + 0x41);
                }

                // We write 2048 bytes that starts with A..Z repeated.
                //  Position 0 = A
                //  Position 1023 = J
                // To read a line, it parses the 2048 characters, not finding a new line. Then we read a character and
                // we expect to get 'A'.
                serial.VirtualBuffer.WriteReceivedData(writeData, 0, writeData.Length);

                Assert.That(() => { stream.ReadLine(); }, Throws.Exception.TypeOf<TimeoutException>());
                Assert.That(stream.ReadChar(), Is.EqualTo((int)'A'));
            }
        }

        [Test]
        public void ReadToResetWithOverflow2()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.ReadTimeout = 0;
                stream.Open();

                byte[] writeData = new byte[2048];
                for (int i = 0; i < writeData.Length - 1; i++) {
                    writeData[i] = (byte)((i % 26) + 0x41);
                }
                writeData[writeData.Length - 1] = (byte)'\n';

                // We write 2048 bytes that starts with A..Z repeated.
                //  Position 0 = A
                //  Position 1023 = J
                //  Position 1024 = K
                //  Position 2047 = \n
                serial.VirtualBuffer.WriteReceivedData(writeData, 0, writeData.Length);

                string line = stream.ReadLine();
                Assert.That(line[0], Is.EqualTo('K'));
                Assert.That(line, Has.Length.EqualTo(1023));   // Is 1024 - Length('\n').
            }
        }

        [Test]
        public void WriteLine()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                stream.WriteLine("TestString");

                // Read the bytes that should be sent, convert to a string.

                byte[] received = new byte[1024];
                int r = serial.VirtualBuffer.ReadSentData(received, 0, received.Length);
                Assert.That(r, Is.EqualTo(10 + stream.NewLine.Length));

                char[] decoded = new char[1024];
                Decoder decoder = stream.Encoding.GetDecoder();
                decoder.Convert(received, 0, r, decoded, 0, decoded.Length, true, out int bu, out int cu, out bool completed);
                Assert.That(bu, Is.EqualTo(r));
                Assert.That(cu, Is.EqualTo(10 + stream.NewLine.Length));
                Assert.That(completed, Is.True);
                string decodedString = new(decoded, 0, cu);
                Assert.That(decodedString, Is.EqualTo("TestString" + stream.NewLine));
            }
        }

        [Test]
        public void WriteText()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                stream.Write("TestString");

                // Read the bytes that should be sent, convert to a string.

                byte[] received = new byte[1024];
                int r = serial.VirtualBuffer.ReadSentData(received, 0, received.Length);
                Assert.That(r, Is.EqualTo(10));

                char[] decoded = new char[1024];
                Decoder decoder = stream.Encoding.GetDecoder();
                decoder.Convert(received, 0, r, decoded, 0, decoded.Length, true, out int bu, out int cu, out bool completed);
                Assert.That(bu, Is.EqualTo(r));
                Assert.That(cu, Is.EqualTo(10));
                Assert.That(completed, Is.True);
                string decodedString = new(decoded, 0, cu);
                Assert.That(decodedString, Is.EqualTo("TestString"));
            }
        }

        [Test]
        public void WriteChars()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                char[] chars = "TestString".ToCharArray();
                stream.Write(chars, 0, chars.Length);

                // Read the bytes that should be sent, convert to a string.

                byte[] received = new byte[1024];
                int r = serial.VirtualBuffer.ReadSentData(received, 0, received.Length);
                Assert.That(r, Is.EqualTo(10));

                char[] decoded = new char[1024];
                Decoder decoder = stream.Encoding.GetDecoder();
                decoder.Convert(received, 0, r, decoded, 0, decoded.Length, true, out int bu, out int cu, out bool completed);
                Assert.That(bu, Is.EqualTo(r));
                Assert.That(cu, Is.EqualTo(10));
                Assert.That(completed, Is.True);
                string decodedString = new(decoded, 0, cu);
                Assert.That(decodedString, Is.EqualTo("TestString"));
            }
        }
    }
}
