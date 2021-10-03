namespace RJCP.IO.Ports.Native
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
