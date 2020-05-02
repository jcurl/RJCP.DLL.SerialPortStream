// Copyright © Jason Curl 2012-2020
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.SerialPortStreamTest
{
    using System;
    using System.Threading;
    using NUnit.Framework;

    /// <summary>
    /// Test sending and receiving data via two serial ports.
    /// </summary>
    /// <remarks>
    /// You will need to have two serial ports connected to each other on the same computer using a NULL modem cable.
    /// Alternatively, you can use a software emulated serial port, such as com0com for tests.
    /// <para>
    /// You need to update the variables c_SourcePort and c_DestPort to be the names of the two serial ports.
    /// </para>
    /// </remarks>
    [TestFixture(Category = "SerialPortStream")]
    [Timeout(10000)]
    public class SerialPortStreamManualTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;
        private readonly string DestPort = SerialConfiguration.DestPort;

        [Test]
        [Category("SerialPortStream.Linux.ManualTest")]
        [Explicit("Manual Test")]
        [Timeout(20000)]
        public void DisposedWhenBlocked()
        {
            byte[] buffer = new byte[1024];

            SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One);
            Thread testThread;

            using (ManualResetEvent disposedEvent = new ManualResetEvent(false))
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

                testThread = new Thread(
                    () => {
                        Thread.Sleep(2000);
                        Console.WriteLine("Disposing serialSource");

                        // It appears that the MSDN .NET implementation blocks here, never
                        // to return as we're blocked on another thread.
                        disposedEvent.Set();
                        serialSource.Dispose();
                        Console.WriteLine("Disposed serialSource");
                    }
                );
                testThread.Start();

                Assert.That(
                    () => {
                        int bufferCount = 1024 * 1024;
                        while (bufferCount > 0) {
                            serialSource.Write(buffer, 0, buffer.Length);
                            if (disposedEvent.WaitOne(0)) {
                                Assert.Fail("Write returned after being disposed.");
                            }
                            bufferCount -= buffer.Length;
                            Console.WriteLine("{0}", bufferCount);
                        }
                    }, Throws.InstanceOf<ObjectDisposedException>());
            }

            testThread.Join(20000);
            Console.WriteLine("Finished");
        }

        [Test]
        [Category("SerialPortStream.Linux.ManualTest")]
        [Explicit("Manual Test")]
        [Timeout(20000)]
        public void ClosedWhenBlocked()
        {
            byte[] buffer = new byte[1024];

            using (ManualResetEvent closedEvent = new ManualResetEvent(false))
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

                new Thread(
                    () => {
                        Thread.Sleep(2000);
                        Console.WriteLine("Closing serialSource");

                        // It appears that the MSDN .NET implementation blocks here, never
                        // to return as we're blocked on another thread.
                        closedEvent.Set();
                        serialSource.Close();
                        Console.WriteLine("Closed serialSource");
                    }
                ).Start();

                Assert.That(
                    () => {
                        int bufferCount = 1024 * 1024;
                        while (bufferCount > 0) {
                            serialSource.Write(buffer, 0, buffer.Length);
                            if (closedEvent.WaitOne(0)) {
                                Assert.Fail("Write returned after being closed.");
                            }
                            bufferCount -= buffer.Length;
                            Console.WriteLine("{0}", bufferCount);
                        }
                    }, Throws.InstanceOf<System.IO.IOException>());
            }
        }

        [Test]
        [Category("SerialPortStream.Linux.ManualTest")]
        [Explicit("Manual Test")]
        [Timeout(20000)]
        public void ClosedWhenBlockedResetHandshake()
        {
            byte[] buffer = new byte[1024];

            using (ManualResetEvent closedEvent = new ManualResetEvent(false))
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

                new Thread(
                    () => {
                        Thread.Sleep(2000);
                        Console.WriteLine("Closing serialSource but first setting handshake to NONE");

                        // It appears that the MSDN .NET implementation blocks here, never
                        // to return as we're blocked on another thread.
                        closedEvent.Set();

                        // In an attempt to "unblock" on MONO, we thought we could set the
                        // handlshake to none. What happens instead, it blocks here which
                        // is also an error, and so a new test case.
                        serialSource.Handshake = Handshake.None;
                        Console.WriteLine("Closing serialSource");
                        serialSource.Close();
                        Console.WriteLine("Closed serialSource");
                    }
                ).Start();

                Assert.That(
                    () => {
                        int bufferCount = 1024 * 1024;
                        while (bufferCount > 0) {
                            serialSource.Write(buffer, 0, buffer.Length);
                            if (closedEvent.WaitOne(0)) {
                                Assert.Fail("Write returned after being closed.");
                            }
                            bufferCount -= buffer.Length;
                            Console.WriteLine("{0}", bufferCount);
                        }
                    }, Throws.InstanceOf<System.IO.IOException>());
            }
        }

        [Test]
        [Category("SerialPortStream.Linux.ManualTest")]
        [Explicit("Manual Test")]
        [Timeout(20000)]
        public void DisposedWhenFlushBlocked()
        {
            byte[] buffer = new byte[8192];

            SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One);
            Thread testThread;

            using (ManualResetEvent disposedEvent = new ManualResetEvent(false))
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

                testThread = new Thread(
                    () => {
                        Thread.Sleep(2000);
                        Console.WriteLine("Disposing serialSource");

                        // It appears that the MSDN .NET implementation blocks here, never
                        // to return as we're blocked on another thread.
                        disposedEvent.Set();
                        serialSource.Dispose();
                        Console.WriteLine("Disposed serialSource");
                    }
                );
                testThread.Start();

                Assert.That(
                    () => {
                        Console.WriteLine("DisposedWhenFlushBlocked Writing");
                        serialSource.Write(buffer, 0, buffer.Length);
                        Console.WriteLine("DisposedWhenFlushBlocked Flushing");
                        serialSource.Flush();
                        Console.WriteLine("DisposedWhenFlushBlocked Flushed");
                        if (disposedEvent.WaitOne(0)) {
                            Assert.Fail("Write returned after being disposed.");
                        }
                    }, Throws.InstanceOf<ObjectDisposedException>());
            }

            testThread.Join(20000);
            Console.WriteLine("Finished");
        }

        [Test]
        [Category("SerialPortStream.Linux.ManualTest")]
        [Explicit("Manual Test")]
        [Timeout(20000)]
        public void ClosedWhenFlushBlocked()
        {
            byte[] buffer = new byte[8192];

            using (ManualResetEvent closedEvent = new ManualResetEvent(false))
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

                new Thread(
                    () => {
                        Thread.Sleep(2000);
                        Console.WriteLine("Closing serialSource");

                        // It appears that the MSDN .NET implementation blocks here, never
                        // to return as we're blocked on another thread.
                        closedEvent.Set();
                        serialSource.Close();
                        Console.WriteLine("Closed serialSource");
                    }
                ).Start();

                Assert.That(
                    () => {
                        Console.WriteLine("ClosedWhenFlushBlocked Writing");
                        serialSource.Write(buffer, 0, buffer.Length);
                        Console.WriteLine("ClosedWhenFlushBlocked Flushing");
                        serialSource.Flush();
                        Console.WriteLine("ClosedWhenFlushBlocked Flushed");
                        if (closedEvent.WaitOne(0)) {
                            Assert.Fail("Write returned after being closed.");
                        }
                    }, Throws.InstanceOf<System.IO.IOException>());
            }
        }

        [Test]
        [Category("SerialPortStream.Linux.ManualTest")]
        [Explicit("Manual Test")]
        [Timeout(20000)]
        public void DisposedWhenReadBlocked()
        {
            byte[] buffer = new byte[1024];

            SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One);
            Thread testThread;

            using (ManualResetEvent disposedEvent = new ManualResetEvent(false))
            using (SerialPortStream serialDest = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                serialDest.Open();

                testThread = new Thread(
                    () => {
                        Thread.Sleep(2000);
                        Console.WriteLine("Disposing serialSource");

                        // It appears that the MSDN .NET implementation blocks here, never
                        // to return as we're blocked on another thread.
                        disposedEvent.Set();
                        serialSource.Dispose();
                        Console.WriteLine("Disposed serialSource");
                    }
                );
                testThread.Start();

                Assert.That(
                    () => {
                        int bytes = serialSource.Read(buffer, 0, buffer.Length);
                        Console.WriteLine("Read finished, returned {0} bytes", bytes);
                        if (disposedEvent.WaitOne(0)) {
                            Assert.Fail("Read returned after being disposed.");
                        }
                    }, Throws.InstanceOf<ObjectDisposedException>());
            }

            testThread.Join(20000);
            Console.WriteLine("Finished");
        }

        [Test]
        [Category("SerialPortStream.Linux.ManualTest")]
        [Explicit("Manual Test")]
        [Timeout(20000)]
        public void ClosedWhenReadBlocked()
        {
            byte[] buffer = new byte[1024];

            using (ManualResetEvent closedEvent = new ManualResetEvent(false))
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                serialDest.Open();

                new Thread(
                    () => {
                        Thread.Sleep(2000);
                        Console.WriteLine("Closing serialSource");

                        // It appears that the MSDN .NET implementation blocks here, never
                        // to return as we're blocked on another thread.
                        closedEvent.Set();
                        serialSource.Close();
                        Console.WriteLine("Closed serialSource");
                    }
                ).Start();

                int bytes = serialSource.Read(buffer, 0, buffer.Length);
                Console.WriteLine("Read finished, returned {0} bytes", bytes);
                if (!closedEvent.WaitOne(0)) {
                    Assert.Fail("Read returned before being disposed.");
                }
                Assert.That(bytes, Is.EqualTo(0));
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>>
        /// This test can be used to check behaviour in case that the serial port is removed.
        /// In case of the device no longer available, it should abort the read with an exception.
        /// </remarks>
        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
        public void DisconnectOnReadBlocked()
        {
            byte[] buffer = new byte[1024];

            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int bytes = serialSource.Read(buffer, 0, buffer.Length);
                Console.WriteLine("{0} bytes read", bytes);

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>>
        /// This test can be used to check behaviour in case that the serial port is removed.
        /// In case of the device no longer available, it should abort the read with an exception.
        /// </remarks>
        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
        public void DisconnectOnReadBlockedReadAgain()
        {
            byte[] buffer = new byte[1024];

            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int bytes = serialSource.Read(buffer, 0, buffer.Length);
                Console.WriteLine("{0} bytes read", bytes);

                Assert.That(
                    () => {
                        bytes = serialSource.Read(buffer, 0, buffer.Length);
                        Console.WriteLine("{0} bytes read again", bytes);
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>>
        /// This test can be used to check behaviour in case that the serial port is removed.
        /// In case of the device no longer available, it should abort the read with an exception.
        /// </remarks>
        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
        public void DisconnectOnReadCharsBlocked()
        {
            char[] buffer = new char[1024];

            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int bytes = serialSource.Read(buffer, 0, buffer.Length);
                Console.WriteLine("{0} bytes read", bytes);

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>>
        /// This test can be used to check behaviour in case that the serial port is removed.
        /// In case of the device no longer available, it should abort the read with an exception.
        /// </remarks>
        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
        public void DisconnectOnReadCharsBlockedReadAgain()
        {
            char[] buffer = new char[1024];

            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int bytes = serialSource.Read(buffer, 0, buffer.Length);
                Console.WriteLine("{0} bytes read", bytes);

                Assert.That(
                    () => {
                        bytes = serialSource.Read(buffer, 0, buffer.Length);
                        Console.WriteLine("{0} bytes read again", bytes);
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>>
        /// This test can be used to check behaviour in case that the serial port is removed.
        /// In case of the device no longer available, it should abort the read with an exception.
        /// </remarks>
        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
        public void DisconnectOnReadByteBlocked()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int c = serialSource.ReadByte();
                Console.WriteLine("{0} byte read", c);

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>>
        /// This test can be used to check behaviour in case that the serial port is removed.
        /// In case of the device no longer available, it should abort the read with an exception.
        /// </remarks>
        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
        public void DisconnectOnReadByteBlockedReadAgain()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int c = serialSource.ReadByte();
                Console.WriteLine("{0} byte read", c);

                Assert.That(
                    () => {
                        c = serialSource.ReadByte();
                        Console.WriteLine("{0} byte read again", c);
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>>
        /// This test can be used to check behaviour in case that the serial port is removed.
        /// In case of the device no longer available, it should abort the read with an exception.
        /// </remarks>
        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
        public void DisconnectOnReadCharBlocked()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int c = serialSource.ReadChar();
                Console.WriteLine("{0} char read", c);

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>>
        /// This test can be used to check behaviour in case that the serial port is removed.
        /// In case of the device no longer available, it should abort the read with an exception.
        /// </remarks>
        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
        public void DisconnectOnReadCharBlockedReadAgain()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                int c = serialSource.ReadChar();
                Console.WriteLine("{0} char read", c);

                Assert.That(
                    () => {
                        c = serialSource.ReadChar();
                        Console.WriteLine("{0} char read again", c);
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        /// <summary>
        /// Put the serial port into a read blocked state.
        /// </summary>
        /// <remarks>>
        /// This test can be used to check behaviour in case that the serial port is removed.
        /// In case of the device no longer available, it should abort the read with an exception.
        /// </remarks>
        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
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

        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
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

        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
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

        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
        public void DisconnectOnWriteAsyncBlocked()
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

        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
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

        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
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

        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
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
                Console.WriteLine("Second Read: {0}", read);
                serialSource.Close();
            }
        }

        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
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
                Console.WriteLine("Second Read: {0}", read);
            }
        }

        [Test]
        [Category("SerialPortStream.ManualTest")]
        [Explicit("Manual Test")]
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
                        Console.WriteLine("\n** Exception: {0}\n{1}\n\n", ex.Message, ex.ToString());
                    }
                    Thread.Sleep(100);
                }
            }
        }
    }
}
