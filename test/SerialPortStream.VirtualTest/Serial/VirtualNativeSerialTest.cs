namespace RJCP.IO.Ports.Serial
{
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class VirtualNativeSerialTest
    {
        private static VirtualNativeSerial GetVirtualSerial(bool open)
        {
            VirtualNativeSerial serial = null;
            try {
                serial = new VirtualNativeSerial() {
                    PortName = "COM"
                };

                if (open) {
                    serial.Open();
                    serial.StartMonitor();
                }
                return serial;
            } catch {
                if (serial is not null) {
                    serial.Dispose();
                }
                throw;
            }
        }

        [Test]
        public void Version()
        {
            using (INativeSerial serial = new VirtualNativeSerial()) {
                Assert.That(serial.Version, Is.Not.Null);
            }
        }

        [Test]
        public void DefaultBufferNotSet()
        {
            using (INativeSerial serial = new VirtualNativeSerial()) {
                Assert.That(serial.Buffer.IsBufferAllocated, Is.False);
                Assert.That(serial.Buffer.ReadStream, Is.Null);
                Assert.That(serial.Buffer.ReadChars, Is.Null);
                Assert.That(serial.Buffer.SerialRead, Is.Null);
                Assert.That(serial.Buffer.WriteStream, Is.Null);
                Assert.That(serial.Buffer.SerialWrite, Is.Null);
            }
        }

        [Test]
        public void BufferSetAfterStartMonitor()
        {
            using (INativeSerial serial = GetVirtualSerial(true)) {
                Assert.That(serial.Buffer.IsBufferAllocated, Is.True);
                Assert.That(serial.Buffer.ReadStream, Is.Not.Null);
                Assert.That(serial.Buffer.ReadChars, Is.Not.Null);
                Assert.That(serial.Buffer.SerialRead, Is.Not.Null);
                Assert.That(serial.Buffer.WriteStream, Is.Not.Null);
                Assert.That(serial.Buffer.SerialWrite, Is.Not.Null);
            }
        }

        [Test]
        public void VirtualCarrierDetect()
        {
            using (VirtualNativeSerial serial = GetVirtualSerial(true)) {
                SerialPinChange pin = SerialPinChange.NoChange;
                serial.PinChanged += (s, e) => {
                    pin = e.EventType;
                };

                serial.CDHolding = !serial.CDHolding;
                Assert.That(pin, Is.EqualTo(SerialPinChange.CDChanged));
            }
        }

        [Test]
        public void VirtualRing()
        {
            using (VirtualNativeSerial serial = GetVirtualSerial(true)) {
                SerialPinChange pin = SerialPinChange.NoChange;
                serial.PinChanged += (s, e) => {
                    pin = e.EventType;
                };

                serial.RingHolding = !serial.RingHolding;
                Assert.That(pin, Is.EqualTo(SerialPinChange.Ring));
            }
        }

        [Test]
        public void VirtualClearToSend()
        {
            using (VirtualNativeSerial serial = GetVirtualSerial(true)) {
                SerialPinChange pin = SerialPinChange.NoChange;
                serial.PinChanged += (s, e) => {
                    pin = e.EventType;
                };

                serial.CtsHolding = !serial.CtsHolding;
                Assert.That(pin, Is.EqualTo(SerialPinChange.CtsChanged));
            }
        }

        [Test]
        public void VirtualDataSetReady()
        {
            using (VirtualNativeSerial serial = GetVirtualSerial(true)) {
                SerialPinChange pin = SerialPinChange.NoChange;
                serial.PinChanged += (s, e) => {
                    pin = e.EventType;
                };

                serial.DsrHolding = !serial.DsrHolding;
                Assert.That(pin, Is.EqualTo(SerialPinChange.DsrChanged));
            }
        }

        [Test]
        public void WriteEvent()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                // Your simulated serial port task would wait for when the user writes to the virtual serial port in a loop,
                // and then it can read the data and process it.
                int dataWritten = -1;
                serial.VirtualBuffer.WriteEvent += (s, e) => {
                    dataWritten = e.Bytes;
                };

                stream.PortName = "COM";
                stream.Open();
                Assert.That(dataWritten, Is.EqualTo(-1));

                // The simulated serial port should now be notified.
                stream.Write("Text");
                Assert.That(dataWritten, Is.EqualTo(4));
                Assert.That(serial.VirtualBuffer.SentDataLength, Is.EqualTo(4));
            }
        }

        [Test]
        public void ReadEvent()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                // Your simulated serial port task would wait for when the user writes to the virtual serial port in a loop,
                // and then it can read the data and process it.
                int dataRead = -1;
                serial.VirtualBuffer.ReadEvent += (s, e) => {
                    dataRead = e.Bytes;
                };

                stream.PortName = "COM";
                stream.Open();
                Assert.That(dataRead, Is.EqualTo(-1));

                serial.VirtualBuffer.WriteReceivedData(new byte[] { 0x41, 0x42, 0x43, 0x44 }, 0, 4);
                Assert.That(dataRead, Is.EqualTo(-1));

                // When the user reads the serial port, we should be notified.
                byte[] buffer = new byte[1024];
                int read = stream.Read(buffer, 0, buffer.Length);

                Assert.That(read, Is.EqualTo(4));
                Assert.That(dataRead, Is.EqualTo(4));
                Assert.That(serial.VirtualBuffer.ReceivedDataLength, Is.EqualTo(0));
            }
        }

        [Test]
        public void SerialDataOnRead()
        {
            using (ManualResetEventSlim mre = new(false))
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                byte[] buffer = new byte[1024];
                int read = 0;

                // The SerialPortStream calls this event on the thread pool, it isn't called immediately when data is posted.
                stream.DataReceived += (s, e) => {
                    read = stream.Read(buffer, 0, buffer.Length);
                    mre.Set();
                };

                stream.PortName = "COM";
                stream.Open();

                // When we provide data, the user should also be notified.
                serial.VirtualBuffer.WriteReceivedData(new byte[] { 0x41, 0x42, 0x43, 0x44 }, 0, 4);

                // After writing the data, the callback isn't notified immediately (it's put on a worker in the thread
                // pool). So for this test we have to wait for it.
                Assert.That(mre.Wait(1000), Is.True);
                Assert.That(read, Is.EqualTo(4));
                Assert.That(serial.VirtualBuffer.ReceivedDataLength, Is.EqualTo(0));
            }
        }

        [Test]
        [Repeat(20)]
        [Timeout(2000)]
        public void VirtualNativeClosed()
        {
            using (VirtualNativeSerial serial = new())
            using (SerialPortStream stream = new(serial)) {
                Task serialTask = new TaskFactory().StartNew(() => {
                    Thread.Sleep(100);
                    serial.StopMonitor();
                });

                byte[] buffer = new byte[512];
                stream.ReadTimeout = Timeout.Infinite;
                stream.WriteTimeout = Timeout.Infinite;
                stream.PortName = "COM";
                stream.Open();
                int read = stream.Read(buffer, 0, buffer.Length);

                Assert.That(read, Is.EqualTo(0));

                serialTask.Wait();
                Assert.That(serial.IsRunning, Is.False);

                // Because we stopped the monitor thread, a second read will raise an exception.
                Assert.That(() => {
                    stream.Read(buffer, 0, buffer.Length);
                }, Throws.TypeOf<System.IO.IOException>());
            }
        }
    }
}
