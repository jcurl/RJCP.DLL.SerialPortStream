namespace RJCP.IO.Ports
{
    using NUnit.Framework;
    using Trace;

    [TestFixture]
    public class SerialPortStreamLoggerTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;

        [Test]
        public void InjectLogger()
        {
            using (SerialPortStream src = new SerialPortStream(new SerialLogger()) {
                PortName = SourcePort,
                BaudRate = 115200,
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One
            }) {
                src.Open();
            }
        }
    }
}
