namespace RJCP.IO.Ports
{
    /// <summary>
    /// An interface for creating a <see cref="SerialPortStream"/>.
    /// </summary>
    public interface ISerialPortStreamFactory
    {
        /// <summary>
        /// Creates a serial port stream with a connection to a port.
        /// </summary>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <returns>A <see cref="SerialPortStream"/> object.</returns>
        SerialPortStream Create(string port);

        /// <summary>
        /// Creates a serial port stream with a connection to a port and a default baud rate.
        /// </summary>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <param name="baud">The baud rate that is passed to the underlying driver.</param>
        /// <returns>A <see cref="SerialPortStream"/> object.</returns>
        SerialPortStream Create(string port, int baud);

        /// <summary>
        /// Creates a serial port stream with a connection to a port and a default baud rate and other settings.
        /// </summary>
        /// <param name="port">The name of the COM port, such as "COM1" or "COM33".</param>
        /// <param name="baud">The baud rate that is passed to the underlying driver.</param>
        /// <param name="data">
        /// The number of data bits. This is checked that the driver supports the data bits provided. The special type
        /// 16X is not supported.
        /// </param>
        /// <param name="parity">The parity for the data stream.</param>
        /// <param name="stopbits">Number of stop bits.</param>
        /// <returns>A <see cref="SerialPortStream"/> object.</returns>
        SerialPortStream Create(string port, int baud, int data, Parity parity, StopBits stopbits);
    }
}
