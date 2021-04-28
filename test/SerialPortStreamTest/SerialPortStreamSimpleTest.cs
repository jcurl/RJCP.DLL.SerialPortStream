// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.SerialPortStreamTest
{
    using System;
    using System.Collections.Generic;
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
    public class SerialPortStreamSimpleTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;
        private readonly string DestPort = SerialConfiguration.DestPort;

        [Test]
        public void SimpleConstructor()
        {
            SerialPortStream src = new SerialPortStream();
            src.Dispose();
            Assert.That(src.IsDisposed, Is.True);
        }

        [Test]
        public void VersionString()
        {
            using (SerialPortStream src = new SerialPortStream()) {
                Assert.That(src.Version, Is.Not.Null.Or.Empty);
                Console.WriteLine("Version: {0}", src.Version);
            }
        }

        [Test]
        public void SimpleConstructorWithPort()
        {
            SerialPortStream src = new SerialPortStream(SourcePort);
            Assert.That(src.PortName, Is.EqualTo(SourcePort));
            src.Dispose();
            Assert.That(src.IsDisposed, Is.True);
        }

        [Test]
        public void SimpleConstructorWithPortGetSettings()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
                Assert.That(src.PortName, Is.EqualTo(SourcePort));
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
        public void SimpleConstructorWithPortGetSettings2()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                Assert.That(src.PortName, Is.EqualTo(SourcePort));
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
        public void PropertyBaudRate()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
                src.BaudRate = 115200;
                Assert.That(src.BaudRate, Is.EqualTo(115200));
            }
        }

        [Test]
        public void PropertyDataBits()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
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
        public void PropertyParity()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
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
        public void PropertyStopBits()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
                src.StopBits = StopBits.One;
                Assert.That(src.StopBits, Is.EqualTo(StopBits.One));
                src.StopBits = StopBits.Two;
                Assert.That(src.StopBits, Is.EqualTo(StopBits.Two));
                src.StopBits = StopBits.One5;
                Assert.That(src.StopBits, Is.EqualTo(StopBits.One5));
            }
        }

        [Test]
        public void PropertyDiscardNull()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
                src.DiscardNull = false;
                Assert.That(src.DiscardNull, Is.EqualTo(false));
                src.DiscardNull = true;
                Assert.That(src.DiscardNull, Is.EqualTo(true));
            }
        }

        [Test]
        public void PropertyParityReplace()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
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
        public void PropertyTxContinueOnXOff()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
                src.TxContinueOnXOff = true;
                Assert.That(src.TxContinueOnXOff, Is.EqualTo(true));
                src.TxContinueOnXOff = false;
                Assert.That(src.TxContinueOnXOff, Is.EqualTo(false));
            }
        }

        [Test]
        public void PropertyXOffLimit()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
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
        public void PropertyXOnLimit()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort)) {
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
        public void GetPortSettings()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.GetPortSettings();
            }
        }

        [Test]
        public void GetPortSettingsWithNoPort()
        {
            using (SerialPortStream sp = new SerialPortStream()) {
                Assert.That(() => sp.GetPortSettings(), Throws.Exception);
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
        [Timeout(30000)]
        public void ListPorts()
        {
            bool result = true;

            Dictionary<string, bool> ports1 = new Dictionary<string, bool>();
            Dictionary<string, bool> ports2 = new Dictionary<string, bool>();

            PortDescription[] portDescs = SerialPortStream.GetPortDescriptions();
            foreach (PortDescription desc in portDescs) {
                Console.WriteLine("GetPortDescriptions: " + desc.Port + "; Description: " + desc.Description);
                ports1.Add(desc.Port, false);
                ports2.Add(desc.Port, false);
            }

            string[] portNames = SerialPortStream.GetPortNames();
            foreach (string c in portNames) {
                Console.WriteLine("GetPortNames: " + c);
                if (ports1.ContainsKey(c)) {
                    ports1[c] = true;
                } else {
                    Console.WriteLine("GetPortNames() shows " + c + ", but not GetPortDescriptions()");
                    result = false;
                }
            }
            foreach (string c in ports1.Keys) {
                if (!ports1[c]) {
                    Console.WriteLine("GetPortDescriptions() shows " + c + ", but not GetPortnames()");
                    result = false;
                }
            }

            Assert.That(result, Is.True);
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
