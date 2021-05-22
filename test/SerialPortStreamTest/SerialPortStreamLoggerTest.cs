// Copyright © Jason Curl 2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.SerialPortStreamTest
{
    using System;
    using System.Threading;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;
    using Trace;

    [TestFixture(Category = "SerialPortStream")]
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