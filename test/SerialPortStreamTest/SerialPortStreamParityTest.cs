// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using System;
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
    public class SerialPortStreamParityTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;
        private readonly string DestPort = SerialConfiguration.DestPort;
        private const int TimeOut = 300;

#if NETCOREAPP3_1
        [SetUp]
        public void InitLogging()
        {
            Trace.GlobalLogger.Initialize();
        }
#endif

        [Test]
        public void SettingsOnOpenParity()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.Odd, StopBits.One)) {
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

            // NOTE: This test case will likely fail on software loopback devices, as they handle bytes and not
            // bits as a real UART does
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

            // NOTE: This test case will likely fail on software loopback devices, as they handle bytes and not
            // bits as a real UART does
            Assert.That(offset, Is.EqualTo(16), "Expected 16 bytes received, but only got {0} bytes", offset);
            byte[] expectedrecv = new byte[] { 0x00, 0x81, 0x82, 0x03, 0x84, 0x05, 0x06, 0x87, 0x88, 0x09, 0x0A, 0x8B, 0x0C, 0x8D, 0x8E, 0x0F };
            for (int i = 0; i < offset; i++) {
                Assert.That(recv[i], Is.EqualTo(expectedrecv[i]), "Offset {0} got {1}; expected {2}", i, recv[i], expectedrecv[i]);
            }
        }

        [Test]
        public void OddParityLoopback()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 7, Parity.Odd, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.Open(); dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;

                TestOddParity(src, dst);
            }
        }

        [Test]
        public void EvenParityLoopback()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 7, Parity.Even, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.Open(); dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;

                TestEvenParity(src, dst);
            }
        }

        [Test]
        public void ParityChangeLoopback()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 7, Parity.Even, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.Open(); dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
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
    }
}
