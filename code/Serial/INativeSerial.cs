// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System;

    /// <summary>
    /// Interface for accessing serial based streams.
    /// </summary>
    public interface INativeSerial : IDisposable
    {
        /// <summary>
        /// Gets the version of the implementation in use.
        /// </summary>
        /// <value>
        /// The version of the implementation in use.
        /// </value>
        string Version { get; }

        /// <summary>
        /// Gets or sets the port device path.
        /// </summary>
        /// <value>
        /// The port device path.
        /// </value>
        string PortName { get; set; }

        /// <summary>
        /// Gets an array of serial port names.
        /// </summary>
        /// <returns>An array of serial port names.</returns>
        string[] GetPortNames();

        /// <summary>
        /// Gets an array of serial port names and descriptions.
        /// </summary>
        /// <returns>An array of serial ports.</returns>
        /// <remarks>
        /// Get the ports available, and their descriptions, for the current serial port implementation. On Windows,
        /// this uses the Windows Management Interface to obtain its information. Therefore, the list may be different
        /// to the list obtained using the <see cref="GetPortNames"/>.
        /// </remarks>
        PortDescription[] GetPortDescriptions();

        /// <summary>
        /// Gets or sets the baud rate.
        /// </summary>
        /// <value>
        /// The baud rate.
        /// </value>
        int BaudRate { get; set; }

        /// <summary>
        /// Gets or sets the data bits.
        /// </summary>
        /// <value>
        /// The data bits.
        /// </value>
        int DataBits { get; set; }

        /// <summary>
        /// Gets or sets the parity.
        /// </summary>
        /// <value>
        /// The parity.
        /// </value>
        Parity Parity { get; set; }

        /// <summary>
        /// Gets or sets the stop bits.
        /// </summary>
        /// <value>
        /// The stop bits.
        /// </value>
        StopBits StopBits { get; set; }

        /// <summary>
        /// Gets or sets a value if null bytes should be discarded or not.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if null bytes should be discarded; otherwise, <see langword="false"/>.
        /// </value>
        bool DiscardNull { get; set; }

        /// <summary>
        /// Gets or sets the parity replace byte.
        /// </summary>
        /// <value>
        /// The byte to use on parity errors.
        /// </value>
        byte ParityReplace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether transmission should still
        /// be sent when the input buffer is full and if the XOff character has been sent.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if transmission should continue after the input buffer
        ///   is within <see cref="XOffLimit"/> bytes of being full and the driver has sent the
        ///   XOff character; otherwise, <see langword="false"/> that transmission should stop
        ///   and only continue when the input buffer is within <see cref="XOnLimit"/> bytes of
        ///   being empty and the driver has sent the XOn character.
        /// </value>
        bool TxContinueOnXOff { get; set; }

        /// <summary>
        /// Gets or sets the XOff limit input when the XOff character should be sent.
        /// </summary>
        /// <value>
        /// The XOff buffer limit.
        /// </value>
        int XOffLimit { get; set; }

        /// <summary>
        /// Gets or sets the XOn limit when the input buffer is below when the XOn character should be sent.
        /// </summary>
        /// <value>
        /// The XOn buffer limit.
        /// </value>
        int XOnLimit { get; set; }

        /// <summary>
        /// Gets or sets the break state of the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if in the break state; otherwise, <see langword="false"/>.
        /// </value>
        bool BreakState { get; set; }

        /// <summary>
        /// Gets or sets the driver input queue size.
        /// </summary>
        /// <value>
        /// The driver input queue size.
        /// </value>
        /// <remarks>
        /// This method is typically available with Windows API only.
        /// </remarks>
        int DriverInQueue { get; set; }

        /// <summary>
        /// Gets or sets the driver output queue size.
        /// </summary>
        /// <value>
        /// The driver output queue size.
        /// </value>
        /// <remarks>
        /// This method is typically available with Windows API only.
        /// </remarks>
        int DriverOutQueue { get; set; }

        /// <summary>
        /// Gets the number of bytes in the input queue of the driver not yet read (not any managed buffers).
        /// </summary>
        /// <value>
        /// The number of bytes in the driver queue for reading. If this value is not supported, zero is returned.
        /// </value>
        int BytesToRead { get; }

        /// <summary>
        /// Gets the number of bytes in the output buffer of the driver still to write (not any managed buffers).
        /// </summary>
        /// <value>
        /// The number of bytes in the driver queue for writing. If this value is not supported, zero is returned.
        /// </value>
        int BytesToWrite { get; }

        /// <summary>
        /// Gets the state of the Carrier Detect pin on the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if carrier detect pin is active; otherwise, <see langword="false"/>.
        /// </value>
        bool CDHolding { get; }

        /// <summary>
        /// Gets the state of the Clear To Send pin on the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the clear to send pin is active; otherwise, <see langword="false"/>.
        /// </value>
        bool CtsHolding { get; }

        /// <summary>
        /// Gets the state of the Data Set Ready pin on the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if data set ready pin is active; otherwise, <see langword="false"/>.
        /// </value>
        bool DsrHolding { get; }

        /// <summary>
        /// Gets the state of the Ring Indicator pin on the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if ring indicator state is active; otherwise, <see langword="false"/>.
        /// </value>
        bool RingHolding { get; }

        /// <summary>
        /// Gets or sets the Data Terminal Ready pin of the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if data terminal pin is active; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This pin only has an effect if handshaking for DTR/DTS is disabled.
        /// </remarks>
        bool DtrEnable { get; set; }

        /// <summary>
        /// Gets or sets the Request To Send pin of the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if [RTS enable]; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This pin only has an effect if the handshaking for RTS/CTS is disabled.
        /// </remarks>
        bool RtsEnable { get; set; }

        /// <summary>
        /// Gets or sets the handshake to use on the serial port.
        /// </summary>
        /// <value>
        /// The handshake mode to use on the serial port.
        /// </value>
        Handshake Handshake { get; set; }

        /// <summary>
        /// Gets a value indicating whether the serial port has been opened.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if this instance is open; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This property only indicates if the port has been opened and that the
        /// internal handle is valid.
        /// </remarks>
        bool IsOpen { get; }

        /// <summary>
        /// Discards the input queue buffer of the driver.
        /// </summary>
        void DiscardInBuffer();

        /// <summary>
        /// Discards the output queue buffer of the driver.
        /// </summary>
        void DiscardOutBuffer();

        /// <summary>
        /// Gets the port settings and updates the properties of the object.
        /// </summary>
        void GetPortSettings();

        /// <summary>
        /// Writes the settings of the serial port as set in this object.
        /// </summary>
        void SetPortSettings();

        /// <summary>
        /// Opens the serial port specified by <see cref="PortName"/>.
        /// </summary>
        /// <remarks>
        /// Opening the serial port does not set any settings (such as baud rate, etc.)
        /// </remarks>
        void Open();

        /// <summary>
        /// Closes the serial port.
        /// </summary>
        /// <remarks>
        /// Closing the serial port invalidates actions that can be done to the serial port,
        /// but it does not prevent the serial port from being reopened
        /// </remarks>
        void Close();

        /// <summary>
        /// Gets the buffer that is used for reading and writing to the serial port.
        /// </summary>
        /// <value>The buffer used for reading and writing to the serial port.</value>
        /// <remarks>
        /// Buffers should be first allocated when a call to <see cref="StartMonitor"/> is given. The
        /// <see cref="SerialPortStream"/> then uses this buffer after calling <see cref="StartMonitor"/> to read and
        /// write data. It is required that <see cref="SerialBuffer.IsBufferAllocated"/> returns <see langword="true"/>
        /// after <see cref="StartMonitor"/>, i.e. if <see cref="StartMonitor"/> was never called, then
        /// <see cref="SerialBuffer.IsBufferAllocated"/> should be <see langword="false"/>.
        /// </remarks>
        SerialBuffer Buffer { get; }

        /// <summary>
        /// Start the monitor thread, that will watch over the serial port.
        /// </summary>
        /// <remarks>
        /// Starting the monitor thread should call <see cref="SerialBuffer.Reset"/> from the <see cref="Buffer"/>
        /// property, which first allocates buffers for reading and writing.
        /// </remarks>
        void StartMonitor();

        /// <summary>
        /// Gets a value indicating whether the thread for monitoring the serial port is running.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is running; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This property differs slightly from <see cref="IsOpen"/>, as this returns status if
        /// the monitoring thread for reading/writing data is actually running. If the thread is
        /// not running for whatever reason, we can expect no data updates in the buffer provided
        /// to <see cref="StartMonitor()"/>.
        /// </remarks>
        bool IsRunning { get; }

        /// <summary>
        /// Occurs when data is received, or the EOF character is detected by the driver.
        /// </summary>
        event EventHandler<SerialDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Occurs when an error condition is detected.
        /// </summary>
        event EventHandler<SerialErrorReceivedEventArgs> ErrorReceived;

        /// <summary>
        /// Occurs when modem pin changes are detected.
        /// </summary>
        event EventHandler<SerialPinChangedEventArgs> PinChanged;
    }
}
