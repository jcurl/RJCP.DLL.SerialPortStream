// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System.Text;

    internal interface IReadChars
    {
        Encoding Encoding { get; set; }

        bool WaitForReadChar(int timeout);

        int Read(char[] buffer, int offset, int count);

        int ReadChar();

        bool ReadTo(string text, out string line);

        string ReadExisting();
    }
}
