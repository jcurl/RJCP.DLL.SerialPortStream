namespace RJCP.IO.Ports
{
    using System.Runtime.Versioning;
    using NUnit.Framework;

    [TestFixture]
    [Platform(Include = "Win32NT")]
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
                Assert.That(stream.Settings.WriteTotalTimeoutConstant, Is.EqualTo(0));
                Assert.That(stream.Settings.WriteTotalTimeoutMultiplier, Is.EqualTo(0));
            }
        }

        [Test]
        public void SetSynchronousTimeouts()
        {
            using (WinSerialPortStream stream = new(SourcePort)) {
                stream.Settings.ReadIntervalTimeout = System.Threading.Timeout.Infinite;
                stream.Settings.ReadTotalTimeoutConstant = 0;
                stream.Settings.ReadTotalTimeoutMultiplier = 0;

                Assert.That(stream.Settings.ReadIntervalTimeout, Is.EqualTo(-1));
                Assert.That(stream.Settings.ReadTotalTimeoutConstant, Is.EqualTo(0));
                Assert.That(stream.Settings.ReadTotalTimeoutMultiplier, Is.EqualTo(0));
            }
        }
    }
}
