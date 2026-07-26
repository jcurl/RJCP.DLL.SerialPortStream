using System;
using System.Linq;
using RJCP.IO.Ports;

string sourcePort = args.Length > 0 ? args[0] : "CNCA0";
string destinationPort = args.Length > 1 ? args[1] : "CNCB0";

await using (SerialPortStream serial = new()) {
    if (!Version.TryParse(serial.Version, out Version version))
        throw new InvalidOperationException($"Invalid native version '{serial.Version}'.");

    string[] ports = serial.GetPortNames();
    Console.WriteLine($"SerialPortStream {version}; ports: {ports.Length}");
}

byte[] sent = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();

await using SerialPortStream source = new(sourcePort, 115200, 8, Parity.None, StopBits.One);
await using SerialPortStream destination = new(destinationPort, 115200, 8, Parity.None, StopBits.One);

destination.Open();
source.Open();
source.Write(sent, 0, sent.Length);

byte[] received = new byte[sent.Length];
await destination.ReadExactlyAsync(received);

for (int i = 0; i < sent.Length; i++) {
    if (sent[i] != received[i])
        throw new InvalidOperationException(
            $"Loopback data differs at byte {i}: sent {sent[i]:X2}, received {received[i]:X2}.");
}

Console.WriteLine($"AOT loopback passed: {sourcePort} -> {destinationPort} ({sent.Length} bytes)");