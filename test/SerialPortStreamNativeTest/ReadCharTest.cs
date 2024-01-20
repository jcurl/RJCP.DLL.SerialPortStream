// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    /// <summary>
    /// Test sending and receiving data via two serial ports.
    /// </summary>
    /// <remarks>
    /// You will need to have two serial ports connected to each other on the same computer using a NULL modem cable.
    /// Alternatively, you can use a software emulated serial port, such as com0com for tests.
    /// <para>You need to update the variables SourcePort and DestPort to be the names of the two serial ports.</para>
    /// </remarks>
    [TestFixture]
    [Timeout(10000)]
    public class ReadCharTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;
        private readonly string DestPort = SerialConfiguration.DestPort;
        private const int TimeOut = 300;

#if NETCOREAPP3_1
        [SetUp]
        public void InitLogging()
        {
            Trace.GlobalLogger.Initialize();
        }
#endif

        [Test]
        public void ReadChars()
        {
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 2 * TimeOut + 500; src.ReadTimeout = 2 * TimeOut + 500;
                dst.WriteTimeout = 2 * TimeOut + 500; dst.ReadTimeout = 2 * TimeOut + 500;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                src.Write(send, 0, send.Length);
                Thread.Sleep(TimeOut + 500);

                char[] recv = new char[5];
                int cread = dst.Read(recv, 0, recv.Length);
                Assert.That(cread, Is.EqualTo(3));
                Assert.That(recv[0], Is.EqualTo('e'));
                Assert.That(recv[1], Is.EqualTo('f'));
                Assert.That(recv[2], Is.EqualTo('g'));
            }
        }

        [Test]
        public void ReadCharsWithTimeout()
        {
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 2 * TimeOut + 500; src.ReadTimeout = 2 * TimeOut + 500;
                dst.WriteTimeout = 2 * TimeOut + 500; dst.ReadTimeout = 2 * TimeOut + 500;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                Task write = new TaskFactory().StartNew(() => {
                    Thread.Sleep(TimeOut + 500);
                    byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                    src.Write(send, 0, send.Length);
                });

                char[] recv = new char[5];
                int cread = 0; int counter = 0;
                while (cread < 3 && counter < 5) {
                    cread += dst.Read(recv, cread, recv.Length - cread);
                    counter++;
                    Console.WriteLine($"dst.Read. Got cread={cread} bytes in {counter} loops");
                }

                for (int i = 0; i < cread; i++) {
                    Console.WriteLine($"cread[{i}] = {recv[i]}");
                }

                Assert.That(cread, Is.EqualTo(3));
                Assert.That(recv[0], Is.EqualTo('e'));
                Assert.That(recv[1], Is.EqualTo('f'));
                Assert.That(recv[2], Is.EqualTo('g'));

                write.Wait();
            }
        }

        [Test]
        public void ReadSingleChar()
        {
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 2 * TimeOut + 500; src.ReadTimeout = 2 * TimeOut + 500;
                dst.WriteTimeout = 2 * TimeOut + 500; dst.ReadTimeout = 2 * TimeOut + 500;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                src.Write(send, 0, send.Length);
                Thread.Sleep(TimeOut + 500);

                Assert.That(dst.ReadChar(), Is.EqualTo((int)'e'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'f'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'g'));
            }
        }

        [Test]
        public void ReadSingleCharWithTimeout()
        {
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 2 * TimeOut + 500; src.ReadTimeout = 2 * TimeOut + 500;
                dst.WriteTimeout = 2 * TimeOut + 500; dst.ReadTimeout = 2 * TimeOut + 500;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                Task write = new TaskFactory().StartNew(() => {
                    Thread.Sleep(TimeOut + 500);
                    byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                    src.Write(send, 0, send.Length);
                });

                Assert.That(dst.ReadChar(), Is.EqualTo((int)'e'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'f'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'g'));

                write.Wait();
            }
        }

        [Test]
        public void ReadSingleCharEuro()
        {
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                byte[] send = new byte[] { 0xE2, 0x82, 0xAC };
                src.Write(send, 0, send.Length);
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'€'));
            }
        }

        [Test]
        public void ReadSingleCharUtf32()
        {
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                byte[] send = new byte[] { 0xF3, 0xA0, 0x82, 0x84 };
                src.Write(send, 0, send.Length);

                Assert.That(dst.ReadChar(), Is.EqualTo(0xDB40));
                Assert.That(dst.ReadChar(), Is.EqualTo(0xDC84));
                Assert.That(dst.ReadChar(), Is.EqualTo(-1));
            }
        }

        [Test]
        public void WriteLineReadLine()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                src.WriteLine("TestString");
                Assert.That(dst.ReadLine(), Is.EqualTo("TestString"));
            }
        }

        [Test]
        public void WriteLineReadLineTimeout1()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                src.Write("TestString");
                Assert.That(() => {
                    _ = dst.ReadLine();
                }, Throws.TypeOf<TimeoutException>());

                src.WriteLine("");
                Assert.That(dst.ReadLine(), Is.EqualTo("TestString"));
            }
        }

        [Test]
        public void WriteLineReadLineTimeout2()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                src.Write("Test");
                Assert.That(() => {
                    _ = dst.ReadLine();
                }, Throws.TypeOf<TimeoutException>());

                src.WriteLine("String");
                Assert.That(dst.ReadLine(), Is.EqualTo("TestString"));
            }
        }

        [Test]
        public void WriteLineReadLineMultilines()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                src.Write("Line1\nLine2\n");
                Assert.That(dst.ReadLine(), Is.EqualTo("Line1"));
                Assert.That(dst.ReadLine(), Is.EqualTo("Line2"));

                Assert.That(() => {
                    _ = dst.ReadLine();
                }, Throws.TypeOf<TimeoutException>());
            }
        }

        [Test]
        public void WriteLineReadLineCharForChar()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                bool err = false;
                string s = null;

                const string send = "A Brief History Of Time\n";

                for (int i = 0; i < send.Length; i++) {
                    src.Write(send[i].ToString());
                    try {
                        s = dst.ReadLine();
                    } catch (Exception e) {
                        if (e is TimeoutException) err = true;
                    }
                    if (i < send.Length - 1) {
                        Assert.That(err, Is.True, $"No timeout exception occurred when waiting for {send[i]} (position {i})");
                    }
                }
                Assert.That(s, Is.EqualTo("A Brief History Of Time"));
            }
        }

        [Test]
        public void WriteLineReadLineMbcsByteForByte()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                bool err = false;
                string s = null;

                byte[] buf = new byte[] {
                    0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A,
                    0xE2, 0x82, 0xAC, 0x0A
                };

                for (int i = 0; i < buf.Length; i++) {
                    src.Write(buf, i, 1);
                    try {
                        s = dst.ReadLine();
                    } catch (Exception e) {
                        if (e is TimeoutException) err = true;
                    }
                    if (i < buf.Length - 1) {
                        Assert.That(err, Is.True, $"No timeout exception occurred when waiting for {buf[i]:X2} (position {i})");
                    }
                }
                Assert.That(s, Is.EqualTo("ABCDEFGHIJ€"));
            }
        }

        [Test]
        public void ReadToCached()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                src.Write("foobar");
                Assert.That(() => {
                    _ = dst.ReadTo("baz");
                }, Throws.TypeOf<TimeoutException>());

                Assert.That(dst.ReadTo("foo"), Is.EqualTo(""));
                Assert.That(dst.ReadTo("bar"), Is.EqualTo(""));

                Assert.That(() => {
                    _ = dst.ReadTo("baz");
                }, Throws.TypeOf<TimeoutException>());
            }
        }

        [Test]
        public void ReadToNormal()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                src.Write("superfoobar");
                Assert.That(dst.ReadTo("foo"), Is.EqualTo("super"));

                // Sleep for 100ms to allow all data to be sent and received. Else we might not receive the
                // entire string, and sometimes only get it partially.
                Thread.Sleep(100);
                Assert.That(dst.ReadExisting(), Is.EqualTo("bar"));
            }
        }

        [Test]
        public void ReadToOverflow()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                // Send 2048 ASCII characters
                Random r = new Random();
                byte[] sdata = new byte[2048];
                for (int i = 0; i < sdata.Length; i++) {
                    sdata[i] = (byte)r.Next(65, 65 + 26);
                }

                // Wait for the data to be received
                src.Write(sdata, 0, sdata.Length);
                src.Write("eof");
                while (dst.BytesToRead < sdata.Length) {
                    Thread.Sleep(100);
                }

                string result = dst.ReadTo("eof");
                Assert.That(dst.BytesToRead, Is.EqualTo(0));
                Assert.That(result, Has.Length.EqualTo(1024 - 3));
                int offset = sdata.Length - result.Length;
                for (int i = 0; i < result.Length; i++) {
                    Assert.That((int)result[i], Is.EqualTo(sdata[offset + i]));
                }
            }
        }

        [Test]
        public void ReadToWithMbcs()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                src.Write(new byte[] { 0x61, 0xE2, 0x82, 0xAC, 0x40, 0x41 }, 0, 6);
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'a'));
                Assert.That(dst.ReadByte(), Is.EqualTo((0xE2)));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'�'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'�'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'@'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'A'));
            }
        }

        [Test]
        public void ReadToResetWithMbcs1()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                src.Write(new byte[] { 0x61, 0xE2, 0x82, 0xAC, 0x40, 0x41 }, 0, 6);
                Assert.That(() => { dst.ReadLine(); }, Throws.Exception.TypeOf<TimeoutException>());

                // So now we should have data in the character cache, but we'll ready a byte.
                Assert.That(dst.ReadByte(), Is.EqualTo(0x61));
                Assert.That(dst.ReadExisting(), Is.EqualTo("€@A"));
            }
        }

        [Test]
        public void ReadToResetWithMbcs2()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                src.Write(new byte[] { 0xE2, 0x82, 0xAC, 0x40, 0x41, 0x62 }, 0, 6);
                Assert.That(() => { dst.ReadLine(); }, Throws.Exception.TypeOf<TimeoutException>());

                // So now we should have data in the character cache, but we'll ready a byte.
                Assert.That(dst.ReadByte(), Is.EqualTo(0xE2));
                Assert.That(dst.ReadExisting(), Is.EqualTo("��@Ab"));
            }
        }

        [Test]
        public void ReadToResetWithMbcs3()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                src.Write(new byte[] { 0xE2, 0x82, 0xAC, 0x40, 0x41, 0x62 }, 0, 6);
                Assert.That(() => { dst.ReadLine(); }, Throws.Exception.TypeOf<TimeoutException>());

                Assert.That(dst.ReadChar(), Is.EqualTo((int)'€'));
                Assert.That(dst.ReadByte(), Is.EqualTo(0x40));
                Assert.That(dst.ReadExisting(), Is.EqualTo("Ab"));
            }
        }

        [Test]
        public void ReadToResetWithOverflow()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                byte[] writeData = new byte[2048];
                for (int i = 0; i < writeData.Length; i++) { writeData[i] = (byte)((i % 26) + 0x41); }

                // We write 2048 bytes that starts with A..Z repeated.
                //  Position 0 = A
                //  Position 1023 = J
                // To read a line, it parses the 2048 characters, not finding a new line. Then we read a character and
                // we expect to get 'A'.
                src.Write(writeData, 0, writeData.Length);
                Assert.That(() => { dst.ReadLine(); }, Throws.Exception.TypeOf<TimeoutException>());
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'A'));
            }
        }

        [Test]
        public void ReadToWithOverflow2()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = -1; src.ReadTimeout = -1;
                dst.WriteTimeout = -1; dst.ReadTimeout = -1;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                byte[] writeData = new byte[2048];
                for (int i = 0; i < writeData.Length - 1; i++) { writeData[i] = (byte)((i % 26) + 0x41); }
                writeData[writeData.Length - 1] = (byte)'\n';

                // We write 2048 bytes that starts with A..Z repeated.
                //  Position 0 = A
                //  Position 1023 = J
                //  Position 1024 = K
                //  Position 2047 = \n
                src.Write(writeData, 0, writeData.Length);
                string line = dst.ReadLine();
                Assert.That(line[0], Is.EqualTo('K'));
                Assert.That(line, Has.Length.EqualTo(1023));   // Is 1024 - Length('\n').
            }
        }
    }
}
