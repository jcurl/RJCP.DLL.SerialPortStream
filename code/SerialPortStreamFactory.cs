namespace RJCP.IO.Ports
{
    using System;
    using RJCP.Core.Environment;
    using Serial;

    /// <summary>
    /// A Factory for <see cref="SerialPortStream"/> objects, based on the runtime environment.
    /// </summary>
    /// <remarks>
    /// Provides a <see cref="SerialPortStream"/> object, that exposes settings
    /// </remarks>
    public class SerialPortStreamFactory : ISerialPortStreamFactory
    {
        private static ISerialPortStreamFactory s_Factory;
        private static readonly object s_FactoryLock = new object();

        /// <summary>
        /// Gets or sets the factory to get a <see cref="SerialPortStream"/> object.
        /// </summary>
        /// <value>The factory for getting a <see cref="SerialPortStream"/> object.</value>
        /// <exception cref="ArgumentNullException">Setting this property was <see langword="null"/>.</exception>
        /// <remarks>
        /// Use this factory to get a <see cref="SerialPortStream"/> object. By default, it creates an object dependent
        /// on the operating system. While the <see cref="SerialPortStream"/> constructor automatically detects the
        /// operating system, using the factory (and then typecasting to the correct object) provides the correct object
        /// but with a <see cref="INativeSerial{T}.Settings"/> property.
        /// <para>
        /// Your application can set this property for changing the factory implementation. This can be useful for test
        /// applications, where you wish to return a SerialPortStream object for your own testing (e.g. something based
        /// on a virtual serial driver, or something different.
        /// </para>
        /// </remarks>
        public static ISerialPortStreamFactory Factory
        {
            get
            {
                if (s_Factory == null) {
                    lock (s_FactoryLock) {
                        if (s_Factory == null) {
                            s_Factory = new SerialPortStreamFactory();
                        }
                    }
                }
                return s_Factory;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Factory));

                lock (s_FactoryLock) {
                    s_Factory = value;
                }
            }
        }

        /// <summary>
        /// Creates a serial port stream with a connection to a port.
        /// </summary>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <returns>A <see cref="SerialPortStream" /> object.</returns>
        /// <remarks>
        /// On Windows, it returns an object of type <see cref="WinSerialPortStream"/>.
        /// </remarks>
        public SerialPortStream Create(string port)
        {
            if (Platform.IsWinNT()) {
                return new WinSerialPortStream(port);
            }
            return new SerialPortStream(port);
        }

        /// <summary>
        /// Creates a serial port stream with a connection to a port and a default baud rate.
        /// </summary>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <param name="baud">The baud rate that is passed to the underlying driver.</param>
        /// <returns>A <see cref="SerialPortStream" /> object.</returns>
        /// <remarks>
        /// On Windows, it returns an object of type <see cref="WinSerialPortStream"/>.
        /// </remarks>
        public SerialPortStream Create(string port, int baud)
        {
            if (Platform.IsWinNT()) {
                return new WinSerialPortStream(port, baud);
            }
            return new SerialPortStream(port, baud);
        }

        /// <summary>
        /// Creates a serial port stream with a connection to a port and a default baud rate and other settings.
        /// </summary>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <param name="baud">The baud rate that is passed to the underlying driver.</param>
        /// <param name="data">The number of data bits. This is checked that the driver supports the data bits provided. The special type
        /// 16X is not supported.</param>
        /// <param name="parity">The parity for the data stream.</param>
        /// <param name="stopbits">Number of stop bits.</param>
        /// <returns>A <see cref="SerialPortStream" /> object.</returns>
        /// <remarks>
        /// On Windows, it returns an object of type <see cref="WinSerialPortStream"/>.
        /// </remarks>
        public SerialPortStream Create(string port, int baud, int data, Parity parity, StopBits stopbits)
        {
            if (Platform.IsWinNT()) {
                return new WinSerialPortStream(port, baud, data, parity, stopbits);
            }
            return new SerialPortStream(port, baud, data, parity, stopbits);
        }
    }
}
