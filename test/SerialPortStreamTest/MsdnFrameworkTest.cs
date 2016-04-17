namespace RJCP.IO.Ports.SerialPortStreamTest
{
    using System;
    using System.IO.Ports;
    using NUnit.Framework;

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
    }
}
