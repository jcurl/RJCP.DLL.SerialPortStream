// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using System;
    using Serial;

#if NETSTANDARD
    using Microsoft.Extensions.Logging;
#endif

    /// <summary>
    /// A <see cref="SerialPortStream"/> with exposure to driver specific settings.
    /// </summary>
    /// <typeparam name="T">The type that exposes settings for the driver.</typeparam>
    /// <remarks>
    /// This constructor can be used to provide further custom configuration for drivers if they allow it, and is still
    /// compatible with all usage of the <see cref="SerialPortStream"/>.
    /// </remarks>
    public class SerialPortStream<T> : SerialPortStream
    {
        /// <summary>
        /// Constructor. Create a stream that doesn't connect to any port.
        /// </summary>
        /// <param name="serial">The native serial port driver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serial"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Serial object in invalid state - serial</exception>
        /// <remarks>
        /// This constructor initialises a stream object, but doesn't assign it to any COM port. The properties then
        /// assume default settings. No COM port is opened and queried.
        /// </remarks>
        public SerialPortStream(INativeSerial<T> serial)
            : base(serial)
        { }

        /// <summary>
        /// Constructor. Create a stream that connects to the specified port.
        /// </summary>
        /// <param name="serial">The native serial port driver.</param>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <exception cref="ArgumentNullException"><paramref name="serial"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Serial object in invalid state - serial</exception>
        /// <remarks>
        /// This constructor attempts to bind directly to the port given. Properties assume the settings of the port
        /// provided. Exceptions may occur if the port cannot be opened.
        /// </remarks>
        public SerialPortStream(INativeSerial<T> serial, string port)
            : base(serial)
        {
            if (port != null) NativeSerial.PortName = port;
        }

        /// <summary>
        /// Constructor. Create a stream that connects to the specified port and sets the initial baud rate.
        /// </summary>
        /// <param name="serial">The native serial port driver.</param>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <param name="baud">The baud rate that is passed to the underlying driver.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serial"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Serial object in invalid state - serial</exception>
        /// <remarks>
        /// The stream doesn't impose any arbitrary limits on setting the baud rate. It is passed directly to the driver
        /// and it is up to the driver to determine if the baud rate is settable or not. Normally, a driver will attempt
        /// to set a baud rate that is within 5% of the requested baud rate (but not guaranteed).
        /// </remarks>
        public SerialPortStream(INativeSerial<T> serial, string port, int baud)
        {
            if (port != null) NativeSerial.PortName = port;
            NativeSerial.BaudRate = baud;
        }

        /// <summary>
        /// Constructor. Create a stream that connects to the specified port with standard parameters.
        /// </summary>
        /// <param name="serial">The native serial port driver.</param>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <param name="baud">The baud rate that is passed to the underlying driver.</param>
        /// <param name="data">
        /// The number of data bits. This is checked that the driver supports the data bits provided. The special type
        /// 16X is not supported.
        /// </param>
        /// <param name="parity">The parity for the data stream.</param>
        /// <param name="stopbits">Number of stop bits.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serial"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Serial object in invalid state - serial</exception>
        /// <remarks>
        /// The stream doesn't impose any arbitrary limits on setting the baud rate. It is passed directly to the driver
        /// and it is up to the driver to determine if the baud rate is settable or not. Normally, a driver will attempt
        /// to set a baud rate that is within 5% of the requested baud rate (but not guaranteed).
        /// <para>
        /// Not all combinations are supported. The driver will interpret the data and indicate if configuration is
        /// possible or not.
        /// </para>
        /// </remarks>
        public SerialPortStream(INativeSerial<T> serial, string port, int baud, int data, Parity parity, StopBits stopbits)
        {
            if (port != null) NativeSerial.PortName = port;
            NativeSerial.BaudRate = baud;
            NativeSerial.DataBits = data;
            NativeSerial.Parity = parity;
            NativeSerial.StopBits = stopbits;
        }

#if NETSTANDARD
        /// <summary>
        /// Constructor. Create a stream that doesn't connect to any port. Specify the logger.
        /// </summary>
        /// <param name="logger">The logger instance to log to.</param>
        /// <param name="serial">The native serial implementation to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serial"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This constructor initialises a stream object, but doesn't assign it to any COM port. The properties then
        /// assume default settings. No COM port is opened and queried.
        /// <para>
        /// This method allows to inject a logging instance to help track down and debug communication on the serial
        /// port. Many defects and issues can be traced back to behaviours of various drivers.
        /// </para>
        /// </remarks>
        [CLSCompliant(false)]
        public SerialPortStream(ILogger logger, INativeSerial<T> serial)
            : base(logger, serial) { }
#endif

        /// <summary>
        /// Gets the settings for the driver underlying the serial port stream.
        /// </summary>
        /// <value>The settings for the driver underlying the serial port stream.</value>
        public T Settings { get { return ((INativeSerial<T>)NativeSerial).Settings; } }
    }
}
