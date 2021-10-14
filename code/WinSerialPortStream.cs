// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using Serial;

    /// <summary>
    /// A Windows specific Serial Port Stream object that provides configurable settings.
    /// </summary>
    public class WinSerialPortStream : SerialPortStream<IWinNativeSettings>
    {
        /// <summary>
        /// Constructor. Create a stream that connects to the specified port.
        /// </summary>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <remarks>
        /// This constructor attempts to bind directly to the port given. Properties assume the settings of the port
        /// provided. Exceptions may occur if the port cannot be opened.
        /// </remarks>
        public WinSerialPortStream(string port)
            : base(new WinNativeSerial(), port) { }

        /// <summary>
        /// Constructor. Create a stream that connects to the specified port and sets the initial baud rate.
        /// </summary>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <param name="baud">The baud rate that is passed to the underlying driver.</param>
        /// <remarks>
        /// The stream doesn't impose any arbitrary limits on setting the baud rate. It is passed directly to the driver
        /// and it is up to the driver to determine if the baud rate is settable or not. Normally, a driver will attempt
        /// to set a baud rate that is within 5% of the requested baud rate (but not guaranteed).
        /// </remarks>
        public WinSerialPortStream(string port, int baud)
            : base(new WinNativeSerial(), port, baud) { }

        /// <summary>
        /// Constructor. Create a stream that connects to the specified port with standard parameters.
        /// </summary>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <param name="baud">The baud rate that is passed to the underlying driver.</param>
        /// <param name="data">
        /// The number of data bits. This is checked that the driver supports the data bits provided. The special type
        /// 16X is not supported.
        /// </param>
        /// <param name="parity">The parity for the data stream.</param>
        /// <param name="stopbits">Number of stop bits.</param>
        /// <remarks>
        /// The stream doesn't impose any arbitrary limits on setting the baud rate. It is passed directly to the driver
        /// and it is up to the driver to determine if the baud rate is settable or not. Normally, a driver will attempt
        /// to set a baud rate that is within 5% of the requested baud rate (but not guaranteed).
        /// <para>
        /// Not all combinations are supported. The driver will interpret the data and indicate if configuration is
        /// possible or not.
        /// </para>
        /// </remarks>
        public WinSerialPortStream(string port, int baud, int data, Parity parity, StopBits stopbits)
            : base(new WinNativeSerial(), port, baud, data, parity, stopbits) { }
    }
}
