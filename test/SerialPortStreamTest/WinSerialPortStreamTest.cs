namespace RJCP.IO.Ports
{
    using System.Runtime.Versioning;
    using NUnit.Framework;

    [TestFixture]
    [Platform(Include = "Win")]
    [SupportedOSPlatform("windows")]
    public class WinSerialPortStreamTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;

        [Test]
        public void DefaultCommTimeOuts()
        {
            using (WinSerialPortStream stream = new(SourcePort)) {
                Assert.That(stream.Settings.ReadIntervalTimeout, Is.EqualTo(10));
                Assert.That(stream.Settings.ReadTotalTimeoutConstant, Is.EqualTo(100));
                Assert.That(stream.Settings.ReadTotalTimeoutMultiplier, Is.EqualTo(0));
                Assert.That(stream.Settings.WriteTotalTimeoutConstant, Is.EqualTo(500));
                Assert.That(stream.Settings.WriteTotalTimeoutMultiplier, Is.EqualTo(0));
            }
        }
    }
}
