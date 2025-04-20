namespace RJCP.IO.Ports
{
    using System;
    using System.IO;
    using System.Threading;
    using NUnit.Framework;
    using Serial;

    [TestFixture]
    public class SerialPortStreamDriverTest
    {
        [Test]
        public void OpenVirtualIsRunning()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                Assert.That(stream.IsOpen, Is.False);

                stream.PortName = "COM";
                Assert.That(stream.CanRead, Is.True);
                Assert.That(stream.CanWrite, Is.False);

                stream.Open();
                Assert.That(stream.IsOpen, Is.True);
                Assert.That(serial.IsOpen, Is.True);
                Assert.That(serial.IsRunning, Is.True);
                Assert.That(stream.CanRead, Is.True);
                Assert.That(stream.CanWrite, Is.True);

                // Simulate that the serial port driver died after receiving one byte
                serial.VirtualBuffer.WriteReceivedData(new byte[] { 0xAA }, 0, 1);
                serial.StopMonitor();

                Assert.That(stream.IsOpen, Is.False);     // The native serial needs to be open and running
                Assert.That(stream.CanRead, Is.True);
                Assert.That(stream.CanWrite, Is.False);

                Assert.That(serial.IsOpen, Is.True);
                Assert.That(serial.IsRunning, Is.False);

                // Even though the driver has died, we should still be able to read the buffered data
                byte[] inbuff = new byte[1024];
                Assert.That(stream.BytesToRead, Is.EqualTo(1));
                Assert.That(stream.Read(inbuff, 0, inbuff.Length), Is.EqualTo(1));
                Assert.That(inbuff[0], Is.EqualTo(0xAA));

                // A second read, should return zero, indicating that the file is closed
                Assert.That(stream.Read(inbuff, 0, inbuff.Length), Is.EqualTo(0));

                // Now that we've read all data, we should get an exception
                Assert.That(() => {
                    _ = stream.Read(inbuff, 0, inbuff.Length);
                }, Throws.TypeOf<IOException>());
            }
        }

        [Test]
        public void VirtualGetPortSettings()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.BaudRate = 115200;
                stream.GetPortSettings();

                // After getting the port settings, it should be set to defaults
                Assert.That(stream.PortName, Is.EqualTo("COM"));
                Assert.That(stream.BaudRate, Is.EqualTo(9600));
                Assert.That(stream.DataBits, Is.EqualTo(8));
                Assert.That(stream.Parity, Is.EqualTo(Parity.None));
                Assert.That(stream.StopBits, Is.EqualTo(StopBits.One));
                Assert.That(stream.Handshake, Is.EqualTo(Handshake.None));
                Assert.That(stream.DiscardNull, Is.False);
                Assert.That(stream.ParityReplace, Is.EqualTo(0));
                Assert.That(stream.TxContinueOnXOff, Is.False);
                Assert.That(stream.XOffLimit, Is.EqualTo(512));
                Assert.That(stream.XOnLimit, Is.EqualTo(2048));

                Assert.That(stream.IsOpen, Is.False);
                Assert.That(serial.IsOpen, Is.False);
                Assert.That(serial.IsRunning, Is.False);
            }
        }

        [Test]
        public void SerialPortStreamSeek()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                Assert.That(stream.CanSeek, Is.False);
                Assert.That(() => { _ = stream.Length; }, Throws.TypeOf<NotSupportedException>());
                Assert.That(() => { _ = stream.Position; }, Throws.TypeOf<NotSupportedException>());
                Assert.That(() => { stream.Position = 0; }, Throws.TypeOf<NotSupportedException>());
                Assert.That(() => { _ = stream.Seek(0, SeekOrigin.Current); }, Throws.TypeOf<NotSupportedException>());
                Assert.That(() => { stream.SetLength(0); }, Throws.TypeOf<NotSupportedException>());
            }
        }

        [Test]
        public void OnCtsPinChanged()
        {
            using (ManualResetEventSlim mre = new(false))
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                // The event is raised on a worker thread, so it isn't notified immediately, hence the MRE.
                SerialPinChange pin = SerialPinChange.NoChange;
                stream.PinChanged += (s, e) => {
                    pin = e.EventType;
                    mre.Set();
                };

                serial.CtsHolding = true;
                mre.Wait();

                Assert.That(pin, Is.EqualTo(SerialPinChange.CtsChanged));
                Assert.That(stream.CtsHolding, Is.True);
            }
        }

        [Test]
        public void OnDsrPinChanged()
        {
            using (ManualResetEventSlim mre = new(false))
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                // The event is raised on a worker thread, so it isn't notified immediately, hence the MRE.
                SerialPinChange pin = SerialPinChange.NoChange;
                stream.PinChanged += (s, e) => {
                    pin = e.EventType;
                    mre.Set();
                };

                serial.DsrHolding = true;
                mre.Wait();

                Assert.That(pin, Is.EqualTo(SerialPinChange.DsrChanged));
                Assert.That(stream.DsrHolding, Is.True);
            }
        }

        [Test]
        public void OnRingPinChanged()
        {
            using (ManualResetEventSlim mre = new(false))
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                // The event is raised on a worker thread, so it isn't notified immediately, hence the MRE.
                SerialPinChange pin = SerialPinChange.NoChange;
                stream.PinChanged += (s, e) => {
                    pin = e.EventType;
                    mre.Set();
                };

                serial.RingHolding = true;
                mre.Wait();

                Assert.That(pin, Is.EqualTo(SerialPinChange.Ring));
                Assert.That(serial.RingHolding, Is.True);
            }
        }

        [Test]
        public void OnCarrierPinChanged()
        {
            using (ManualResetEventSlim mre = new(false))
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                // The event is raised on a worker thread, so it isn't notified immediately, hence the MRE.
                SerialPinChange pin = SerialPinChange.NoChange;
                stream.PinChanged += (s, e) => {
                    pin = e.EventType;
                    mre.Set();
                };

                serial.CDHolding = true;
                mre.Wait();

                Assert.That(pin, Is.EqualTo(SerialPinChange.CDChanged));
                Assert.That(serial.CDHolding, Is.True);
            }
        }

        [Test]
        public void SetDtrEnable()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                Assert.That(stream.DtrEnable, Is.True);
                stream.DtrEnable = false;
                Assert.That(stream.DtrEnable, Is.False);
            }
        }

        [Test]
        public void SetRtsEnable()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                stream.PortName = "COM";
                stream.Open();

                Assert.That(stream.RtsEnable, Is.True);
                stream.RtsEnable = false;
                Assert.That(stream.RtsEnable, Is.False);
            }
        }
    }
}
