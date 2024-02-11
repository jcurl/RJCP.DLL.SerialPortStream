namespace RJCP.IO.Ports
{
    using Diagnostics.Trace;

    internal static class Log
    {
        public const string SerialPortStream = "RJCP.IO.Ports.SerialPortStream";

        public static readonly LogSource Serial = new LogSource(SerialPortStream);
    }
}
