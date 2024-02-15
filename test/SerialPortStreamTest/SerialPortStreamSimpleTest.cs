namespace RJCP.IO.Ports
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    using System.Threading;
    using NUnit.Framework;
    using Serial;

    [TestFixture]
    public class SerialPortStreamSimpleTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;

#if NET6_0_OR_GREATER
        [SetUp]
        public void InitLogging()
        {
            Trace.GlobalLogger.Initialize();
        }
#endif

        [Test]
        public void SimpleConstructor()
        {
            SerialPortStream src = new();
            src.Dispose();
            Assert.That(src.IsDisposed, Is.True);
        }

        [Test]
        public void VersionString()
        {
            using (SerialPortStream src = new()) {
                Assert.That(src.Version, Is.Not.Null.Or.Empty);
                Console.WriteLine($"Version: {src.Version}");
            }
        }

        [Test]
        public void ConstructorWithPort()
        {
            using (SerialPortStream src = new(SourcePort)) {
                Assert.That(src.PortName, Is.EqualTo(SourcePort));
            }
        }

        [Test]
        public void ConstructorWithPortBaud()
        {
            using (SerialPortStream src = new(SourcePort, 1200)) {
                Assert.That(src.PortName, Is.EqualTo(SourcePort));
                Assert.That(src.BaudRate, Is.EqualTo(1200));
            }
        }

        [Test]
        public void ConstructorWithFullConfig()
        {
            using (SerialPortStream src = new(SourcePort, 1200, 8, Parity.None, StopBits.One)) {
                Assert.That(src.PortName, Is.EqualTo(SourcePort));
                Assert.That(src.BaudRate, Is.EqualTo(1200));
                Assert.That(src.DataBits, Is.EqualTo(8));
                Assert.That(src.Parity, Is.EqualTo(Parity.None));
                Assert.That(src.StopBits, Is.EqualTo(StopBits.One));
            }
        }

        [Test]
        public void ConstructorNullNativeSerial()
        {
            Assert.That(() => {
                _ = new SerialPortStream((INativeSerial)null);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ConstructorNativeSerialOpen()
        {
            using (VirtualNativeSerial serial = new()) {
                serial.PortName = "COM";
                serial.Open();

                Assert.That(() => {
                    _ = new SerialPortStream(serial);
                }, Throws.TypeOf<ArgumentException>());
            }
        }

        [Test]
        public void ConstructorNativeSerialRunning()
        {
            using (VirtualNativeSerial serial = new()) {
                serial.PortName = "COM";
                serial.Open();
                serial.StartMonitor();

                Assert.That(() => {
                    _ = new SerialPortStream(serial);
                }, Throws.TypeOf<ArgumentException>());
            }
        }

        [Test]
        public void PropertyBaudRate()
        {
            using (SerialPortStream src = new(SourcePort)) {
                src.BaudRate = 115200;
                Assert.That(src.BaudRate, Is.EqualTo(115200));
            }
        }

        [Test]
        public void PropertyDataBits()
        {
            using (SerialPortStream src = new(SourcePort)) {
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
            using (SerialPortStream src = new(SourcePort)) {
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
            using (SerialPortStream src = new(SourcePort)) {
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
            using (SerialPortStream src = new(SourcePort)) {
                src.DiscardNull = false;
                Assert.That(src.DiscardNull, Is.EqualTo(false));
                src.DiscardNull = true;
                Assert.That(src.DiscardNull, Is.EqualTo(true));
            }
        }

        [Test]
        public void PropertyParityReplace()
        {
            using (SerialPortStream src = new(SourcePort)) {
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
            using (SerialPortStream src = new(SourcePort)) {
                src.TxContinueOnXOff = true;
                Assert.That(src.TxContinueOnXOff, Is.EqualTo(true));
                src.TxContinueOnXOff = false;
                Assert.That(src.TxContinueOnXOff, Is.EqualTo(false));
            }
        }

        [Test]
        public void PropertyXOffLimit()
        {
            using (SerialPortStream src = new(SourcePort)) {
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
            using (SerialPortStream src = new(SourcePort)) {
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
        public void GetPortSettingsWithNoPort()
        {
            using (SerialPortStream sp = new()) {
                Assert.That(() => sp.GetPortSettings(), Throws.Exception);
            }
        }

        [Test]
        [Timeout(30000)]
        public void ListPorts()
        {
            bool result = true;

            Dictionary<string, bool> ports1 = new();
            Dictionary<string, bool> ports2 = new();

            using (SerialPortStream serialPort = new()) {
                PortDescription[] portDescs = serialPort.GetPortDescriptions();
                foreach (PortDescription desc in portDescs) {
                    Console.WriteLine($"GetPortDescriptions: {desc}");
                    ports1.Add(desc.Port, false);
                    ports2.Add(desc.Port, false);
                }

                string[] portNames = serialPort.GetPortNames();
                foreach (string c in portNames) {
                    Console.WriteLine($"GetPortNames: {c}");
                    if (ports1.ContainsKey(c)) {
                        ports1[c] = true;
                    } else {
                        Console.WriteLine($"GetPortNames() shows {c}, but not GetPortDescriptions()");
                        result = false;
                    }
                }
                foreach (string c in ports1.Keys) {
                    if (!ports1[c]) {
                        Console.WriteLine($"GetPortDescriptions() shows {c}, but not GetPortNames()");
                        result = false;
                    }
                }

                Assert.That(result, Is.True);
            }
        }

        [Test]
        [Platform(Include = "Win")]
        [SupportedOSPlatform("windows")]
        public void SerialPortStreamNativeSerialWindows()
        {
            INativeSerial serial = new WinNativeSerial();
            Assert.That(serial.IsOpen, Is.False);

            using (SerialPortStream winSerial = new(serial)) {
                Assert.That(winSerial.IsOpen, Is.False);
            }
        }

        [Test]
        [Platform(Include = "Linux")]
        [SupportedOSPlatform("linux")]
        public void SerialPortStreamNativeSerialLinux()
        {
            INativeSerial serial = new UnixNativeSerial();
            Assert.That(serial.IsOpen, Is.False);

            using (SerialPortStream unixSerial = new(serial)) {
                Assert.That(unixSerial.IsOpen, Is.False);
            }
        }
    }
}
