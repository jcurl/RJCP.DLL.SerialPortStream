// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.SerialPortStreamTest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using NUnit.Framework;

    /// <summary>
    /// Test sending and receiving data via two serial ports.
    /// </summary>
    /// <remarks>
    /// You will need to have two serial ports connected to each other on the same computer
    /// using a NULL modem cable. Alternatively, you can use a software emulated serial
    /// port, such as com0com for tests.
    /// <para>You need to update the variables c_SourcePort and c_DestPort to be the names
    /// of the two serial ports.</para>
    /// </remarks>
    [TestFixture]
    [Timeout(10000)]
    public class SerialPortStreamTest
    {
        private readonly string c_SourcePort;
        private readonly string c_DestPort;

        public SerialPortStreamTest()
        {
            c_SourcePort = SerialConfiguration.SourcePort;
            c_DestPort = SerialConfiguration.DestPort;
        }

        private const int c_TimeOut = 300;

        [Test]
        [Category("SerialPortStream")]
        public void SimpleConstructor()
        {
            SerialPortStream src = new SerialPortStream();
            src.Dispose();
            Assert.That(src.IsDisposed, Is.True);
        }

        [Test]
        [Category("SerialPortStream")]
        public void VersionString()
        {
            using (SerialPortStream src = new SerialPortStream()) {
                Assert.That(src.Version, Is.Not.Null.Or.Empty);
                Console.WriteLine("Version: {0}", src.Version);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void SimpleConstructorWithPort()
        {
            SerialPortStream src = new SerialPortStream(c_SourcePort);
            Assert.That(src.PortName, Is.EqualTo(c_SourcePort));
            src.Dispose();
            Assert.That(src.IsDisposed, Is.True);
        }

        [Test]
        [Category("SerialPortStream")]
        public void SimpleConstructorWithPortGetSettings()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort)) {
                Assert.That(src.PortName, Is.EqualTo(c_SourcePort));
                src.GetPortSettings();
                Console.WriteLine("    PortName: {0}", src.PortName);
                Console.WriteLine("    BaudRate: {0}", src.BaudRate);
                Console.WriteLine("    DataBits: {0}", src.DataBits);
                Console.WriteLine("      Parity: {0}", src.Parity);
                Console.WriteLine("    StopBits: {0}", src.StopBits);
                Console.WriteLine("   Handshake: {0}", src.Handshake);
                Console.WriteLine(" DiscardNull: {0}", src.DiscardNull);
                Console.WriteLine("  ParityRepl: {0}", src.ParityReplace);
                Console.WriteLine("TxContOnXOff: {0}", src.TxContinueOnXOff);
                Console.WriteLine("   XOffLimit: {0}", src.XOffLimit);
                Console.WriteLine("    XOnLimit: {0}", src.XOnLimit);
                Console.WriteLine("  DrvInQueue: {0}", src.DriverInQueue);
                Console.WriteLine(" DrvOutQueue: {0}", src.DriverOutQueue);
                Console.WriteLine("{0}", src.ToString());
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void SimpleConstructorWithPortGetSettings2()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                Assert.That(src.PortName, Is.EqualTo(c_SourcePort));
                src.GetPortSettings();
                Console.WriteLine("    PortName: {0}", src.PortName);
                Console.WriteLine("    BaudRate: {0}", src.BaudRate);
                Console.WriteLine("    DataBits: {0}", src.DataBits);
                Console.WriteLine("      Parity: {0}", src.Parity);
                Console.WriteLine("    StopBits: {0}", src.StopBits);
                Console.WriteLine("   Handshake: {0}", src.Handshake);
                Console.WriteLine(" DiscardNull: {0}", src.DiscardNull);
                Console.WriteLine("  ParityRepl: {0}", src.ParityReplace);
                Console.WriteLine("TxContOnXOff: {0}", src.TxContinueOnXOff);
                Console.WriteLine("   XOffLimit: {0}", src.XOffLimit);
                Console.WriteLine("    XOnLimit: {0}", src.XOnLimit);
                Console.WriteLine("  DrvInQueue: {0}", src.DriverInQueue);
                Console.WriteLine(" DrvOutQueue: {0}", src.DriverOutQueue);
                Console.WriteLine("{0}", src.ToString());
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void PropertyBaudRate()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort)) {
                src.BaudRate = 115200;
                Assert.That(src.BaudRate, Is.EqualTo(115200));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void PropertyDataBits()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort)) {
                src.DataBits = 8;
                Assert.That(src.DataBits, Is.EqualTo(8));
                src.DataBits = 7;
                Assert.That(src.DataBits, Is.EqualTo(7));
                src.DataBits = 6;
                Assert.That(src.DataBits, Is.EqualTo(6));
                src.DataBits = 5;
                Assert.That(src.DataBits, Is.EqualTo(5));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void PropertyParity()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort)) {
                src.Parity = Parity.None;
                Assert.That(src.Parity, Is.EqualTo(Parity.None));
                src.Parity = Parity.Even;
                Assert.That(src.Parity, Is.EqualTo(Parity.Even));
                src.Parity = Parity.Odd;
                Assert.That(src.Parity, Is.EqualTo(Parity.Odd));
                src.Parity = Parity.Mark;
                Assert.That(src.Parity, Is.EqualTo(Parity.Mark));
                src.Parity = Parity.Space;
                Assert.That(src.Parity, Is.EqualTo(Parity.Space));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void PropertyStopBits()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort)) {
                src.StopBits = StopBits.One;
                Assert.That(src.StopBits, Is.EqualTo(StopBits.One));
                src.StopBits = StopBits.Two;
                Assert.That(src.StopBits, Is.EqualTo(StopBits.Two));
                src.StopBits = StopBits.One5;
                Assert.That(src.StopBits, Is.EqualTo(StopBits.One5));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void PropertyDiscardNull()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort)) {
                src.DiscardNull = false;
                Assert.That(src.DiscardNull, Is.EqualTo(false));
                src.DiscardNull = true;
                Assert.That(src.DiscardNull, Is.EqualTo(true));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void PropertyParityReplace()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort)) {
                src.ParityReplace = 0;
                Assert.That(src.ParityReplace, Is.EqualTo(0));
                src.ParityReplace = (byte)'.';
                Assert.That(src.ParityReplace, Is.EqualTo((byte)'.'));
                src.ParityReplace = 255;
                Assert.That(src.ParityReplace, Is.EqualTo(255));
                src.ParityReplace = 127;
                Assert.That(src.ParityReplace, Is.EqualTo(127));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void PropertyTxContinueOnXOff()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort)) {
                src.TxContinueOnXOff = true;
                Assert.That(src.TxContinueOnXOff, Is.EqualTo(true));
                src.TxContinueOnXOff = false;
                Assert.That(src.TxContinueOnXOff, Is.EqualTo(false));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void PropertyXOffLimit()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort)) {
                src.XOffLimit = 512;
                Assert.That(src.XOffLimit, Is.EqualTo(512));
                src.XOffLimit = 2048;
                Assert.That(src.XOffLimit, Is.EqualTo(2048));
                src.XOffLimit = 1024;
                Assert.That(src.XOffLimit, Is.EqualTo(1024));
                src.XOffLimit = 8192;
                Assert.That(src.XOffLimit, Is.EqualTo(8192));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void PropertyXOnLimit()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort)) {
                src.XOnLimit = 512;
                Assert.That(src.XOnLimit, Is.EqualTo(512));
                src.XOnLimit = 2048;
                Assert.That(src.XOnLimit, Is.EqualTo(2048));
                src.XOnLimit = 1024;
                Assert.That(src.XOnLimit, Is.EqualTo(1024));
                src.XOnLimit = 8192;
                Assert.That(src.XOnLimit, Is.EqualTo(8192));
            }
        }

        /// <summary>
        /// Test the basic features of a serial port.
        /// </summary>
        [Test]
        [Category("SerialPortStream")]
        public void OpenCloseBasicProperties()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 100;
                src.ReadTimeout = 100;

                Assert.That(src.CanRead, Is.True);
                Assert.That(src.CanWrite, Is.False);
                Assert.That(src.IsOpen, Is.False);
                Assert.That(src.PortName, Is.EqualTo(c_SourcePort));
                Assert.That(src.BytesToRead, Is.EqualTo(0));
                Assert.That(src.BytesToWrite, Is.EqualTo(0));

                src.Open();
                Assert.That(src.CanRead, Is.True);
                Assert.That(src.CanWrite, Is.True);
                Assert.That(src.IsOpen, Is.True);

                src.Close();
                Assert.That(src.CanRead, Is.True);
                Assert.That(src.CanWrite, Is.False);
                Assert.That(src.IsOpen, Is.False);
            }
        }

        /// <summary>
        /// Check that Open() and Close() can be called multiple times.
        /// </summary>
        [Test]
        [Category("SerialPortStream")]
        public void OpenClose()
        {
            SerialPortStream src;
            using (src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 100;
                src.ReadTimeout = 100;

                Assert.That(src.CanRead, Is.True);
                Assert.That(src.CanWrite, Is.False);
                Assert.That(src.IsOpen, Is.False);
                Assert.That(src.IsDisposed, Is.False);

                src.Open();
                Assert.That(src.CanRead, Is.True);
                Assert.That(src.CanWrite, Is.True);
                Assert.That(src.IsOpen, Is.True);
                Assert.That(src.IsDisposed, Is.False);

                src.Close();
                Assert.That(src.CanRead, Is.True);
                Assert.That(src.CanWrite, Is.False);
                Assert.That(src.IsOpen, Is.False);
                Assert.That(src.IsDisposed, Is.False);

                src.Open();
                Assert.That(src.CanRead, Is.True);
                Assert.That(src.CanWrite, Is.True);
                Assert.That(src.IsOpen, Is.True);
                Assert.That(src.IsDisposed, Is.False);

                src.Close();
                Assert.That(src.CanRead, Is.True);
                Assert.That(src.CanWrite, Is.False);
                Assert.That(src.IsOpen, Is.False);
                Assert.That(src.IsDisposed, Is.False);
            }
            Assert.That(src.IsDisposed, Is.True);
        }

        [Test]
        [Category("SerialPortStream")]
        public void OpenInUse()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open();

                using (SerialPortStream s2 = new SerialPortStream(c_SourcePort, 9600, 8, Parity.None, StopBits.One)) {
                    // The port is already open by src, and should be an exclusive resource.
                    Assert.That(() => s2.Open(), Throws.Exception);
                }
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void GetPortSettings()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.GetPortSettings();
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void GetPortSettingsWithNoPort()
        {
            using (SerialPortStream s2 = new SerialPortStream()) {
                Assert.That(() => s2.GetPortSettings(), Throws.Exception);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ModemSignals()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort)) {
                src.Handshake = Handshake.None;
                dst.Handshake = Handshake.None;

                src.Open();
                dst.Open();

                src.RtsEnable = false;
                Assert.That(dst.CtsHolding, Is.False);

                src.RtsEnable = true;
                Assert.That(dst.CtsHolding, Is.True);

                src.DtrEnable = false;
                Assert.That(dst.DsrHolding, Is.False);

                src.DtrEnable = true;
                Assert.That(dst.DsrHolding, Is.True);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ModemSignalsWithSleep10()
        {
            // On some chipsets (PL2303H Win7 x86), need small delays for this test case
            // to work properly.
            using (SerialPortStream src = new SerialPortStream(c_SourcePort))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort)) {
                src.Handshake = Handshake.None;
                dst.Handshake = Handshake.None;

                src.Open();
                dst.Open();

                src.RtsEnable = false;
                Thread.Sleep(10);  // Required for PL2303H
                Assert.That(dst.CtsHolding, Is.False);

                src.RtsEnable = true;
                Thread.Sleep(10);  // Required for PL2303H
                Assert.That(dst.CtsHolding, Is.True);

                src.DtrEnable = false;
                Thread.Sleep(10);  // Required for PL2303H
                Assert.That(dst.DsrHolding, Is.False);

                src.DtrEnable = true;
                Thread.Sleep(10);  // Required for PL2303H
                Assert.That(dst.DsrHolding, Is.True);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void NewLine()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                Assert.That(() => src.NewLine = "", Throws.Exception.TypeOf<ArgumentException>(),
                    "Expected exception when setting newline to empty string");

                Assert.That(() => src.NewLine = null, Throws.Exception.TypeOf<ArgumentNullException>(),
                    "Expected exception when setting newline to null");
            }
        }

        [Test]
        [Category("SerialPortStream")]
        [Timeout(20000)]
        public void SendReceive()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                // Send Maximum data in one go
                byte[] sendbuf = new byte[src.WriteBufferSize];
                Random r = new Random();
                r.NextBytes(sendbuf);
                src.Write(sendbuf, 0, sendbuf.Length);

                // Receive sent data
                int rcv = 0;
                int c = 0;
                byte[] rcvbuf = new byte[sendbuf.Length + 10];
                while (rcv < rcvbuf.Length) {
                    Console.WriteLine("Begin Receive: Offset=" + rcv + "; Count=" + (rcvbuf.Length - rcv));
                    int b = dst.Read(rcvbuf, rcv, rcvbuf.Length - rcv);
                    if (b == 0) {
                        if (c == 0) break;
                        c++;
                    } else {
                        c = 0;
                    }
                    rcv += b;
                }

                bool dump = false;
                if (rcv != sendbuf.Length) {
                    Console.WriteLine("Read length not the same as the amount of data sent (got " + rcv + " bytes)");
                    dump = true;
                }
                for (int i = 0; i < sendbuf.Length; i++) {
                    if (sendbuf[i] != rcvbuf[i]) {
                        Console.WriteLine("Comparison failure at " + i);
                        dump = true;
                        break;
                    }
                }

                if (dump) {
                    Console.WriteLine("Send Buffer DUMP");
                    for (int i = 0; i < sendbuf.Length; i++) {
                        Console.WriteLine(sendbuf[i].ToString("X2"));
                    }

                    Console.WriteLine("Receive Buffer DUMP");
                    for (int i = 0; i < rcv; i++) {
                        Console.WriteLine(rcvbuf[i].ToString("X2"));
                    }
                }
                src.Close();
                dst.Close();
                Assert.That(dump, Is.False, "Error in transfer");
            }
        }

        private class SendReceiveAsyncState
        {
            public ManualResetEvent finished = new ManualResetEvent(false);
            public SerialPortStream src;
            public SerialPortStream dst;
            public byte[] sendBuf;
            public byte[] recvBuf;
            public int rcv;
        }

        [Test]
        [Category("SerialPortStream")]
        [Timeout(20000)]
        public void SendReceiveWithBeginEnd()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                SendReceiveAsyncState state = new SendReceiveAsyncState();
                state.src = src;
                state.dst = dst;
                state.sendBuf = new byte[src.WriteBufferSize];
                state.recvBuf = new byte[src.WriteBufferSize + 10];
                Random r = new Random();
                r.NextBytes(state.sendBuf);

                // Here we start the read and write in parallel. The read will wait up to c_Timeout for the first byte.
                dst.BeginRead(state.recvBuf, 0, state.recvBuf.Length, SendReceiveAsyncReadComplete, state);
                src.BeginWrite(state.sendBuf, 0, state.sendBuf.Length, SendReceiveAsyncWriteComplete, state);
                if (!state.finished.WaitOne(30000)) {
                    Assert.Fail("BeginWrite/BeginRead test case timeout");
                }
            }
        }

        private void SendReceiveAsyncWriteComplete(IAsyncResult ar)
        {
            SendReceiveAsyncState state = (SendReceiveAsyncState)ar.AsyncState;
            state.src.EndWrite(ar);
        }

        private void SendReceiveAsyncReadComplete(IAsyncResult ar)
        {
            SendReceiveAsyncState state = (SendReceiveAsyncState)ar.AsyncState;
            int bytes = state.dst.EndRead(ar);

            if (bytes != 0) {
                state.rcv += bytes;
                if (state.rcv < state.recvBuf.Length) {
                    Console.WriteLine("Begin Receive: Offset=" + state.rcv + "; Count=" + (state.recvBuf.Length - state.rcv));
                    state.dst.BeginRead(state.recvBuf, state.rcv, state.recvBuf.Length - state.rcv, SendReceiveAsyncReadComplete, state);
                } else {
                    state.finished.Set();
                    Assert.Fail("Received more data than expected");
                }
            } else {
                if (state.rcv != state.sendBuf.Length) {
                    state.finished.Set();
                    Assert.Fail("Received more/less data than expected: {0} expected; {1} received", state.sendBuf.Length, state.rcv);
                }
                state.finished.Set();
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void SendAndFlush1()
        {
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                // Required for com0com to send the data (the remote side must receive it)
                dst.Open();

                src.Open();
                src.WriteLine("Connected");
                src.Flush();
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void SendAndFlush2()
        {
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                // Required for com0com to send the data (the remote side must receive it)
                dst.Open();

                Trace.WriteLine("1. Open()");
                src.Open();
                Trace.WriteLine("2. WriteLine()");
                src.WriteLine("Connected");
                Trace.WriteLine("3. Sleep()");
                Thread.Sleep(100);
                Trace.WriteLine("4. WriteLine()");
                src.WriteLine("Disconnected");
                Trace.WriteLine("5. Flush()");
                src.Flush();
                Trace.WriteLine("6. Dispose()");
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ReadChars()
        {
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 2 * c_TimeOut + 500; src.ReadTimeout = 2 * c_TimeOut + 500;
                dst.WriteTimeout = 2 * c_TimeOut + 500; dst.ReadTimeout = 2 * c_TimeOut + 500;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                src.Write(send, 0, send.Length);
                Thread.Sleep(c_TimeOut + 500);

                char[] recv = new char[5];
                int cread = dst.Read(recv, 0, recv.Length);
                Assert.That(cread, Is.EqualTo(3));
                Assert.That(recv[0], Is.EqualTo('e'));
                Assert.That(recv[1], Is.EqualTo('f'));
                Assert.That(recv[2], Is.EqualTo('g'));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ReadCharsWithTimeout()
        {
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 2 * c_TimeOut + 500; src.ReadTimeout = 2 * c_TimeOut + 500;
                dst.WriteTimeout = 2 * c_TimeOut + 500; dst.ReadTimeout = 2 * c_TimeOut + 500;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                new Thread(() => {
                    Thread.Sleep(c_TimeOut + 500);
                    byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                    src.Write(send, 0, send.Length);
                }).Start();

                char[] recv = new char[5];
                int cread = 0; int counter = 0;
                while (cread < 3 && counter < 5) {
                    cread += dst.Read(recv, cread, recv.Length - cread);
                    counter++;
                    Console.WriteLine("dst.Read. Got cread={0} bytes in {1} loops", cread, counter);
                }

                for (int i = 0; i < cread; i++) {
                    Console.WriteLine("cread[{0}] = {1}", i, recv[i]);
                }

                Assert.That(cread, Is.EqualTo(3));
                Assert.That(recv[0], Is.EqualTo('e'));
                Assert.That(recv[1], Is.EqualTo('f'));
                Assert.That(recv[2], Is.EqualTo('g'));
            }
        }


        [Test]
        [Category("SerialPortStream")]
        public void ReadSingleChar()
        {
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 2 * c_TimeOut + 500; src.ReadTimeout = 2 * c_TimeOut + 500;
                dst.WriteTimeout = 2 * c_TimeOut + 500; dst.ReadTimeout = 2 * c_TimeOut + 500;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                src.Write(send, 0, send.Length);
                Thread.Sleep(c_TimeOut + 500);

                Assert.That(dst.ReadChar(), Is.EqualTo((int)'e'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'f'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'g'));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ReadSingleCharWithTimeout()
        {
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 2 * c_TimeOut + 500; src.ReadTimeout = 2 * c_TimeOut + 500;
                dst.WriteTimeout = 2 * c_TimeOut + 500; dst.ReadTimeout = 2 * c_TimeOut + 500;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                new Thread(() => {
                    Thread.Sleep(c_TimeOut + 500);
                    byte[] send = new byte[] { 0x65, 0x66, 0x67 };
                    src.Write(send, 0, send.Length);
                }).Start();

                Assert.That(dst.ReadChar(), Is.EqualTo((int)'e'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'f'));
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'g'));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ReadSingleCharEuro()
        {
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                byte[] send = new byte[] { 0xE2, 0x82, 0xAC };
                src.Write(send, 0, send.Length);
                Assert.That(dst.ReadChar(), Is.EqualTo((int)'€'));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ReadSingleCharUtf32()
        {
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
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
        [Category("SerialPortStream")]
        public void ListPorts()
        {
            bool result = true;

            Dictionary<string, bool> ports1 = new Dictionary<string, bool>();
            Dictionary<string, bool> ports2 = new Dictionary<string, bool>();

            foreach (PortDescription desc in SerialPortStream.GetPortDescriptions()) {
                Trace.WriteLine("GetPortDescriptions: " + desc.Port + "; Description: " + desc.Description);
                ports1.Add(desc.Port, false);
                ports2.Add(desc.Port, false);
            }

            foreach (string c in SerialPortStream.GetPortNames()) {
                Trace.WriteLine("GetPortNames: " + c);
                if (ports1.ContainsKey(c)) {
                    ports1[c] = true;
                } else {
                    Trace.WriteLine("GetPortNames() shows " + c + ", but not GetPortDescriptions()");
                    result = false;
                }
            }
            foreach (string c in ports1.Keys) {
                if (ports1[c] == false) {
                    Trace.WriteLine("GetPortDescriptions() shows " + c + ", but not GetPortnames()");
                    result = false;
                }
            }

            foreach (string c in System.IO.Ports.SerialPort.GetPortNames()) {
                Trace.WriteLine("SerialPort.GetPortNames: " + c);
                if (ports2.ContainsKey(c)) {
                    ports2[c] = true;
                } else {
                    Trace.WriteLine("System.IO.Ports.SerialPort.GetPortNames() shows " + c + ", but not GetPortDescriptions()");
                    result = false;
                }
            }
            foreach (string c in ports1.Keys) {
                if (ports2[c] == false) {
                    Trace.WriteLine("GetPortDescriptions() shows " + c + ", but not System.IO.Ports.SerialPort.GetPortNames()");
                    result = false;
                }
            }

            Assert.IsTrue(result);
        }

        [Test]
        [Category("SerialPortStream")]
        public void WriteLineReadLine()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                string s;
                src.WriteLine("TestString");
                s = dst.ReadLine();
                Assert.That(s, Is.EqualTo("TestString"));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WriteLineReadLineTimeout1()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                string s;

                src.Write("TestString");
                Assert.Throws<TimeoutException>(() => { s = dst.ReadLine(); }, "No timeout exception occurred");

                src.WriteLine("");
                s = dst.ReadLine();
                Assert.That(s, Is.EqualTo("TestString"));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WriteLineReadLineTimeout2()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                string s;

                src.Write("Test");
                Assert.Throws<TimeoutException>(() => { s = dst.ReadLine(); }, "No timeout exception occurred");

                src.WriteLine("String");
                s = dst.ReadLine();
                Assert.AreEqual("TestString", s);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WriteLineReadLineMultilines()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                string s;
                src.Write("Line1\nLine2\n");
                s = dst.ReadLine();
                Assert.AreEqual("Line1", s);
                s = dst.ReadLine();
                Assert.AreEqual("Line2", s);

                Assert.Throws<TimeoutException>(() => { s = dst.ReadLine(); }, "No timeout exception occurred");
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WriteLineReadLineCharForChar()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                bool err = false;
                string s = null;

                const string send = "A Brief History Of Time\n";

                for (int i = 0; i < send.Length; i++) {
                    src.Write(send[i].ToString());
                    try {
                        s = dst.ReadLine();
                    } catch (System.Exception e) {
                        if (e is TimeoutException) err = true;
                    }
                    if (i < send.Length - 1) {
                        Assert.That(err, Is.True, "No timeout exception occurred when waiting for " + send[i] + " (position " + i + ")");
                    }
                }
                Assert.That(s, Is.EqualTo("A Brief History Of Time"));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WriteLineReadLineMbcsByteForByte()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
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
                    } catch (System.Exception e) {
                        if (e is TimeoutException) err = true;
                    }
                    if (i < buf.Length - 1) {
                        Assert.That(err, Is.True, "No timeout exception occurred when waiting for " + buf[i].ToString("X2") + " (position " + i + ")");
                    }
                }
                Assert.That(s, Is.EqualTo("ABCDEFGHIJ€"));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ReadToCached()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                string s;

                src.Write("foobar");
                Assert.Throws<TimeoutException>(() => { s = dst.ReadTo("baz"); }, "No timeout exception occurred when reading 'baz'");

                s = dst.ReadTo("foo");
                Assert.That(s, Is.EqualTo(""));

                s = dst.ReadTo("bar");
                Assert.That(s, Is.EqualTo(""));

                Assert.Throws<TimeoutException>(() => { s = dst.ReadTo("baz"); }, "No timeout exception occurred when reading 'baz' when empty");
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ReadToNormal()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                string s;

                src.Write("superfoobar");
                s = dst.ReadTo("foo");
                Assert.That(s, Is.EqualTo("super"));

                // Sleep for 100ms to allow all data to be sent and received. Else we might not receive the
                // entire string, and sometimes only get it partially.
                Thread.Sleep(100);
                s = dst.ReadExisting();
                Assert.That(s, Is.EqualTo("bar"));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ReadToOverflow()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
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
                src.Write("EOF");
                while (dst.BytesToRead < sdata.Length) {
                    Thread.Sleep(100);
                }

                string result = dst.ReadTo("EOF");
                Assert.That(dst.BytesToRead, Is.EqualTo(0));
                Assert.That(result.Length, Is.EqualTo(1024 - 3));
                int offset = sdata.Length - result.Length;
                for (int i = 0; i < result.Length; i++) {
                    Assert.That((int)result[i], Is.EqualTo(sdata[offset + i]));
                }
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ReadToWithMbcs()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
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
        [Category("SerialPortStream")]
        public void ReadToResetWithMbcs1()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
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
        [Category("SerialPortStream")]
        public void ReadToResetWithMbcs2()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
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
        [Category("SerialPortStream")]
        public void ReadToResetWithMbcs3()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
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
        [Category("SerialPortStream")]
        public void ReadToResetWithOverflow()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
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
        [Category("SerialPortStream")]
        public void ReadToWithOverflow2()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
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
                Assert.That(line.Length, Is.EqualTo(1023));   // Is 1024 - Length('\n').
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void Flush()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                byte[] sdata = new byte[512];
                for (int i = 0; i < sdata.Length; i++) {
                    sdata[i] = (byte)(64 + i % 48);
                }

                // It should take 512 * 10 / 115200 s = 44ms to send, timeout of 300ms.
                src.Write(sdata, 0, sdata.Length);
                src.Flush();
                Assert.That(src.BytesToWrite, Is.EqualTo(0));

                src.Write(sdata, 0, sdata.Length);
                src.Flush();
                Assert.That(src.BytesToWrite, Is.EqualTo(0));
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void SettingsOnOpenParity()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.Odd, StopBits.One)) {
                Assert.That(src.Parity, Is.EqualTo(Parity.Odd));
                src.Open();
                Assert.That(src.Parity, Is.EqualTo(Parity.Odd));
            }
        }

        private void TestOddParity(SerialPortStream src, SerialPortStream dst)
        {
            src.Write(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0, 16);
            src.Flush();

            int offset = 0;
            int counter = 0;
            byte[] recv = new byte[256];
            while (offset < 16 && counter < 10) {
                offset += dst.Read(recv, offset, recv.Length - offset);
                counter++;
                Console.WriteLine("Buffer Bytes Received: {0}; Read attempts: {1}", offset, counter);
            }

            for (int i = 0; i < offset; i++) {
                Console.WriteLine("Offset: {0} = {1:X2}", i, recv[i]);
            }

            Assert.That(offset, Is.EqualTo(16), "Expected 16 bytes received, but only got {0} bytes", offset);
            byte[] expectedrecv = new byte[] { 0x80, 0x01, 0x02, 0x83, 0x04, 0x85, 0x86, 0x07, 0x08, 0x89, 0x8A, 0x0B, 0x8C, 0x0D, 0x0E, 0x8F };
            for (int i = 0; i < offset; i++) {
                Assert.That(recv[i], Is.EqualTo(expectedrecv[i]), "Offset {0} got {1}; expected {2}", i, recv[i], expectedrecv[i]);
            }
        }

        private void TestEvenParity(SerialPortStream src, SerialPortStream dst)
        {
            src.Write(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F }, 0, 16);
            src.Flush();

            int offset = 0;
            int counter = 0;
            byte[] recv = new byte[256];
            while (offset < 16 && counter < 10) {
                offset += dst.Read(recv, offset, recv.Length - offset);
                counter++;
                Console.WriteLine("Buffer Bytes Received: {0}; Read attempts: {1}", offset, counter);
            }

            for (int i = 0; i < offset; i++) {
                Console.WriteLine("Offset: {0} = {1:X2}", i, recv[i]);
            }

            Assert.That(offset, Is.EqualTo(16), "Expected 16 bytes received, but only got {0} bytes", offset);
            byte[] expectedrecv = new byte[] { 0x00, 0x81, 0x82, 0x03, 0x84, 0x05, 0x06, 0x87, 0x88, 0x09, 0x0A, 0x8B, 0x0C, 0x8D, 0x8E, 0x0F };
            for (int i = 0; i < offset; i++) {
                Assert.That(recv[i], Is.EqualTo(expectedrecv[i]), "Offset {0} got {1}; expected {2}", i, recv[i], expectedrecv[i]);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void OddParityLoopback()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 7, Parity.Odd, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.Open(); dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;

                TestOddParity(src, dst);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void EvenParityLoopback()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 7, Parity.Even, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.Open(); dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;

                TestEvenParity(src, dst);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void ParityChangeLoopback()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 7, Parity.Even, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.Open(); dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;
                TestEvenParity(src, dst);

                src.Parity = Parity.Odd;
                dst.Parity = Parity.None;
                TestOddParity(src, dst);

                src.Parity = Parity.None;
                src.DataBits = 8;
                dst.DataBits = 7;
                dst.Parity = Parity.Odd;
                TestOddParity(dst, src);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WaitForRxCharEventOn1Byte()
        {
            using (ManualResetEvent rxChar = new ManualResetEvent(false))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.Open(); dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;

                dst.ReceivedBytesThreshold = 1;
                dst.DataReceived += (s, a) => { rxChar.Set(); };
                src.WriteByte(0x00);
                Assert.That(rxChar.WaitOne(1000), Is.True);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WaitForRxCharEventOn2Bytes()
        {
            using (ManualResetEvent rxChar = new ManualResetEvent(false))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.Open(); dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;

                dst.ReceivedBytesThreshold = 2;
                dst.DataReceived += (s, a) => { rxChar.Set(); };
                src.WriteByte(0x00);
                Assert.That(rxChar.WaitOne(1000), Is.False);
                src.WriteByte(0x01);
                Assert.That(rxChar.WaitOne(1000), Is.True);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WaitForRxCharEventOnEofChar()
        {
            using (ManualResetEvent rxChar = new ManualResetEvent(false))
            using (ManualResetEvent eofChar = new ManualResetEvent(false))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.Open(); dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;

                dst.ReceivedBytesThreshold = 5;
                dst.DataReceived += (s, a) => {
                    if (a.EventType == SerialData.Chars) rxChar.Set();
                    if (a.EventType == SerialData.Eof) eofChar.Set();
                };
                src.WriteByte(0x00);
                Assert.That(rxChar.WaitOne(500), Is.False);
                Assert.That(eofChar.WaitOne(0), Is.False);
                src.WriteByte(0x01);
                Assert.That(rxChar.WaitOne(500), Is.False);
                Assert.That(eofChar.WaitOne(0), Is.False);
                src.WriteByte(0x1A);
                Assert.That(rxChar.WaitOne(500), Is.False);
                Assert.That(eofChar.WaitOne(0), Is.True);
                eofChar.Reset();
                src.WriteByte(0x02);
                Assert.That(rxChar.WaitOne(500), Is.False);
                Assert.That(eofChar.WaitOne(0), Is.False);
                src.WriteByte(0x03);
                Assert.That(rxChar.WaitOne(500), Is.True);
                Assert.That(eofChar.WaitOne(0), Is.False);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WaitForCtsChangedEvent()
        {
            using (ManualResetEvent cts = new ManualResetEvent(false))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.Open(); dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;

                Thread.Sleep(300);   // Wait for events to be cleared after opening.

                dst.PinChanged += (s, a) => {
                    if (a.EventType.HasFlag(SerialPinChange.CtsChanged)) cts.Set();
                };

                // By default, the RTS is enabled. So we toggle it to off.
                src.RtsEnable = false;

                Assert.That(cts.WaitOne(500), Is.True);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WaitForDsrChangedEvent()
        {
            using (ManualResetEvent dsr = new ManualResetEvent(false))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = c_TimeOut; src.ReadTimeout = c_TimeOut;
                dst.Open(); dst.WriteTimeout = c_TimeOut; dst.ReadTimeout = c_TimeOut;

                Thread.Sleep(300);   // Wait for events to be cleared after opening.

                dst.PinChanged += (s, a) => {
                    if (a.EventType.HasFlag(SerialPinChange.DsrChanged)) dsr.Set();
                };

                // By default, the DTR is enabled. So we toggle it to off.
                src.DtrEnable = false;

                Assert.That(dsr.WaitOne(500), Is.True);
            }
        }

        [Test]
        [Category("SerialPortStream")]
        public void WriteWhenClosedException()
        {
            byte[] buffer = new byte[256];

            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                Assert.That(() => {
                    src.Write(buffer, 0, buffer.Length);
                }, Throws.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        [Category("SerialPortStream")]
        [Timeout(20000)]
        public void DisposedWhenBlocked()
        {
            byte[] buffer = new byte[1024];
            
            SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One);
            Thread testThread;

            using (ManualResetEvent disposedEvent = new ManualResetEvent(false))
            using (SerialPortStream serialDest = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
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
        [Category("SerialPortStream")]
        [Timeout(20000)]
        public void ClosedWhenBlocked()
        {
            byte[] buffer = new byte[1024];

            using (ManualResetEvent closedEvent = new ManualResetEvent(false))
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
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
        [Category("SerialPortStream")]
        [Timeout(20000)]
        public void DisposedWhenFlushBlocked()
        {
            byte[] buffer = new byte[8192];

            SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One);
            Thread testThread;

            using (ManualResetEvent disposedEvent = new ManualResetEvent(false))
            using (SerialPortStream serialDest = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
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
        [Category("SerialPortStream")]
        [Timeout(20000)]
        public void ClosedWhenFlushBlocked()
        {
            byte[] buffer = new byte[8192];

            using (ManualResetEvent closedEvent = new ManualResetEvent(false))
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
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
        [Category("SerialPortStream")]
        [Timeout(20000)]
        public void DisposedWhenReadBlocked()
        {
            byte[] buffer = new byte[1024];

            SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One);
            Thread testThread;

            using (ManualResetEvent disposedEvent = new ManualResetEvent(false))
            using (SerialPortStream serialDest = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
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
        [Category("SerialPortStream")]
        [Timeout(20000)]
        public void ClosedWhenReadBlocked()
        {
            byte[] buffer = new byte[1024];

            using (ManualResetEvent closedEvent = new ManualResetEvent(false))
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnReadBlocked()
        {
            byte[] buffer = new byte[1024];

            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnReadBlockedReadAgain()
        {
            byte[] buffer = new byte[1024];

            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnReadCharsBlocked()
        {
            char[] buffer = new char[1024];

            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnReadCharsBlockedReadAgain()
        {
            char[] buffer = new char[1024];

            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnReadByteBlocked()
        {
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnReadByteBlockedReadAgain()
        {
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnReadCharBlocked()
        {
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnReadCharBlockedReadAgain()
        {
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnReadLineBlocked()
        {
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();

                Assert.That(
                    () => {
                        string l = serialSource.ReadLine();
                        Console.WriteLine("line read length={0} ({1})", l == null ? -1 : l.Length, l == null ? "" : l);
                    }, Throws.InstanceOf<System.IO.IOException>());

                // Device should still be open.
                Assert.That(serialSource.IsOpen, Is.True);
                serialSource.Close();
            }
        }

        [Test]
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnFlushBlocked()
        {
            byte[] buffer = new byte[8192];
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnWriteBlocked()
        {
            byte[] buffer = new byte[8192];
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
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
        //[Ignore("Manual Test")]
        [Category("SerialPortStream.ManualTest")]
        public void DisconnectOnWriteAsyncBlocked()
        {
            byte[] buffer = new byte[8192];
            using (SerialPortStream serialSource = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
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
