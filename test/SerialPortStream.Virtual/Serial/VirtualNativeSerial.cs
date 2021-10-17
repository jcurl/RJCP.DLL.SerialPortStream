// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System;
    using System.Reflection;

    /// <summary>
    /// The <see cref="VirtualNativeSerial"/> exposes buffers to simulate a serial port.
    /// </summary>
    public class VirtualNativeSerial : INativeSerial
    {
        private readonly VirtualSerialBuffer m_Buffer;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualNativeSerial"/> class.
        /// </summary>
        public VirtualNativeSerial()
        {
            m_Buffer = new VirtualSerialBuffer();
            m_Buffer.DataReceived += OnDataReceived;
        }

        private string m_Version;

        /// <summary>
        /// Gets the version of the implementation in use.
        /// </summary>
        /// <value>The version of the implementation in use.</value>
        public virtual string Version
        {
            get
            {
                if (m_Version != null) return m_Version;

#if NETSTANDARD
                var assembly = typeof(VirtualNativeSerial).GetTypeInfo().Assembly;
#else
                Assembly assembly = Assembly.GetExecutingAssembly();
#endif
                AssemblyInformationalVersionAttribute v =
                    Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute), false)
                    as AssemblyInformationalVersionAttribute;
                m_Version = v.InformationalVersion;
                return m_Version;
            }
        }

        private string m_PortName;

        /// <summary>
        /// Gets or sets the port device path.
        /// </summary>
        /// <value>The port device path.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException">Port already open.</exception>
        public virtual string PortName
        {
            get { return m_PortName; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(VirtualNativeSerial));
                if (IsOpen) throw new InvalidOperationException("Port already open");
                m_PortName = value;
            }
        }

        /// <summary>
        /// Gets an array of serial port names.
        /// </summary>
        /// <returns>An array of serial port names.</returns>
        public virtual string[] GetPortNames()
        {
#if NETFRAMEWORK
            return new string[0];
#else
            return Array.Empty<string>();
#endif
        }

        /// <summary>
        /// Gets an array of serial port names and descriptions.
        /// </summary>
        /// <remarks>
        /// Get the ports available, and their descriptions, for the current serial port implementation. On Windows,
        /// this uses the Windows Management Interface to obtain its information. Therefore, the list may be different
        /// to the list obtained using the <see cref="GetPortNames"/>.
        /// </remarks>
        /// <returns>An array of serial ports.</returns>
        public virtual PortDescription[] GetPortDescriptions()
        {
#if NETFRAMEWORK
            return new PortDescription[0];
#else
            return Array.Empty<PortDescription>();
#endif
        }

        private int m_Baud = 115200;

        /// <summary>
        /// Gets or sets the baud rate.
        /// </summary>
        /// <value>The baud rate.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentOutOfRangeException">Baud rate must be positive.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual int BaudRate
        {
            get { return m_Baud; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(VirtualNativeSerial));
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(BaudRate), "Baud rate must be positive");
                m_Baud = value;
            }
        }

        private int m_DataBits = 8;

        /// <summary>
        /// Gets or sets the data bits.
        /// </summary>
        /// <value>The data bits.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentOutOfRangeException">May only be 5, 6, 7, 8 or 16.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual int DataBits
        {
            get { return m_DataBits; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(VirtualNativeSerial));
                if ((value < 5 || value > 8) && value != 16) {
                    throw new ArgumentOutOfRangeException(nameof(DataBits), "May only be 5, 6, 7, 8 or 16");
                }
                m_DataBits = value;
            }
        }

        private Parity m_Parity = Parity.None;

        /// <summary>
        /// Gets or sets the parity.
        /// </summary>
        /// <value>The parity.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentOutOfRangeException">Unknown value for Parity.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual Parity Parity
        {
            get { return m_Parity; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(VirtualNativeSerial));
                if (!Enum.IsDefined(typeof(Parity), value)) {
                    throw new ArgumentOutOfRangeException(nameof(Parity), "Unknown value for Parity");
                }
                m_Parity = value;
            }
        }

        private StopBits m_StopBits = StopBits.One;

        /// <summary>
        /// Gets or sets the stop bits.
        /// </summary>
        /// <value>The stop bits.</value>
        /// <exception cref="ArgumentOutOfRangeException">Unknown value for Stop Bits.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual StopBits StopBits
        {
            get { return m_StopBits; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(VirtualNativeSerial));
                if (!Enum.IsDefined(typeof(StopBits), value)) {
                    throw new ArgumentOutOfRangeException(nameof(StopBits), "Unknown value for Stop Bits");
                }
                m_StopBits = value;
            }
        }

        private bool m_DiscardNull;

        /// <summary>
        /// Gets or sets a value if null bytes should be discarded or not.
        /// </summary>
        /// <value><see langword="true"/> if null bytes should be discarded; otherwise, <see langword="false"/>.</value>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual bool DiscardNull
        {
            get { return m_DiscardNull; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(VirtualNativeSerial));
                m_DiscardNull = value;
            }
        }

        private byte m_ParityReplace;

        /// <summary>
        /// Gets or sets the parity replace byte.
        /// </summary>
        /// <value>The byte to use on parity errors.</value>
        /// <exception cref="ArgumentOutOfRangeException">Must be a byte value from 0 to 255.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual byte ParityReplace
        {
            get { return m_ParityReplace; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                m_ParityReplace = value;
            }
        }

        private bool m_TxContinueOnXOff;

        /// <summary>
        /// Gets or sets a value indicating whether transmission should still be sent when the input buffer is full and
        /// if the XOff character has been sent.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if transmission should continue after the input buffer is within
        /// <see cref="XOffLimit"/> bytes of being full and the driver has sent the XOff character; otherwise,
        /// <see langword="false"/> that transmission should stop and only continue when the input buffer is within
        /// <see cref="XOnLimit"/> bytes of being empty and the driver has sent the XOn character.
        /// </value>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual bool TxContinueOnXOff
        {
            get { return m_TxContinueOnXOff; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                m_TxContinueOnXOff = value;
            }
        }

        private int m_XOffLimit = 512;

        /// <summary>
        /// Gets or sets the XOff limit input when the XOff character should be sent.
        /// </summary>
        /// <value>The XOff buffer limit.</value>
        /// <exception cref="ArgumentOutOfRangeException">XOffLimit must be positive.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual int XOffLimit
        {
            get { return m_XOffLimit; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (value < 0) {
                    throw new ArgumentOutOfRangeException(nameof(XOffLimit), "XOffLimit must be positive");
                }
                m_XOffLimit = value;
            }
        }

        private int m_XOnLimit = 2048;

        /// <summary>
        /// Gets or sets the XOn limit when the input buffer is below when the XOn character should be sent.
        /// </summary>
        /// <value>The XOn buffer limit.</value>
        /// <exception cref="ArgumentOutOfRangeException">XOffLimit must be positive.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual int XOnLimit
        {
            get { return m_XOnLimit; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (value < 0) {
                    throw new ArgumentOutOfRangeException(nameof(XOnLimit), "XOffLimit must be positive");
                }
                m_XOnLimit = value;
            }
        }

        private bool m_BreakState;

        /// <summary>
        /// Gets or sets the break state of the serial port.
        /// </summary>
        /// <value><see langword="true"/> if in the break state; otherwise, <see langword="false"/>.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException">Port not open.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual bool BreakState
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (!IsOpen) return false;
                return m_BreakState;
            }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (!IsOpen) throw new InvalidOperationException("Port not open");
                m_BreakState = value;
            }
        }

        private int m_DriverInQueue = 4096;

        /// <summary>
        /// Gets or sets the driver input queue size.
        /// </summary>
        /// <value>The driver input queue size.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentOutOfRangeException">value must be a positive integer.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual int DriverInQueue
        {
            get { return m_DriverInQueue; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(DriverInQueue), "value must be a positive integer");
                m_DriverInQueue = value;
            }
        }

        private int m_DriverOutQueue = 2048;

        /// <summary>
        /// Gets or sets the driver output queue size.
        /// </summary>
        /// <value>The driver output queue size.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="ArgumentOutOfRangeException">value must be a positive integer.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual int DriverOutQueue
        {
            get { return m_DriverOutQueue; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(DriverOutQueue), "value must be a positive integer");
                m_DriverOutQueue = value;
            }
        }

        /// <summary>
        /// Gets the number of bytes in the driver queue still to be read.
        /// </summary>
        /// <value>The bytes to read.</value>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual int BytesToRead
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                return 0;
            }
        }

        /// <summary>
        /// Gets the number of bytes in the output buffer of the driver still to write (not any managed buffers).
        /// </summary>
        /// <value>
        /// The number of bytes in the driver queue for writing. If this value is not supported, zero is returned.
        /// </value>
        /// <exception cref="ObjectDisposedException"/>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual int BytesToWrite
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                return 0;
            }
        }

        private bool m_CDHolding;

        /// <summary>
        /// Gets the state of the Carrier Detect pin on the serial port.
        /// </summary>
        /// <value><see langword="true"/> if carrier detect pin is active; otherwise, <see langword="false"/>.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <remarks>Setting this property results in the <see cref="PinChanged"/> event being raised.</remarks>
        public virtual bool CDHolding
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (!IsOpen) return false;
                return m_CDHolding;
            }
            set
            {
                bool changed = m_CDHolding != value;
                m_CDHolding = value;
                if (changed) OnPinChanged(this, new SerialPinChangedEventArgs(SerialPinChange.CDChanged));
            }
        }

        private bool m_CtsHolding;

        /// <summary>
        /// Gets the state of the Clear To Send pin on the serial port.
        /// </summary>
        /// <value><see langword="true"/> if the clear to send pin is active; otherwise, <see langword="false"/>.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <remarks>Setting this property results in the <see cref="PinChanged"/> event being raised.</remarks>
        public virtual bool CtsHolding
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (!IsOpen) return false;
                return m_CtsHolding;
            }
            set
            {
                bool changed = m_CtsHolding != value;
                m_CtsHolding = value;
                if (changed) OnPinChanged(this, new SerialPinChangedEventArgs(SerialPinChange.CtsChanged));
            }
        }

        private bool m_DsrHolding;

        /// <summary>
        /// Gets the state of the Data Set Ready pin on the serial port.
        /// </summary>
        /// <value><see langword="true"/> if data set ready pin is active; otherwise, <see langword="false"/>.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <remarks>Setting this property results in the <see cref="PinChanged"/> event being raised.</remarks>
        public virtual bool DsrHolding
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (!IsOpen) return false;
                return m_DsrHolding;
            }
            set
            {
                bool changed = m_DsrHolding != value;
                m_DsrHolding = value;
                if (changed) OnPinChanged(this, new SerialPinChangedEventArgs(SerialPinChange.DsrChanged));
            }
        }

        private bool m_RingHolding;

        /// <summary>
        /// Gets the state of the Ring Indicator pin on the serial port.
        /// </summary>
        /// <value><see langword="true"/> if ring indicator state is active; otherwise, <see langword="false"/>.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <remarks>Setting this property results in the <see cref="PinChanged"/> event being raised.</remarks>
        public virtual bool RingHolding
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (!IsOpen) return false;
                return m_RingHolding;
            }
            set
            {
                bool changed = m_RingHolding != value;
                m_RingHolding = value;
                if (changed) OnPinChanged(this, new SerialPinChangedEventArgs(SerialPinChange.Ring));
            }
        }

        private bool m_DtrEnable = true;

        /// <summary>
        /// Gets or sets the Data Terminal Ready pin of the serial port.
        /// </summary>
        /// <value><see langword="true"/> if data terminal pin is active; otherwise, <see langword="false"/>.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual bool DtrEnable
        {
            get { return m_DtrEnable; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                m_DtrEnable = value;
            }
        }

        private bool m_RtsEnable = true;

        /// <summary>
        /// Gets or sets the Request To Send pin of the serial port.
        /// </summary>
        /// <value><see langword="true"/> if RTS is enabled; otherwise, <see langword="false"/>.</value>
        /// <exception cref="ObjectDisposedException"/>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual bool RtsEnable
        {
            get { return m_RtsEnable; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                m_RtsEnable = value;
            }
        }

        private Handshake m_Handshake = Handshake.None;

        /// <summary>
        /// Gets or sets the handshake to use on the serial port.
        /// </summary>
        /// <value>The handshake mode to use on the serial port.</value>
        /// <exception cref="ArgumentOutOfRangeException">Unknown value for Handshake.</exception>
        /// <remarks>This property has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual Handshake Handshake
        {
            get { return m_Handshake; }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
                if (!Enum.IsDefined(typeof(Handshake), value)) {
                    throw new ArgumentOutOfRangeException(nameof(Handshake), "Unknown value for Handshake");
                }
                m_Handshake = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the serial port has been opened.
        /// </summary>
        /// <value><see langword="true"/> if this instance is open; otherwise, <see langword="false"/>.</value>
        public bool IsOpen { get; protected set; }

        /// <summary>
        /// Discards the input queue buffer of the driver.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException">Port not open.</exception>
        /// <remarks>This method has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual void DiscardInBuffer()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
            if (!IsOpen) throw new InvalidOperationException("Port not open");
        }

        /// <summary>
        /// Discards the output queue buffer of the driver.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException">Port not open.</exception>
        public virtual void DiscardOutBuffer()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
            if (!IsOpen) throw new InvalidOperationException("Port not open");

            lock (m_Buffer.SerialWrite.Lock) {
                m_Buffer.SerialWrite.Purge();
            }
        }

        private int m_ShadowBaudRate = 9600;
        private int m_ShadowDataBits = 8;
        private Parity m_ShadowParity = Parity.None;
        private StopBits m_ShadowStopBits = StopBits.One;
        private Handshake m_ShadowHandShake = Handshake.None;
        private bool m_ShadowDiscardNull = false;
        private byte m_ShadowParityReplace = 0;

        /// <summary>
        /// Gets the port settings and updates the properties of the object.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException">Port not open.</exception>
        /// <remarks>This method has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual void GetPortSettings()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
            if (!IsOpen) throw new InvalidOperationException("Port not open");

            BaudRate = m_ShadowBaudRate;
            DataBits = m_ShadowDataBits;
            Parity = m_ShadowParity;
            StopBits = m_ShadowStopBits;
            Handshake = m_ShadowHandShake;
            DiscardNull = m_ShadowDiscardNull;
            ParityReplace = m_ShadowParityReplace;
        }

        /// <summary>
        /// Writes the settings of the serial port as set in this object.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException">Port not open.</exception>
        /// <remarks>This method has no effect for <see cref="VirtualNativeSerial"/>.</remarks>
        public virtual void SetPortSettings()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
            if (!IsOpen) throw new InvalidOperationException("Port not open");

            m_ShadowBaudRate = BaudRate;
            m_ShadowDataBits = DataBits;
            m_ShadowParity = Parity;
            m_ShadowStopBits = StopBits;
            m_ShadowHandShake = Handshake;
            m_ShadowDiscardNull = DiscardNull;
            m_ShadowParityReplace = ParityReplace;
        }

        /// <summary>
        /// Opens the serial port specified by <see cref="PortName"/>.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <exception cref="InvalidOperationException">
        /// Port must first be set;
        /// <para>- or -</para>
        /// Serial Port currently open.
        /// </exception>
        public virtual void Open()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
            if (string.IsNullOrWhiteSpace(PortName)) throw new InvalidOperationException("Port must first be set");
            if (IsOpen) throw new InvalidOperationException("Serial Port currently open");
            IsOpen = true;
        }

        /// <summary>
        /// Closes the serial port.
        /// </summary>
        /// <exception cref="ObjectDisposedException"/>
        /// <remarks>
        /// Closing the serial port invalidates actions that can be done to the serial port, but it does not prevent the
        /// serial port from being reopened
        /// </remarks>
        public virtual void Close()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
            m_Buffer.Close();
            IsRunning = false;
            IsOpen = false;
        }

        /// <summary>
        /// Gets the buffer that is used for reading and writing to the serial port.
        /// </summary>
        /// <value>The buffer used for reading and writing to the serial port.</value>
        public SerialBuffer Buffer
        {
            get { return m_Buffer; }
        }

        /// <summary>
        /// Start the monitor thread, that will watch over the serial port.
        /// </summary>
        public virtual void StartMonitor()
        {
            if (IsDisposed) throw new ObjectDisposedException(nameof(WinNativeSerial));
            if (!IsOpen) throw new InvalidOperationException("Serial Port not open");

            m_Buffer.Reset();
            IsRunning = true;
        }

        /// <summary>
        /// Stops the monitor thread, as if an error occurred (e.g. the device was removed).
        /// </summary>
        /// <remarks>
        /// Stopping the monitor thread, as opposed to calling <see cref="Close"/>, will eventually indicate an error
        /// when reading from the stream, as the device remains open, but is not running. Calling <see cref="Close"/>
        /// should not be done except by the <see cref="SerialPortStream"/> itself.
        /// </remarks>
        public virtual void StopMonitor()
        {
            IsRunning = false;
            m_Buffer.Close();
        }

        private bool m_IsRunning;

        /// <summary>
        /// Gets a value indicating whether the thread for monitoring the serial port is running.
        /// </summary>
        /// <value><see langword="true"/> if this instance is running; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// This property differs slightly from <see cref="IsOpen"/>, as this returns status if the monitoring thread
        /// for reading/writing data is actually running. If the thread is not running for whatever reason, we can
        /// expect no data updates in the buffer provided to <see cref="StartMonitor()"/>.
        /// </remarks>
        public bool IsRunning
        {
            get
            {
                if (IsDisposed || !IsOpen) return false;
                return m_IsRunning;
            }
            protected set { m_IsRunning = value; }
        }

        /// <summary>
        /// Provides access to methods for manipulating the <see cref="Buffer"/> in a safe way.
        /// </summary>
        /// <value>The virtual buffer.</value>
        /// <remarks>
        /// Use the methods exposed by this property to access low level buffer API for knowing when the user wrote to
        /// the stream, and to post results that the user can read from the stream.
        /// </remarks>
        public IVirtualSerialBuffer VirtualBuffer { get { return m_Buffer; } }

        /// <summary>
        /// Occurs when data is received, or the EOF character is detected by the driver.
        /// </summary>
        public event EventHandler<SerialDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Called when data is received, or the EOF character is detected by the driver.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SerialDataReceivedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            EventHandler<SerialDataReceivedEventArgs> handler = DataReceived;
            if (handler != null) {
                handler(sender, args);
            }
        }

        /// <summary>
        /// Occurs when an error condition is detected.
        /// </summary>
        public event EventHandler<SerialErrorReceivedEventArgs> ErrorReceived;

        /// <summary>
        /// Called when an error condition is detected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SerialErrorReceivedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCommError(object sender, SerialErrorReceivedEventArgs args)
        {
            EventHandler<SerialErrorReceivedEventArgs> handler = ErrorReceived;
            if (handler != null) {
                handler(sender, args);
            }
        }

        /// <summary>
        /// Occurs when modem pin changes are detected.
        /// </summary>
        public event EventHandler<SerialPinChangedEventArgs> PinChanged;

        /// <summary>
        /// Called when modem pin changes are detected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SerialPinChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPinChanged(object sender, SerialPinChangedEventArgs args)
        {
            EventHandler<SerialPinChangedEventArgs> handler = PinChanged;
            if (handler != null) {
                handler(sender, args);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>Is <see langword="true"/> if this instance is disposed; otherwise, <see langword="false"/>.</value>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release
        /// only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing) {
                if (IsOpen) Close();
            }

            IsDisposed = true;
        }
    }
}
