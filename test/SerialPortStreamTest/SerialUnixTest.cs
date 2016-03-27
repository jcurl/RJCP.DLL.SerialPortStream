// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.SerialPortStreamTest
{
    using System;
    using NUnit.Framework;
    using Native.Unix;

    [TestFixture]
    public class SerialUnixTest
    {
        [Test]
        public void GetVersion()
        {
            SerialUnix sunix = new SerialUnix();
            Console.WriteLine("Version: {0}", sunix.serial_version());
        }

        [Test]
        public void InitAndTerminate()
        {
            SerialUnix sunix = new SerialUnix();
            IntPtr handle = sunix.serial_init();
            Assert.That(handle, Is.Not.EqualTo (IntPtr.Zero));
            sunix.serial_terminate(handle);
        }

        [Test]
        public void SetAndGetDeviceName()
        {
            SerialUnix sunix = new SerialUnix();
            IntPtr handle = sunix.serial_init();
            Assert.That(handle, Is.Not.EqualTo (IntPtr.Zero));
            Assert.That(sunix.serial_setdevicename(handle, "/dev/ttyS0"), Is.Not.EqualTo(-1));
            Assert.That(sunix.serial_getdevicename(handle), Is.EqualTo("/dev/ttyS0"));
            sunix.serial_terminate(handle);
        }
    }
}

