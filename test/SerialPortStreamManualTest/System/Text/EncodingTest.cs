// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace System.Text
{
    using NUnit.Framework;

    [TestFixture]
    [Explicit(".NET Framework Test")]
    public class EncodingTest
    {
        // NOTE: This test is expected to fail on Windows and Mono.
        [Test]
        public void DecoderTooManyBytes()
        {
            byte[] data = new byte[] { 0x61, 0xE2, 0x82, 0xAC, 0x40, 0x41 };
            char[] oneChar = new char[2];

            Decoder decoder = Encoding.UTF8.GetDecoder();
            decoder.Convert(data, 0, 2, oneChar, 0, 1, false, out int bu, out int cu, out _);

            // One might expect that only one byte is used, but in .NET 4.0 and later (including Mono), we see
            // that while 'cu' is 1, 'bu' is not 1. This had an impact during development that we couldn't optimise
            // our byte to character decoder and expect them to be on MBCS character boundaries.
            Assume.That(bu, Is.EqualTo(1));
            Assume.That(cu, Is.EqualTo(1));
        }
    }
}
