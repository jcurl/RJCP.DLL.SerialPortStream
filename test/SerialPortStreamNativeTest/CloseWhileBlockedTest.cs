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
    /// Test sending closing / disposing Serial Ports in blocked states for bad behaviour.
    /// </summary>
    /// <remarks>
    /// You will need to have two serial ports connected to each other on the same computer using a NULL modem cable.
    /// Alternatively, you can use a software emulated serial port, such as com0com for tests.
    /// <para>You need to update the variables SourcePort and DestPort to be the names of the two serial ports.</para>
    /// </remarks>
    [TestFixture]
    [Explicit("Fails on Linux")]
    [Category("ManualTest")]
    [Timeout(10000)]
    public class CloseWhileBlockedTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;
        private readonly string DestPort = SerialConfiguration.DestPort;

        private const int SleepDelay = 1000;

#if NET6_0_OR_GREATER
        [SetUp]
        public void InitLogging()
        {
            Trace.GlobalLogger.Initialize();
        }
#endif

        [Test]
        public void DisposedWhenBlocked()
        {
            byte[] buffer = new byte[1024];

            SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One);
            Task testTask;

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

                testTask = new TaskFactory().StartNew(() => {
                    Thread.Sleep(SleepDelay);
                    Console.WriteLine("Disposing serialSource");

                    // It appears that the MSDN .NET implementation blocks here, never
                    // to return as we're blocked on another thread.
                    disposedEvent.Set();
                    serialSource.Dispose();
                    Console.WriteLine("Disposed serialSource");
                });

                Assert.That(
                    () => {
                        int bufferCount = 1024 * 1024;
                        while (bufferCount > 0) {
                            serialSource.Write(buffer, 0, buffer.Length);
                            if (disposedEvent.WaitOne(0)) {
                                Assert.Fail("Write returned after being disposed.");
                            }
                            bufferCount -= buffer.Length;
                            Console.WriteLine($"{bufferCount}");
                        }
                    }, Throws.InstanceOf<ObjectDisposedException>());
            }

            testTask.Wait(5000 + SleepDelay);
            Console.WriteLine("Finished");
        }

        [Test]
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

                Task serial = new TaskFactory().StartNew(() => {
                    Thread.Sleep(SleepDelay);
                    Console.WriteLine("Closing serialSource");

                    // It appears that the MSDN .NET implementation blocks here, never
                    // to return as we're blocked on another thread.
                    closedEvent.Set();
                    serialSource.Close();
                    Console.WriteLine("Closed serialSource");
                });

                Assert.That(
                    () => {
                        int bufferCount = 1024 * 1024;
                        while (bufferCount > 0) {
                            serialSource.Write(buffer, 0, buffer.Length);
                            if (closedEvent.WaitOne(0)) {
                                Assert.Fail("Write returned after being closed.");
                            }
                            bufferCount -= buffer.Length;
                            Console.WriteLine($"{bufferCount}");
                        }
                    }, Throws.InstanceOf<System.IO.IOException>());

                serial.Wait();
            }
        }

        [Test]
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

                Task serial = new TaskFactory().StartNew(() => {
                    Thread.Sleep(SleepDelay);
                    Console.WriteLine("Closing serialSource but first setting handshake to NONE");

                    // It appears that the MSDN .NET implementation blocks here, never
                    // to return as we're blocked on another thread.
                    closedEvent.Set();

                    // In an attempt to "unblock" on MONO, we thought we could set the
                    // handshake to none. What happens instead, it blocks here which
                    // is also an error, and so a new test case.
                    serialSource.Handshake = Handshake.None;
                    Console.WriteLine("Closing serialSource");
                    serialSource.Close();
                    Console.WriteLine("Closed serialSource");
                });

                Assert.That(
                    () => {
                        int bufferCount = 1024 * 1024;
                        while (bufferCount > 0) {
                            serialSource.Write(buffer, 0, buffer.Length);
                            if (closedEvent.WaitOne(0)) {
                                Assert.Fail("Write returned after being closed.");
                            }
                            bufferCount -= buffer.Length;
                            Console.WriteLine($"{bufferCount}");
                        }
                    }, Throws.InstanceOf<System.IO.IOException>());

                serial.Wait();
            }
        }

        [Test]
        public void DisposedWhenFlushBlocked()
        {
            byte[] buffer = new byte[8192];

            SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One);
            Task testTask;

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

                testTask = new TaskFactory().StartNew(() => {
                    Thread.Sleep(SleepDelay);
                    Console.WriteLine("Disposing serialSource");

                    // It appears that the MSDN .NET implementation blocks here, never
                    // to return as we're blocked on another thread.
                    disposedEvent.Set();
                    serialSource.Dispose();
                    Console.WriteLine("Disposed serialSource");
                });

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

            testTask.Wait(5000 + SleepDelay);
            Console.WriteLine("Finished");
        }

        [Test]
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

                Task serial = new TaskFactory().StartNew(() => {
                    Thread.Sleep(SleepDelay);
                    Console.WriteLine("Closing serialSource");

                    // It appears that the MSDN .NET implementation blocks here, never
                    // to return as we're blocked on another thread.
                    closedEvent.Set();
                    serialSource.Close();
                    Console.WriteLine("Closed serialSource");
                });

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

                serial.Wait();
            }
        }

        [Test]
        public void DisposedWhenReadBlocked()
        {
            byte[] buffer = new byte[1024];

            SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One);
            Task testTask;

            using (ManualResetEvent disposedEvent = new ManualResetEvent(false))
            using (SerialPortStream serialDest = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                serialDest.Open();

                testTask = new TaskFactory().StartNew(() => {
                    Thread.Sleep(SleepDelay);
                    Console.WriteLine("Disposing serialSource");

                    // It appears that the MSDN .NET implementation blocks here, never
                    // to return as we're blocked on another thread.
                    disposedEvent.Set();
                    serialSource.Dispose();
                    Console.WriteLine("Disposed serialSource");
                });

                Assert.That(
                    () => {
                        int bytes = serialSource.Read(buffer, 0, buffer.Length);
                        Console.WriteLine($"Read finished, returned {bytes} bytes");
                        if (disposedEvent.WaitOne(0)) {
                            Assert.Fail("Read returned after being disposed.");
                        }
                    }, Throws.InstanceOf<ObjectDisposedException>());
            }

            testTask.Wait(5000 + SleepDelay);
            Console.WriteLine("Finished");
        }

        [Test]
        public void ClosedWhenReadBlocked()
        {
            byte[] buffer = new byte[1024];

            using (ManualResetEvent closedEvent = new ManualResetEvent(false))
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                serialDest.Open();

                Task serial = new TaskFactory().StartNew(() => {
                    Thread.Sleep(SleepDelay);
                    Console.WriteLine("Closing serialSource");

                    // It appears that the MSDN .NET implementation blocks here, never
                    // to return as we're blocked on another thread.
                    closedEvent.Set();
                    serialSource.Close();
                    Console.WriteLine("Closed serialSource");
                });

                int bytes = serialSource.Read(buffer, 0, buffer.Length);
                Console.WriteLine($"Read finished, returned {bytes} bytes");
                if (!closedEvent.WaitOne(0)) {
                    Assert.Fail("Read returned before being disposed.");
                }
                Assert.That(bytes, Is.EqualTo(0));

                serial.Wait();
            }
        }
    }
}
