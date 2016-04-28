namespace RJCP.IO.Ports.FrameworkTest
{
    using System;
    using System.IO.Ports;
    using System.Text;
    using System.Threading;
    using NUnit.Framework;
    using SerialPortStreamTest;

    [TestFixture]
    public class MsdnFrameworkTest
    {
        private readonly string c_SourcePort;
        private readonly string c_DestPort;

        public MsdnFrameworkTest()
        {
            c_SourcePort = SerialConfiguration.SourcePort;
            c_DestPort = SerialConfiguration.DestPort;
        }

        [Test]
        public void SerialPortClosedWrite()
        {
            byte[] buffer = new byte[256];

            using (SerialPort serialSource = new SerialPort(c_SourcePort, 115200, Parity.None, 8, StopBits.One)) {
                Assert.That(() => { serialSource.Write(buffer, 0, buffer.Length); }, Throws.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        [Timeout(4000)]
        public void SerialPortDisposedWhenBlocked()
        {
            byte[] buffer = new byte[1024];

            using (SerialPort serialSource = new SerialPort(c_SourcePort, 115200, Parity.None, 8, StopBits.One))
            using (SerialPort serialDest = new SerialPort(c_DestPort, 115200, Parity.None, 8, StopBits.One)) {
                serialSource.Open();
                serialDest.Open();

                serialDest.RtsEnable = false;

                new Thread(
                    () => {
                        Thread.Sleep(2000);
                        Console.WriteLine("Disposing serialSource");

                        // It appears that the MSDN .NET implementation blocks here, never
                        // to return as we're blocked on another thread.
                        serialSource.Dispose();
                        Console.WriteLine("Disposed serialSource");
                    }
                ).Start();

                int bufferCount = 1024 * 1024;
                while (bufferCount > 0) {
                    serialSource.Write(buffer, 0, buffer.Length);
                    bufferCount -= buffer.Length;
                    Console.WriteLine("{0}", bufferCount);
                }
            }
        }

        [Test]
        [Timeout(4000)]
        public void SerialPortClosedWhenBlocked()
        {
            byte[] buffer = new byte[1024];

            using (SerialPort serialSource = new SerialPort(c_SourcePort, 115200, Parity.None, 8, StopBits.One))
            using (SerialPort serialDest = new SerialPort(c_DestPort, 115200, Parity.None, 8, StopBits.One)) {
                serialSource.Open();
                serialDest.Open();

                serialDest.RtsEnable = false;

                new Thread(
                    () => {
                        Thread.Sleep(2000);
                        Console.WriteLine("Closing serialSource");

                        // It appears that the MSDN .NET implementation blocks here, never
                        // to return as we're blocked on another thread.
                        serialSource.Close();
                        Console.WriteLine("Closed serialSource");
                    }
                ).Start();

                int bufferCount = 1024 * 1024;
                while (bufferCount > 0) {
                    serialSource.Write(buffer, 0, buffer.Length);
                    bufferCount -= buffer.Length;
                    Console.WriteLine("{0}", bufferCount);
                }
            }
        }

        [Test]
        public void DecoderTooManyBytes()
        {
            Encoding encoding = Encoding.GetEncoding("UTF-8");
            Decoder decoder = encoding.GetDecoder();

            byte[] data = new byte[] { 0x61, 0xE2, 0x82, 0xAC, 0x40, 0x41 };
            char[] oneChar = new char[2];

            int bu;
            int cu;
            bool complete;
            decoder.Convert(data, 0, 2, oneChar, 0, 1, false, out bu, out cu, out complete);
            Assert.That(bu, Is.EqualTo(1));
            Assert.That(cu, Is.EqualTo(1));
        }
    }
}
