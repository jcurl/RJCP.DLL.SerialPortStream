namespace RJCP.IO.Ports
{
    using System;
    using System.Threading;
    using NUnit.Framework;

    internal sealed class SerialPortReceive : IDisposable
    {
        /// <summary>
        /// Initialize a port to receive data and discard it.
        /// </summary>
        /// <param name="port">The port to receive on.</param>
        /// <param name="settings">The settings to use, based on another port.</param>
        /// <returns>
        /// A <see cref="SerialPortStream"/> that is automatically opened, set to receive and should be disposed.
        /// </returns>
        /// <remarks>
        /// This method is useful for Com0Com testing. Normal serial ports are single devices that send data and then
        /// don't care if something is there receiving it or not. Com0Com however is a virtual pair, and when data is
        /// sent on one port, the other port is expected to receive it. If it doesn't, the first write works as
        /// expected, but subsequent writes timeout, until the other end reads it.
        /// </remarks>
        public static SerialPortReceive IdleReceive(string port, SerialPortStream settings)
        {
            return new SerialPortReceive(port, settings);
        }

        private readonly SerialPortStream m_RxPort;
        private readonly Thread m_Receiver;

        private SerialPortReceive(string port, SerialPortStream settings)
        {
            string testcase = TestContext.CurrentContext.Test.Name;

            Console.WriteLine($"Starting receiver on {port} for test case {testcase}");

            try {
                m_RxPort = new SerialPortStream(port) {
                    BaudRate = settings.BaudRate,
                    DataBits = settings.DataBits,
                    Parity = settings.Parity,
                    DtrEnable = settings.DtrEnable,
                    StopBits = settings.StopBits,
                    ReadTimeout = -1,
                    WriteTimeout = -1
                };
                m_RxPort.Open();

                // The receiver will run, until the serial port is disposed.
                m_Receiver = new Thread(() => {
                    char[] buffer = new char[256];
                    int r = 0;
                    do {
                        try {
                            r = m_RxPort.Read(buffer, 0, buffer.Length);
                        } catch (Exception ex) {
                            Console.WriteLine($"Thread receiver closed on {port} for test case {testcase} due to {ex.Message}");
                            r = 0;
                        }
                    } while (r > 0);
                    Console.WriteLine($"Thread receiver closed on {port} for test case {testcase}");
                });
                m_Receiver.Start();
            } catch {
                if (m_RxPort is not null) m_RxPort.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool m_IsDisposed;

        private void Dispose(bool disposing)
        {
            if (disposing && !m_IsDisposed) {
                m_RxPort.Dispose();
                m_Receiver.Join(5000);
                m_IsDisposed = true;
            }
        }
    }
}
