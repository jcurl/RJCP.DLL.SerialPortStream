// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.MonoBugs
{
    using System;
    using System.Text;
    using NUnit.Framework;

    [TestFixture]
    public class SystemText
    {
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
    