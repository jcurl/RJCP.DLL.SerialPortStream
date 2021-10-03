namespace RJCP.IO.Ports
{
    using RJCP.Core.CommandLine;

    internal class CommandOptions
    {
        [Option('p', "port", true)]
        public string Port { get; private set; }

        [Option('b', "baud")]
        public int Baud { get; private set; } = 9600;

        [Option('l', "length")]
        public int Length { get; private set; } = 10240;
    }
}
