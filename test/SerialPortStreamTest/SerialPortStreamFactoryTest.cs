﻿namespace RJCP.IO.Ports
{
    using System.Runtime.Versioning;
    using NUnit.Framework;

    [TestFixture]
    public class SerialPortStreamFactoryTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;

        [Test]
        [Platform(Include = "Win32NT")]
        [SupportedOSPlatform("windows")]
        public void InstantiateWinSerialPortStream()
        {
            using (SerialPortStream stream = SerialPortStreamFactory.Factory.Create(SourcePort)) {
                Assert.That(stream, Is.TypeOf<WinSerialPortStream>());
            }
        }

        [Test]
        [Platform(Include = "Linux")]
        [SupportedOSPlatform("linux")]
        public void InstantiateSerialPortStream()
        {
            using (SerialPortStream stream = SerialPortStreamFactory.Factory.Create(SourcePort)) {
                Assert.That(stream, Is.TypeOf<SerialPortStream>());
            }
        }
    }
}
