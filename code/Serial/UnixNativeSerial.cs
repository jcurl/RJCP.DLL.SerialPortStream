namespace RJCP.IO.Ports.Serial
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Versioning;
    using System.Threading;
    using Native.Unix;
    using RJCP.Diagnostics.Trace;

    /// <summary>
    /// Windows implementation for a Native Serial connection.
    /// </summary>
    [SupportedOSPlatform("linux")]
    public class UnixNativeSerial : INativeSerial
    {
        private readonly LibNSerial.SafeSerialHandle m_Handle;
        private readonly IntPtr m_HandlePtr;
        private readonly LogSource m_Log;
        private readonly SerialBuffer m_Buffer;

        private readonly AutoResetEvent m_WriteClearEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent m_WriteClearDoneEvent = new AutoResetEvent(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixNativeSerial"/> class for Unix systems.
        /// </summary>
        public UnixNativeSerial() : this(new LogSource()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnixNativeSerial"/> class with logging for Unix systems.
        /// </summary>
        /// <param name="log">The log source object.</param>
        /// <exception cref="ArgumentNullException"><paramref name="log"/> is <see langword="null"/>.</exception>
        /// <exception cref="PlatformNotSupportedException">Can't initialise platform library.</exception>
        public UnixNativeSerial(LogSource log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            m_Log = log;

            m_Buffer = new SerialBuffer();
            m_Handle = LibNSerial.serial_init();
            if (m_Handle.IsInvalid) {
                throw new PlatformNotSupportedException("Can't initialise platform library");
            }
            m_HandlePtr = m_Handle.DangerousGetHandle();

#if NET6_0_OR_GREATER
            // On NetStandard 2.1, we must have proper exception handling
            try {
                // These methods were first added in libnserial 1.1
                LibNSerial.netfx_errno(0);
            } catch (EntryPointNotFoundException) {
                throw new PlatformNotSupportedException("Must have libnserial 1.1.0 or later on .NET Core");
            }
#endif
        }

        private void ThrowException()
        {
            if (m_IsDisposed) return;

            LibNSerial.SysErrNo managedErrNo;
            string sysDescription;
            try {
                // These methods were first added in libnserial 1.1
                managedErrNo = LibNSerial.netfx_errno(LibNSerial.errno);
                sysDescription = LibNSerial.netfx_errstring(LibNSerial.errno);
            } catch (EntryPointNotFoundException) {
#if NET6_0_OR_GREATER
                ThrowExceptionNetStandard();
#else
                ThrowExceptionMono();
#endif
                throw;
            }
            string libDescription = LibNSerial.serial_error(m_Handle);

            string description = string.Format("{0} ({1}; {2})",
                libDescription, LibNSerial.errno, sysDescription);

            switch (managedErrNo) {
            case LibNSerial.SysErrNo.NETFX_OK:
            case LibNSerial.SysErrNo.NETFX_EINTR:
            case LibNSerial.SysErrNo.NETFX_EAGAIN:
            case LibNSerial.SysErrNo.NETFX_EWOULDBLOCK:
                // We throw here in any case, as the methods that call ThrowException don't
                // expect it to return. This would mean something that needs to be debugged
                // and fixed (probably in the C Interop Code).
                throw new InternalApplicationException(description);
            case LibNSerial.SysErrNo.NETFX_EINVAL:
                throw new ArgumentException(description);
            case LibNSerial.SysErrNo.NETFX_EACCES:
                throw new UnauthorizedAccessException(description);
            case LibNSerial.SysErrNo.NETFX_ENOMEM:
                throw new OutOfMemoryException(description);
            case LibNSerial.SysErrNo.NETFX_EBADF:
                throw new InvalidOperationException(description);
            case LibNSerial.SysErrNo.NETFX_ENOSYS:
                throw new PlatformNotSupportedException(description);
            case LibNSerial.SysErrNo.NETFX_EIO:
                throw new IOException(description);
            default:
                throw new InvalidOperationException(description);
            }
        }

#if !NET6_0_OR_GREATER
        // For compatibility with libnserial 1.0 only.
        private void ThrowExceptionMono()
        {
            Mono.Unix.Native.Errno errno = Mono.Unix.Native.NativeConvert.ToErrno(LibNSerial.errno);
            string description = LibNSerial.serial_error(m_Handle);
            switch (errno) {
            case Mono.Unix.Native.Errno.EINVAL:
                throw new ArgumentException(description);
            case Mono.Unix.Native.Errno.EACCES:
                throw new UnauthorizedAccessException(description);
            default:
                throw new InvalidOperationException(description);
            }
        }
#endif

#if NET6_0_OR_GREATER
        // For compatibility with libnserial 1.0 only.
        private void ThrowExceptionNetStandard()
        {
            string description = LibNSerial.serial_error(m_Handle);
            throw new Exception(string.Format($"Error {LibNSerial.errno}: {description}"));
        }
#endif

        /// <summary>
        /// Gets the version of the implementation in use.
        /// </summary>
        /// <value>
        /// The version of the implementation in use.
        /// </value>
        public string Version
        {
            get { return LibNSerial.serial_version(); }
        }

        /// <summary>
        /// Gets or sets the port device path.
        /// </summary>
        /// <value>
        /// The port device path.
        /// </value>
        public string PortName
        {
            get
            {
                string deviceName = LibNSerial.serial_getdevicename(m_Handle);
                if (deviceName == null) ThrowException();
                return deviceName;
            }
            set
            {
                if (LibNSerial.serial_setdevicename(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets an array of serial port names.
        /// </summary>
        /// <returns>An array of serial port names.</returns>
        public string[] GetPortNames()
        {
            PortDescription[] ports;
            try {
                ports = LibNSerial.serial_getports(m_Handle);
            } catch (EntryPointNotFoundException) {
                // libnserial is version < 1.1.0
                ports = null;
            }

            if (ports == null)
#if NETFRAMEWORK
                // The implementation, most likely from Mono.
                return System.IO.Ports.SerialPort.GetPortNames();
#else
                // There is no alternative implementation for Linux.
                return Array.Empty<string>();
#endif

            string[] portNames = new string[ports.Length];
            for (int i = 0; i < ports.Length; i++) {
                portNames[i] = ports[i].Port;
            }
            return portNames;
        }

        /// <summary>
        /// Gets an array of serial port names and descriptions.
        /// </summary>
        /// <returns>An array of serial ports.</returns>
        /// <remarks>
        /// Get the ports available, and their descriptions, for the current serial port implementation. On Windows,
        /// this uses the Windows Management Interface to obtain its information. Therefore, the list may be different
        /// to the list obtained using the <see cref="GetPortNames"/>.
        /// </remarks>
        public PortDescription[] GetPortDescriptions()
        {
            PortDescription[] ports;
            try {
                ports = LibNSerial.serial_getports(m_Handle);
            } catch (EntryPointNotFoundException) {
                ports = null;
            }
            if (ports != null)
                return ports;

#if NETFRAMEWORK
            string[] portNamess =
                System.IO.Ports.SerialPort.GetPortNames();
            PortDescription[] portdescs = new PortDescription[portNamess.Length];

            for (int i = 0; i < portNamess.Length; i++) {
                portdescs[i] = new PortDescription(portNamess[i], "");
            }
            return portdescs;
#else
            return Array.Empty<PortDescription>();
#endif
        }

        /// <summary>
        /// Gets or sets the baud rate.
        /// </summary>
        /// <value>
        /// The baud rate.
        /// </value>
        public int BaudRate
        {
            get
            {
                if (LibNSerial.serial_getbaud(m_Handle, out int baud) == -1) ThrowException();
                return baud;
            }
            set
            {
                if (LibNSerial.serial_setbaud(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the data bits.
        /// </summary>
        /// <value>
        /// The data bits.
        /// </value>
        public int DataBits
        {
            get
            {
                if (LibNSerial.serial_getdatabits(m_Handle, out int databits) == -1) ThrowException();
                return databits;
            }
            set
            {
                if (LibNSerial.serial_setdatabits(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the parity.
        /// </summary>
        /// <value>
        /// The parity.
        /// </value>
        public Parity Parity
        {
            get
            {
                if (LibNSerial.serial_getparity(m_Handle, out Parity parity) == -1) ThrowException();
                return parity;
            }
            set
            {
                if (LibNSerial.serial_setparity(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the stop bits.
        /// </summary>
        /// <value>
        /// The stop bits.
        /// </value>
        public StopBits StopBits
        {
            get
            {
                if (LibNSerial.serial_getstopbits(m_Handle, out StopBits stopbits) == -1) ThrowException();
                return stopbits;
            }
            set
            {
                if (LibNSerial.serial_setstopbits(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets a value if null bytes should be discarded or not.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if null bytes should be discarded; otherwise, <see langword="false"/>.
        /// </value>
        public bool DiscardNull
        {
            get
            {
                if (LibNSerial.serial_getdiscardnull(m_Handle, out bool discardNull) == -1) ThrowException();
                return discardNull;
            }
            set
            {
                if (LibNSerial.serial_setdiscardnull(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the parity replace byte.
        /// </summary>
        /// <value>
        /// The byte to use on parity errors.
        /// </value>
        public byte ParityReplace
        {
            get
            {
                if (LibNSerial.serial_getparityreplace(m_Handle, out int parityReplace) == -1) ThrowException();
                return (byte)parityReplace;
            }
            set
            {
                if (LibNSerial.serial_setparityreplace(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether transmission should still
        /// be sent when the input buffer is full and if the XOff character has been sent.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if transmission should continue after the input buffer
        ///   is within <see cref="XOffLimit"/> bytes of being full and the driver has sent the
        ///   XOff character; otherwise,<see langword="false"/> that transmission should stop
        ///   and only continue when the input buffer is within <see cref="XOnLimit"/> bytes of
        ///   being empty and the driver has sent the XOn character.
        /// </value>
        public bool TxContinueOnXOff
        {
            get
            {
                if (LibNSerial.serial_gettxcontinueonxoff(m_Handle, out bool txContinueOnXOff) == -1) ThrowException();
                return txContinueOnXOff;
            }
            set
            {
                if (LibNSerial.serial_settxcontinueonxoff(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the XOff limit input when the XOff character should be sent.
        /// </summary>
        /// <value>
        /// The XOff buffer limit.
        /// </value>
        public int XOffLimit
        {
            get
            {
                if (LibNSerial.serial_getxofflimit(m_Handle, out int xoffLimit) == -1) ThrowException();
                return xoffLimit;
            }
            set
            {
                if (LibNSerial.serial_setxofflimit(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the XOn limit when the input buffer is below when the XOn character should be sent.
        /// </summary>
        /// <value>
        /// The XOn buffer limit.
        /// </value>
        public int XOnLimit
        {
            get
            {
                if (LibNSerial.serial_getxonlimit(m_Handle, out int xonLimit) == -1) ThrowException();
                return xonLimit;
            }
            set
            {
                if (LibNSerial.serial_setxonlimit(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the break state of the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if in the break state; otherwise, <see langword="false"/>.
        /// </value>
        public bool BreakState
        {
            get
            {
                if (LibNSerial.serial_getbreak(m_Handle, out bool breakState) == -1) ThrowException();
                return breakState;
            }
            set
            {
                if (LibNSerial.serial_setbreak(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the driver input queue size.
        /// </summary>
        /// <value>
        /// The driver input queue size.
        /// </value>
        /// <remarks>
        /// This method is typically available with Windows API only.
        /// </remarks>
        public int DriverInQueue
        {
            get { return -1; }
            set { /* Not supported, just ignore */ }
        }

        /// <summary>
        /// Gets or sets the driver output queue size.
        /// </summary>
        /// <value>
        /// The driver output queue size.
        /// </value>
        /// <remarks>
        /// This method is typically available with Windows API only.
        /// </remarks>
        public int DriverOutQueue
        {
            get { return -1; }
            set { /* Not supported, just ignore */ }
        }

        /// <summary>
        /// Gets the number of bytes in the input queue of the driver not yet read (not any managed buffers).
        /// </summary>
        /// <value>
        /// The number of bytes in the driver queue for reading. If this value is not supported, zero is returned.
        /// </value>
        public int BytesToRead { get { return 0; } }

        /// <summary>
        /// Gets the number of bytes in the output buffer of the driver still to write (not any managed buffers).
        /// </summary>
        /// <value>
        /// The number of bytes in the driver queue for writing. If this value is not supported, zero is returned.
        /// </value>
        public int BytesToWrite { get { return 0; } }

        /// <summary>
        /// Gets the state of the Carrier Detect pin on the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if carrier detect pin is active; otherwise, <see langword="false"/>.
        /// </value>
        public bool CDHolding
        {
            get
            {
                if (LibNSerial.serial_getdcd(m_Handle, out bool cdHolding) == -1) ThrowException();
                return cdHolding;
            }
        }

        /// <summary>
        /// Gets the state of the Clear To Send pin on the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the clear to send pin is active; otherwise, <see langword="false"/>.
        /// </value>
        public bool CtsHolding
        {
            get
            {
                if (LibNSerial.serial_getcts(m_Handle, out bool ctsHolding) == -1) ThrowException();
                return ctsHolding;
            }
        }

        /// <summary>
        /// Gets the state of the Data Set Ready pin on the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if data set ready pin is active; otherwise, <see langword="false"/>.
        /// </value>
        public bool DsrHolding
        {
            get
            {
                if (LibNSerial.serial_getdsr(m_Handle, out bool dsrHolding) == -1) ThrowException();
                return dsrHolding;
            }
        }

        /// <summary>
        /// Gets the state of the Ring Indicator pin on the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if ring indicator state is active; otherwise, <see langword="false"/>.
        /// </value>
        public bool RingHolding
        {
            get
            {
                if (LibNSerial.serial_getri(m_Handle, out bool ringHolding) == -1) ThrowException();
                return ringHolding;
            }
        }

        /// <summary>
        /// Gets or sets the Data Terminal Ready pin of the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if data terminal pin is active; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This pin only has an effect if handshaking for DTR/DTS is disabled.
        /// </remarks>
        public bool DtrEnable
        {
            get
            {
                if (LibNSerial.serial_getdtr(m_Handle, out bool dtrEnable) == -1) ThrowException();
                return dtrEnable;
            }
            set
            {
                if (LibNSerial.serial_setdtr(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the Request To Send pin of the serial port.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if [RTS enable]; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This pin only has an effect if the handshaking for RTS/CTS is disabled.
        /// </remarks>
        public bool RtsEnable
        {
            get
            {
                if (LibNSerial.serial_getrts(m_Handle, out bool rtsEnable) == -1) ThrowException();
                return rtsEnable;
            }
            set
            {
                if (LibNSerial.serial_setrts(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the handshake to use on the serial port.
        /// </summary>
        /// <value>
        /// The handshake mode to use on the serial port.
        /// </value>
        public Handshake Handshake
        {
            get
            {
                if (LibNSerial.serial_gethandshake(m_Handle, out Handshake handshake) == -1) ThrowException();
                return handshake;
            }
            set
            {
                if (LibNSerial.serial_sethandshake(m_Handle, value) == -1) ThrowException();
            }
        }

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
        public bool IsOpen
        {
            get
            {
                if (LibNSerial.serial_isopen(m_Handle, out bool isOpen) == -1) ThrowException();
                return isOpen;
            }
        }

        /// <summary>
        /// Discards the input queue buffer of the driver.
        /// </summary>
        public void DiscardInBuffer()
        {
            if (LibNSerial.serial_discardinbuffer(m_Handle) == -1) ThrowException();
        }

        /// <summary>
        /// Discards the output queue buffer of the driver.
        /// </summary>
        public void DiscardOutBuffer()
        {
            if (IsRunning) {
                m_WriteClearEvent.Set();
                m_WriteClearDoneEvent.WaitOne(Timeout.Infinite);
            }
        }

        /// <summary>
        /// Gets the port settings and updates the properties of the object.
        /// </summary>
        public void GetPortSettings()
        {
            if (LibNSerial.serial_getproperties(m_Handle) == -1) ThrowException();
        }

        /// <summary>
        /// Writes the settings of the serial port as set in this object.
        /// </summary>
        public void SetPortSettings()
        {
            if (LibNSerial.serial_setproperties(m_Handle) == -1) ThrowException();
        }

        /// <summary>
        /// Opens the serial port specified by <see cref="PortName"/>.
        /// </summary>
        /// <remarks>
        /// Opening the serial port does not set any settings (such as baud rate, etc.)
        /// </remarks>
        public void Open()
        {
            if (LibNSerial.serial_open(m_Handle) == -1) ThrowException();
        }

        /// <summary>
        /// Closes the serial port.
        /// </summary>
        /// <remarks>
        /// Closing the serial port invalidates actions that can be done to the serial port,
        /// but it does not prevent the serial port from being reopened
        /// </remarks>
        public void Close()
        {
            if (IsOpen) Stop();
            m_Buffer.Close();
            if (LibNSerial.serial_close(m_Handle) == -1) ThrowException();
        }

        /// <summary>
        /// Gets the buffer that is used for reading and writing to the serial port.
        /// </summary>
        /// <value>The buffer used for reading and writing to the serial port.</value>
        public SerialBuffer Buffer
        {
            get { return m_Buffer; }
        }

        private Thread m_MonitorThread;
        private Thread m_PinThread;
        private string m_Name;
        private volatile bool m_IsRunning;
        private volatile bool m_MonitorPins;
        private readonly ManualResetEvent m_StopRunning = new ManualResetEvent(false);

        /// <summary>
        /// Start the monitor thread, that will watch over the serial port.
        /// </summary>
        public void StartMonitor()
        {
            if (m_IsDisposed) throw new ObjectDisposedException(nameof(UnixNativeSerial));
            if (!IsOpen) throw new InvalidOperationException("Serial Port not open");

            m_Buffer.Reset();
            m_Name = PortName;
            m_StopRunning.Reset();

            try {
                m_IsRunning = true;
                m_MonitorThread = new Thread(new ThreadStart(ReadWriteThread)) {
                    Name = "NSerMon_" + m_Name,
                    IsBackground = true
                };
                m_MonitorThread.Start();
            } catch {
                m_IsRunning = false;
                throw;
            }

            try {
                m_MonitorPins = true;
                m_PinThread = new Thread(new ThreadStart(PinChangeThread)) {
                    Name = "NSerPin_" + m_Name,
                    IsBackground = true
                };
                m_PinThread.Start();
            } catch {
                m_PinThread = null;
                m_MonitorPins = false;
                throw;
            }
        }

        private void Stop()
        {
            if (m_Log.ShouldTrace(TraceEventType.Verbose))
                m_Log.TraceEvent(TraceEventType.Verbose, $"{m_Name}: ReadWriteThread: Stopping Thread");
            m_StopRunning.Set();
            InterruptReadWriteLoop();

            if (m_PinThread != null) {
                int killcounter = 0;

                if (m_MonitorPins) {
                    if (m_Log.ShouldTrace(TraceEventType.Verbose))
                        m_Log.TraceEvent(TraceEventType.Verbose, $"{m_Name}: PinChangeThread: Stopping Thread");
                    LibNSerial.serial_abortwaitformodemevent(m_Handle);
                }
                if (m_Log.ShouldTrace(TraceEventType.Verbose))
                    m_Log.TraceEvent(TraceEventType.Verbose, $"{m_Name}: PinChangeThread: Waiting for Thread");
                while (killcounter < 3 && !m_PinThread.Join(100)) {
                    if (m_Log.ShouldTrace(TraceEventType.Verbose))
                        m_Log.TraceEvent(TraceEventType.Verbose, $"{m_Name}: PinChangeThread: Waiting for Thread, counter={killcounter}");
                    killcounter++;
                }
                if (m_Log.ShouldTrace(TraceEventType.Verbose))
                    m_Log.TraceEvent(TraceEventType.Verbose, $"{m_Name}: PinChangeThread: Thread Stopped");
                m_PinThread = null;
                m_MonitorPins = false;
            }

            if (m_MonitorThread != null) {
                if (m_Log.ShouldTrace(TraceEventType.Verbose))
                    m_Log.TraceEvent(TraceEventType.Verbose, $"{m_Name}: ReadWriteThread: Waiting for Thread");
                m_MonitorThread.Join();
                if (m_Log.ShouldTrace(TraceEventType.Verbose))
                    m_Log.TraceEvent(TraceEventType.Verbose, $"{m_Name}: ReadWriteThread: Thread Stopped");
                m_MonitorThread = null;
            }
        }

        private unsafe void ReadWriteThread()
        {
            WaitHandle[] handles = new WaitHandle[] {
                m_StopRunning,
                m_WriteClearEvent,
                m_Buffer.SerialRead.BufferNotFull,
                m_Buffer.SerialWrite.BufferNotEmpty
            };

            // The user writes data, and the serial_waitforevent should be interrupted.
            m_Buffer.SerialWrite.WriteEvent += SerialBufferWriteEvent;

            while (m_IsRunning) {
                LibNSerial.SerialReadWriteEvent rwevent = LibNSerial.SerialReadWriteEvent.NoEvent;

                int handle = WaitHandle.WaitAny(handles, -1);
                switch (handle) {
                case 0: // StopRunning - Should abort
                    m_IsRunning = false;
                    continue;
                case 1: // Clear write buffers
                    m_Buffer.SerialWrite.Purge();
                    LibNSerial.serial_discardoutbuffer(m_Handle);
                    m_WriteClearDoneEvent.Set();
                    continue;
                }

                // These are not in the switch statement to ensure that we can actually
                // read/write simultaneously.
                if (m_Buffer.SerialRead.IsBufferNotFull) {
                    rwevent |= LibNSerial.SerialReadWriteEvent.ReadEvent;
                }
                if (m_Buffer.SerialWrite.IsBufferNotEmpty) {
                    rwevent |= LibNSerial.SerialReadWriteEvent.WriteEvent;
                }

                LibNSerial.SerialReadWriteEvent result = LibNSerial.serial_waitforevent(m_Handle, rwevent, 500);
                if (result == LibNSerial.SerialReadWriteEvent.Error) {
                    if (m_Log.ShouldTrace(TraceEventType.Error))
                        m_Log.TraceEvent(TraceEventType.Error,
                            $"{m_Name}: ReadWriteThread: Error waiting for event; errno={LibNSerial.errno}; description={LibNSerial.serial_error(m_Handle)}");
                    m_IsRunning = false;
                    continue;
                } else if (m_Log.ShouldTrace(TraceEventType.Verbose)) {
                    m_Log.TraceEvent(TraceEventType.Verbose,
                        $"{m_Name}: ReadWriteThread: serial_waitforevent({m_HandlePtr}, {rwevent}) == {result}");
                }

                if ((result & LibNSerial.SerialReadWriteEvent.ReadEvent) != 0) {
                    int rresult;
                    fixed (byte* b = m_Buffer.SerialRead.Buffer) {
                        byte* bo;
                        int length;
                        lock (m_Buffer.SerialRead.Lock) {
                            bo = b + m_Buffer.SerialRead.BufferEnd;
                            length = m_Buffer.SerialRead.BufferWriteLength;
                        }
                        rresult = LibNSerial.serial_read(m_Handle, (IntPtr)bo, length);
                        if (rresult < 0) {
                            if (m_Log.ShouldTrace(TraceEventType.Error))
                                m_Log.TraceEvent(TraceEventType.Error,
                                    $"{m_Name}: ReadWriteThread: Error reading data; errno={LibNSerial.errno}; description={LibNSerial.serial_error(m_Handle)}");
                            m_IsRunning = false;
                            continue;
                        } else {
                            if (m_Log.ShouldTrace(TraceEventType.Verbose))
                                m_Log.TraceEvent(TraceEventType.Verbose,
                                    $"{m_Name}: ReadWriteThread: serial_read({m_HandlePtr}, {(IntPtr)bo}, {length}) == {rresult}");
                            if (rresult > 0) {
                                lock (m_Buffer.SerialRead.Lock) {
                                    m_Buffer.SerialRead.Produce(rresult);
                                }
                            }
                        }
                    }
                    if (rresult > 0) OnDataReceived(this, new SerialDataReceivedEventArgs(SerialData.Chars));
                }

                if ((result & LibNSerial.SerialReadWriteEvent.WriteEvent) != 0) {
                    int wresult;
                    fixed (byte* b = m_Buffer.SerialWrite.Buffer) {
                        byte* bo;
                        int length;
                        lock (m_Buffer.SerialWrite.Lock) {
                            bo = b + m_Buffer.SerialWrite.BufferStart;
                            length = m_Buffer.SerialWrite.BufferReadLength;
                        }
                        wresult = LibNSerial.serial_write(m_Handle, (IntPtr)bo, length);
                        if (wresult < 0) {
                            if (m_Log.ShouldTrace(TraceEventType.Error))
                                m_Log.TraceEvent(TraceEventType.Error,
                                    $"{m_Name}: ReadWriteThread: Error writing data; errno={LibNSerial.errno}; description={LibNSerial.serial_error(m_Handle)}");
                            m_IsRunning = false;
                        } else {
                            if (m_Log.ShouldTrace(TraceEventType.Verbose))
                                m_Log.TraceEvent(TraceEventType.Verbose,
                                    $"{m_Name}: ReadWriteThread: serial_write({m_HandlePtr}, {(IntPtr)bo}, {length}) == {wresult}");
                            if (wresult > 0) {
                                lock (m_Buffer.SerialWrite.Lock) {
                                    m_Buffer.SerialWrite.Consume(wresult);
                                }
                            }
                        }
                    }
                }
            }
            m_Buffer.SerialWrite.WriteEvent -= SerialBufferWriteEvent;

            // We must notify the stream that any blocking waits should abort.
            m_Buffer.Close();
        }

        private void SerialBufferWriteEvent(object sender, EventArgs e)
        {
            InterruptReadWriteLoop();
        }

        private void InterruptReadWriteLoop()
        {
            if (m_IsRunning && !m_Handle.IsInvalid) {
                int result = LibNSerial.serial_abortwaitforevent(m_Handle);
                if (result == -1) {
                    if (m_Log.ShouldTrace(TraceEventType.Error))
                        m_Log.TraceEvent(TraceEventType.Error,
                            $"{m_Name}: ReadWriteThread: Error aborting event; errno={LibNSerial.errno}; description={LibNSerial.serial_error(m_Handle)}");
                } else {
                    if (m_Log.ShouldTrace(TraceEventType.Verbose))
                        m_Log.TraceEvent(TraceEventType.Verbose,
                            $"{m_Name}: ReadWriteThread: serial_abortwaitforevent({m_HandlePtr}) = {result}");
                }
            }
        }

        private const LibNSerial.WaitForModemEvent c_ModemEvents =
            LibNSerial.WaitForModemEvent.RingIndicator |
            LibNSerial.WaitForModemEvent.ClearToSend |
            LibNSerial.WaitForModemEvent.DataCarrierDetect |
            LibNSerial.WaitForModemEvent.DataSetReady;

        private void PinChangeThread()
        {
            while (m_MonitorPins) {
                if (m_StopRunning.WaitOne(0)) {
                    m_MonitorPins = false;
                    continue;
                }

                if (m_Log.ShouldTrace(TraceEventType.Verbose))
                    m_Log.TraceEvent(TraceEventType.Verbose,
                        $"{m_Name}: PinChangeThread: Waiting");
                LibNSerial.WaitForModemEvent mevent = LibNSerial.serial_waitformodemevent(m_Handle, c_ModemEvents);
                if (mevent == LibNSerial.WaitForModemEvent.Error) {
                    if (m_Log.ShouldTrace(TraceEventType.Error))
                        m_Log.TraceEvent(TraceEventType.Error,
                            $"{m_Name}: PinChangeThread: Error aborting event; errno={LibNSerial.errno}; description={LibNSerial.serial_error(m_Handle)}");
                    m_MonitorPins = false;
                    return;
                }

                if (mevent != LibNSerial.WaitForModemEvent.None) {
                    SerialPinChange pins = SerialPinChange.NoChange;
                    if ((mevent & LibNSerial.WaitForModemEvent.ClearToSend) != 0) pins |= SerialPinChange.CtsChanged;
                    if ((mevent & LibNSerial.WaitForModemEvent.DataCarrierDetect) != 0) pins |= SerialPinChange.CDChanged;
                    if ((mevent & LibNSerial.WaitForModemEvent.DataSetReady) != 0) pins |= SerialPinChange.DsrChanged;
                    if ((mevent & LibNSerial.WaitForModemEvent.RingIndicator) != 0) pins |= SerialPinChange.Ring;
                    // TODO: Break not implemented

                    if (m_Log.ShouldTrace(TraceEventType.Verbose))
                        m_Log.TraceEvent(TraceEventType.Verbose,
                            $"{m_Name}: PinChangeThread: Event Received: {mevent}");
                    OnPinChanged(this, new SerialPinChangedEventArgs(pins));
                }
            }
        }

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
        public bool IsRunning { get { return m_IsRunning; } }

        /// <summary>
        /// Occurs when data is received, or the EOF character is detected by the driver.
        /// </summary>
        public event EventHandler<SerialDataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Handles the <see cref="DataReceived"/> event.
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
        /// Handles the <see cref="ErrorReceived"/> event.
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
        /// Handles the <see cref="PinChanged"/> event.
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

        #region IDisposable Support
        private bool m_IsDisposed;

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
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_IsDisposed) return;
            m_IsDisposed = true;

            if (disposing) {
                if (IsOpen) Close();
                m_Buffer.Dispose();
                m_Handle.Dispose();
                m_WriteClearEvent.Dispose();
                m_WriteClearDoneEvent.Dispose();
                m_StopRunning.Dispose();
            }
        }
        #endregion
    }
}
