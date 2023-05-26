// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using RJCP.Core.CommandLine;

    public static class Program
    {
        public static int Main(string[] args)
        {
            CommandOptions options = new CommandOptions();
            try {
                Options.Parse(options, args);
            } catch (OptionException ex) {
                Console.WriteLine($"{ex.Message}");
                return 1;
            }

            byte[] buffer = new byte[2048];
            Random r = new Random();
            r.NextBytes(buffer);

            Task monitor = null;
            try {
                Console.WriteLine($"Testing port {options.Port} at Baud {options.Baud}");
                using (SerialPortStream port = new SerialPortStream(options.Port, options.Baud)) {
                    port.Open();

                    // Monitor the number of bytes to write and print when it changes. On Windows, it depends on the
                    // internal buffer and the number of bytes to write provided by the driver.
                    monitor = new TaskFactory().StartNew(() => {
                        int prevBytesToWrite = -1;
                        int bytesToWrite;
                        while (port.IsOpen) {
                            bytesToWrite = port.BytesToWrite;
                            if (bytesToWrite != prevBytesToWrite) {
                                Console.WriteLine($"BytesToWrite = {bytesToWrite}");
                                prevBytesToWrite = bytesToWrite;
                            }
                        }
                    });

                    Thread.Sleep(50);
                    int write = options.Length;
                    int written = 0;
                    while (write > 0) {
                        int writeLength = write > buffer.Length ? buffer.Length : write;
                        written += writeLength;
                        Console.WriteLine($"Writing: {writeLength} (write {written} bytes of {options.Length})");
                        port.Write(buffer, 0, writeLength);
                        write -= writeLength;
                    }
                    Console.WriteLine("Flushing");
                    port.Flush();
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }

            try {
                if (monitor != null)
                    monitor.Wait();
            } catch (ObjectDisposedException) {
                /* Ignore */
            } catch (AggregateException ex) {
                if (ex.InnerExceptions.Count > 0) {
                    if (!(ex.InnerExceptions[0] is ObjectDisposedException)) {
                        throw ex.InnerExceptions[0];
                    }
                }
            }

            return 0;
        }
    }
}
