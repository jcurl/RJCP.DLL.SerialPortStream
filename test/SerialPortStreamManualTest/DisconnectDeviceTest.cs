// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using System;
    using System.Threading;
    using NUnit.Framework;

    /// <summary>
    /// Manual tests for when devices are disconnected.
    /// </summary>
    /// <remarks>
    /// Each test should be run individually. Follow the instructions in the remarks for each test case.
    /// </remarks>
    [TestFixture]
    [Category("ManualTest")]
    [Explicit("Manual Test")]
    [Timeout(10000)]
    public class DisconnectDeviceTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;
        private readonly string DestPort = SerialConfiguration.DestPort;

#if NET6_0_OR_GREATER
        [SetUp]
        public void InitLogging()
        {
            Trace.GlobalLogger.Initialize();
        }
#endif

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnReadBlocked()
        {
            byte[] buffer = new byte[1024];

            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int bytes = serialSource.Read(buffer, 0, buffer.Length);
                Console.WriteLine($"{bytes} bytes read");

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnReadBlockedReadAgain()
        {
            byte[] buffer = new byte[1024];

            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int bytes = serialSource.Read(buffer, 0, buffer.Length);
                Console.WriteLine($"{bytes} bytes read");

                Assert.That(
                    () => {
                        bytes = serialSource.Read(buffer, 0, buffer.Length);
                        Console.WriteLine($"{bytes} bytes read again");
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnReadCharsBlocked()
        {
            char[] buffer = new char[1024];

            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int bytes = serialSource.Read(buffer, 0, buffer.Length);
                Console.WriteLine($"{bytes} bytes read");

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnReadCharsBlockedReadAgain()
        {
            char[] buffer = new char[1024];

            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int bytes = serialSource.Read(buffer, 0, buffer.Length);
                Console.WriteLine($"{bytes} bytes read");

                Assert.That(
                    () => {
                        bytes = serialSource.Read(buffer, 0, buffer.Length);
                        Console.WriteLine($"{bytes} bytes read again");
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnReadByteBlocked()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int c = serialSource.ReadByte();
                Console.WriteLine($"{c} byte read");

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnReadByteBlockedReadAgain()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int c = serialSource.ReadByte();
                Console.WriteLine($"{c} byte read");

                Assert.That(
                    () => {
                        c = serialSource.ReadByte();
                        Console.WriteLine($"{c} byte read again");
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnReadCharBlocked()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int c = serialSource.ReadChar();
                Console.WriteLine($"{c} char read");

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnReadCharBlockedReadAgain()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int c = serialSource.ReadChar();
                Console.WriteLine($"{c} char read");

                Assert.That(
                    () => {
                        c = serialSource.ReadChar();
                        Console.WriteLine($"{c} char read again");
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnReadLineBlocked()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();

                Assert.That(
                    () => {
                        string l = serialSource.ReadLine();
                        Console.WriteLine("line read length={0} ({1})",
                            l == null ? -1 : l.Length,
                            l ?? string.Empty);
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state, then close it when finished.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void ReadUntilDisconnectThenClose()
        {
            byte[] buffer = new byte[8192];
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.ReadBufferSize = 8192;
                serialSource.WriteBufferSize = 8192;
                serialSource.Open();
                Thread.Sleep(100);

                while (serialSource.Read(buffer, 0, buffer.Length) > 0) {
                    Console.WriteLine("In Read Loop");
                    /* throw away the data */
                }
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state, then dispose it when finished.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void ReadUntilDisconnectThenDispose()
        {
            byte[] buffer = new byte[8192];
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.ReadBufferSize = 8192;
                serialSource.WriteBufferSize = 8192;
                serialSource.Open();
                Thread.Sleep(100);

                while (serialSource.Read(buffer, 0, buffer.Length) > 0) {
                    Console.WriteLine("In Read Loop");
                    /* throw away the data */
                }
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state, then read again and close it when finished.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void ReadUntilDisconnectAndReadAgainThenClose()
        {
            byte[] buffer = new byte[8192];
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.ReadBufferSize = 8192;
                serialSource.WriteBufferSize = 8192;
                serialSource.Open();
                Thread.Sleep(100);

                int read = 0;
                while (serialSource.Read(buffer, 0, buffer.Length) > 0) {
                    Console.WriteLine("In Read Loop");
                    /* throw away the data */
                }

                try {
                    read = serialSource.Read(buffer, 0, buffer.Length);
                } catch (System.IO.IOException) {
                    Console.WriteLine("IOException occurred");
                }
                Console.WriteLine($"Second Read: {read}");
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state, then read again, then dispose it when finished.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>Insert a USB-SER as the SourcePort.</item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void ReadUntilDisconnectAndReadAgainThenDispose()
        {
            byte[] buffer = new byte[8192];
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.ReadBufferSize = 8192;
                serialSource.WriteBufferSize = 8192;
                serialSource.Open();
                Thread.Sleep(100);

                int read = 0;
                while (serialSource.Read(buffer, 0, buffer.Length) > 0) {
                    Console.WriteLine("In Read Loop");
                    /* throw away the data */
                }

                try {
                    read = serialSource.Read(buffer, 0, buffer.Length);
                } catch (System.IO.IOException) {
                    Console.WriteLine("IOException occurred");
                }
                Console.WriteLine($"Second Read: {read}");
            }
        }

        /// <summary>
        /// Continuously write a byte, while device is disconnected.
        /// </summary>
        /// <remarks>
        /// This test can be used to check behaviour in case that the serial port is removed. In case of the device no
        /// longer available, it should abort the read with an exception.
        /// <list type="bullet">
        /// <item>
        /// Insert a USB-SER as the SourcePort. The DestPort remains in the PC always. Both devices must support HW flow
        /// control.
        /// </item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>
        /// The SerialPortStream should see that the device is removed and the test case passes because the serial port
        /// is closed.
        /// </item>
        /// </list>
        /// </remarks>
        [Test]
        public void WriteByteUntilDisconnected()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                serialDest.Open();

                while (serialSource.IsOpen) {
                    try {
                        serialSource.WriteByte(0xAA);
                    } catch (Exception ex) {
                        Console.WriteLine($"\n** Exception: {ex.Message}\n{ex}\n\n");
                    }
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Block on Flush because of RTS is blocked (HW flow control is active).
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// Insert a USB-SER as the SourcePort. The DestPort remains in the PC always. Both devices must support HW flow
        /// control.
        /// </item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnFlushBlocked()
        {
            byte[] buffer = new byte[8192];
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.ReadBufferSize = 8192;
                serialSource.WriteBufferSize = 8192;
                serialDest.ReadBufferSize = 8192;
                serialDest.WriteBufferSize = 8192;
                serialSource.Handshake = Handshake.Rts;
                serialSource.Open();
                serialDest.Open();

                serialDest.RtsEnable = false;
                Thread.Sleep(100);

                Assert.That(
                    () => {
                        Console.WriteLine("DisconnectOnFlushBlocked Writing");
                        serialSource.Write(buffer, 0, buffer.Length);
                        Console.WriteLine("DisconnectOnFlushBlocked Flushing");
                        serialSource.Flush();
                        Console.WriteLine("DisconnectOnFlushBlocked Flushed");
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Block on Write because of RTS is blocked (HW flow control is active).
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// Insert a USB-SER as the SourcePort. The DestPort remains in the PC always. Both devices must support HW flow
        /// control.
        /// </item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnWriteBlocked()
        {
            byte[] buffer = new byte[8192];
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.ReadBufferSize = 8192;
                serialSource.WriteBufferSize = 8192;
                serialDest.ReadBufferSize = 8192;
                serialDest.WriteBufferSize = 8192;
                serialSource.Handshake = Handshake.Rts;
                serialSource.Open();
                serialDest.Open();

                serialDest.RtsEnable = false;
                Thread.Sleep(100);

                Assert.That(
                    () => {
                        while (true) {
                            Console.WriteLine("DisconnectOnWriteBlocked Writing");
                            serialSource.Write(buffer, 0, buffer.Length);
                        }
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Block on Write because of RTS is blocked (HW flow control is active).
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// Insert a USB-SER as the SourcePort. The DestPort remains in the PC always. Both devices must support HW flow
        /// control.
        /// </item>
        /// <item>Run the test.</item>
        /// <item>Within 10s, remove the device.</item>
        /// <item>The SerialPortStream should see that the device is removed and the test case passes.</item>
        /// </list>
        /// </remarks>
        [Test]
        public void DisconnectOnBeginWriteBlocked()
        {
            byte[] buffer = new byte[8192];
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.ReadBufferSize = 8192;
                serialSource.WriteBufferSize = 8192;
                serialDest.ReadBufferSize = 8192;
                serialDest.WriteBufferSize = 8192;
                serialSource.Handshake = Handshake.Rts;
                serialSource.Open();
                serialDest.Open();

                serialDest.RtsEnable = false;
                Thread.Sleep(100);

                Assert.That(
                    () => {
                        while (true) {
                            Console.WriteLine("DisconnectOnWriteAsyncBlocked BeginWrite");
                            IAsyncResult ar = serialSource.BeginWrite(buffer, 0, buffer.Length, null, null);
                            Console.WriteLine("DisconnectOnWriteAsyncBlocked EndWrite");
                            serialSource.EndWrite(ar);
                        }
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }
    }
}
