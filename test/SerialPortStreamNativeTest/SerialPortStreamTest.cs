// Copyright © Jason Curl 2012-2023
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
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
    /// <para>You need to update the variables SourcePort and DestPort to be the names of the two serial ports.</para>
    /// </remarks>
    [TestFixture]
    [Timeout(10000)]
    public class SerialPortStreamTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;
        private readonly string DestPort = SerialConfiguration.DestPort;

#if NETCOREAPP3_1
        [SetUp]
        public void InitLogging()
        {
            Trace.GlobalLogger.Initialize();
        }
#endif

        [Test]
        public void GetSettings()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
                Assert.That(src.PortName, Is.EqualTo(SourcePort));
                src.GetPortSettings();
                Console.WriteLine($"    PortName: {src.PortName}");
                Console.WriteLine($"    BaudRate: {src.BaudRate}");
                Console.WriteLine($"    DataBits: {src.DataBits}");
                Console.WriteLine($"      Parity: {src.Parity}");
                Console.WriteLine($"    StopBits: {src.StopBits}");
                Console.WriteLine($"   Handshake: {src.Handshake}");
                Console.WriteLine($" DiscardNull: {src.DiscardNull}");
                Console.WriteLine($"  ParityRepl: {src.ParityReplace}");
                Console.WriteLine($"TxContOnXOff: {src.TxContinueOnXOff}");
                Console.WriteLine($"   XOffLimit: {src.XOffLimit}");
                Console.WriteLine($"    XOnLimit: {src.XOnLimit}");
                Console.WriteLine($"  DrvInQueue: {src.DriverInQueue}");
                Console.WriteLine($" DrvOutQueue: {src.DriverOutQueue}");
                Console.WriteLine($"{src}");
            }
        }

        [Test]
        public void GetSettingsWithSetProperties()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                Assert.That(src.PortName, Is.EqualTo(SourcePort));
                src.GetPortSettings();
                Console.WriteLine($"    PortName: {src.PortName}");
                Console.WriteLine($"    BaudRate: {src.BaudRate}");
                Console.WriteLine($"    DataBits: {src.DataBits}");
                Console.WriteLine($"      Parity: {src.Parity}");
                Console.WriteLine($"    StopBits: {src.StopBits}");
                Console.WriteLine($"   Handshake: {src.Handshake}");
                Console.WriteLine($" DiscardNull: {src.DiscardNull}");
                Console.WriteLine($"  ParityRepl: {src.ParityReplace}");
                Console.WriteLine($"TxContOnXOff: {src.TxContinueOnXOff}");
                Console.WriteLine($"   XOffLimit: {src.XOffLimit}");
                Console.WriteLine($"    XOnLimit: {src.XOnLimit}");
                Console.WriteLine($"  DrvInQueue: {src.DriverInQueue}");
                Console.WriteLine($" DrvOutQueue: {src.DriverOutQueue}");
                Console.WriteLine($"{src}");
            }
        }

        [Test]
        public void OpenCloseBasicProperties()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 100;
                src.ReadTimeout = 100;

                Assert.That(src.CanRead, Is.True);
                Assert.That(src.CanWrite, Is.False);
                Assert.That(src.IsOpen, Is.False);
                Assert.That(src.PortName, Is.EqualTo(SourcePort));
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

        [Test]
        [Repeat(100)]
        public void OpenClose()
        {
            SerialPortStream src;
            using (src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
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
        public void OpenInUse()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open();

                using (SerialPortStream s2 = new SerialPortStream(SourcePort, 9600, 8, Parity.None, StopBits.One)) {
                    // The port is already open by src, and should be an exclusive resource.
                    Assert.That(() => s2.Open(), Throws.Exception.InstanceOf<UnauthorizedAccessException>());
                }
            }
        }

        [Test]
        public void ModemSignals()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort))
            using (SerialPortStream dst = new SerialPortStream(DestPort)) {
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
        public void ModemSignalsWithSleep10()
        {
            // On some chipsets (PL2303H Win7 x86), need small delays for this test case to work properly.
            using (SerialPortStream src = new SerialPortStream(SourcePort))
            using (SerialPortStream dst = new SerialPortStream(DestPort)) {
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
        public void DiscardInBuffer()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                serialSource.DiscardInBuffer();
            }
        }

        [Test]
        public void DiscardOutBuffer()
        {
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.Open();
                serialSource.DiscardOutBuffer();
            }
        }

        [Test]
        public void DiscardOutBufferAfterWrite()
        {
            var buffer = new byte[65536];

            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortReceive.IdleReceive(DestPort, serialSource)) {
                serialSource.Open();
                serialSource.Write(buffer, 0, buffer.Length);
                serialSource.DiscardOutBuffer();
                Thread.Sleep(50);
                Assert.That(serialSource.BytesToWrite, Is.EqualTo(0));
            }
        }

        [Test]
        public void RtsEnableBeforeOpen()
        {
            SerialPortStream serial = null;
            try {
                serial = new SerialPortStream(SourcePort) {
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    RtsEnable = true,
                    StopBits = StopBits.One,
                    ReadTimeout = -1,
                    WriteTimeout = -1
                };

                serial.Open();
                Assert.That(serial.RtsEnable, Is.True);
            } finally {
                if (serial != null) serial.Dispose();
            }
        }

        [Test]
        public void RtsDisableBeforeOpen()
        {
            SerialPortStream serial = null;
            try {
                serial = new SerialPortStream(SourcePort) {
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    RtsEnable = false,
                    StopBits = StopBits.One,
                    ReadTimeout = -1,
                    WriteTimeout = -1
                };

                serial.Open();
                Assert.That(serial.RtsEnable, Is.False);
            } finally {
                if (serial != null) serial.Dispose();
            }
        }

        [Test]
        public void DtrEnableBeforeOpen()
        {
            SerialPortStream serial = null;
            try {
                serial = new SerialPortStream(SourcePort) {
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    DtrEnable = true,
                    StopBits = StopBits.One,
                    ReadTimeout = -1,
                    WriteTimeout = -1
                };

                serial.Open();
                Assert.That(serial.DtrEnable, Is.True);
            } finally {
                if (serial != null) serial.Dispose();
            }
        }

        [Test]
        public void DtrDisableBeforeOpen()
        {
            SerialPortStream serial = null;
            try {
                serial = new SerialPortStream(SourcePort) {
                    BaudRate = 115200,
                    DataBits = 8,
                    Parity = Parity.None,
                    DtrEnable = false,
                    StopBits = StopBits.One,
                    ReadTimeout = -1,
                    WriteTimeout = -1
                };

                serial.Open();
                Assert.That(serial.DtrEnable, Is.False);
            } finally {
                if (serial != null) serial.Dispose();
            }
        }
    }
}
