namespace System.IO.Ports
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using RJCP.IO.Ports;

    [TestFixture]
    [Category("ManualTest")]
    [Explicit("Operating System Framework Test")]
    public class SerialPortTest
    {
        // Be sure to set up the COM port properly in the app.config file, els you'll get an exception with an
        // invalid port name.
        private readonly string SourcePort = SerialConfiguration.SourcePort;
        private readonly string DestPort = SerialConfiguration.DestPort;

        [Test]
        public void SerialPortClosedWrite()
        {
            byte[] buffer = new byte[256];

            // Be sure to set up the COM port properly in the app.config file, els you'll get an exception with an
            // invalid port name.
            using (SerialPort serialSource = new SerialPort(SourcePort, 115200, Parity.None, 8, StopBits.One)) {
                Assume.That(() => { serialSource.Write(buffer, 0, buffer.Length); }, Throws.TypeOf<InvalidOperationException>());
            }
        }

        // NOTE: This test is expected to fail or block forever on Windows.
        [Test]
        [Timeout(4000)]
        public void SerialPortDisposedWhenBlocked()
        {
            byte[] buffer = new byte[1024];

            // Be sure to set up the COM port properly in the app.config file, els you'll get an exception with an
            // invalid port name.
            using (SerialPort serialSource = new SerialPort(SourcePort, 115200, Parity.None, 8, StopBits.One))
            using (SerialPort serialDest = new SerialPort(DestPort, 115200, Parity.None, 8, StopBits.One)) {
                serialSource.Open();
                serialDest.Open();

                serialDest.RtsEnable = false;

                Task serial = new TaskFactory().StartNew(() => {
                    Thread.Sleep(2000);
                    Console.WriteLine("Disposing serialSource");

                    // It appears that the MSDN .NET implementation blocks here, never
                    // to return as we're blocked on another thread.
                    serialSource.Dispose();
                    Console.WriteLine("Disposed serialSource");
                });

                int bufferCount = 1024 * 1024;
                while (bufferCount > 0) {
                    serialSource.Write(buffer, 0, buffer.Length);
                    bufferCount -= buffer.Length;
                    Console.WriteLine($"{bufferCount}");
                }

                serial.Wait();
            }
        }

        // NOTE: This test is expected to fail or block forever on Windows.
        [Test]
        [Timeout(4000)]
        public void SerialPortClosedWhenBlocked()
        {
            byte[] buffer = new byte[1024];

            // Be sure to set up the COM port properly in the app.config file, els you'll get an exception with an
            // invalid port name.
            using (SerialPort serialSource = new SerialPort(SourcePort, 115200, Parity.None, 8, StopBits.One))
            using (SerialPort serialDest = new SerialPort(DestPort, 115200, Parity.None, 8, StopBits.One)) {
                serialSource.Open();
                serialDest.Open();

                serialDest.RtsEnable = false;

                Task serial = new TaskFactory().StartNew(() => {
                    Thread.Sleep(2000);
                    Console.WriteLine("Closing serialSource");

                    // It appears that the MSDN .NET implementation blocks here, never
                    // to return as we're blocked on another thread.
                    serialSource.Close();
                    Console.WriteLine("Closed serialSource");
                });

                int bufferCount = 1024 * 1024;
                while (bufferCount > 0) {
                    serialSource.Write(buffer, 0, buffer.Length);
                    bufferCount -= buffer.Length;
                    Console.WriteLine($"{bufferCount}");
                }

                serial.Wait();
            }
        }
    }
}
