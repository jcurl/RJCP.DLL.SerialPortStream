// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

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
            using (WinSerialPortStream stream = new WinSerialPortStream(SourcePort)) {
                Assert.That(stream.Settings.ReadIntervalTimeout, Is.EqualTo(10));
                Assert.That(stream.Settings.ReadTotalTimeoutConstant, Is.EqualTo(100));
                Assert.That(stream.Settings.ReadTotalTimeoutMultiplier, Is.EqualTo(0));
                Assert.That(stream.Settings.WriteTotalTimeoutConstant, Is.EqualTo(500));
                Assert.That(stream.Settings.WriteTotalTimeoutMultiplier, Is.EqualTo(0));
            }
        }
    }
}
