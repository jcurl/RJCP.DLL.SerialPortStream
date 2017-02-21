// Copyright © Jason Curl 2012-2017
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native
{
    using System;
    using System.Threading;
    using Unix;

    /// <summary>
    /// Windows implementation for a Native Serial connection.
    /// </summary>
    /// <seealso cref="RJCP.IO.Ports.Native.INativeSerial" />
    internal class UnixNativeSerial : INativeSerial
    {
        private INativeSerialDll m_Dll;
        private IntPtr m_Handle;

        public UnixNativeSerial()
        {
            m_Dll = new SerialUnix();
            m_Handle = m_Dll.serial_init();
            if (m_Handle.Equals(IntPtr.Zero)) {
                throw new PlatformNotSupportedException("Can't initialise platform library");
            }
        }

        private void ThrowException()
        {
            if (m_Dll == null) 
            return;

#if NETSTANDARD15
            throw new Exception(string.Format("Error {0}", m_Dll.errno));
#else   
            Mono.Unix.Native.Errno errno = Mono.Unix.Native.NativeConvert.ToErrno(m_Dll.errno);
            string description = m_Dll.serial_error(m_Handle);

            switch (errno) {
            case Mono.Unix.Native.Errno.EINVAL:
                throw new ArgumentException(description);
            case Mono.Unix.Native.Errno.EACCES:
                throw new UnauthorizedAccessException(description);
            default:
                throw new InvalidOperationException(description);
            }
#endif
        }

        /// <summary>
        /// Gets the version of the implementation in use.
        /// </summary>
        /// <value>
        /// The version of the implementation in use.
        /// </value>
        public string Version
        {
            get { return m_Dll.serial_version(); }
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
                string deviceName = m_Dll.serial_getdevicename(m_Handle);
                if (deviceName == null) ThrowException();
                return deviceName;
            }
            set
            {
                if (m_Dll.serial_setdevicename(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets an array of serial port names for the current computer.
        /// </summary>
        /// <returns>An array of serial port names for the current computer.</returns>
        public string[] GetPortNames()
        {
#if NETSTANDARD15
            throw new NotImplementedException("GetPortNames");
#else
            PortDescription[] ports;
            try {
                ports = m_Dll.serial_getports(m_Handle);
            } catch (System.EntryPointNotFoundException) {
                // libnserial is version < 1.1.0
                ports = null;
            }
            if (ports == null) return System.IO.Ports.SerialPort.GetPortNames();

            string[] portNames = new string[ports.Length];
            for (int i = 0; i < ports.Length; i++) {
                portNames[i] = ports[i].Port;
            }
            return portNames;
#endif
        }

        /// <summary>
        /// Gets an array of serial port names and descriptions for the current computer.
        /// </summary>
        /// <remarks>
        /// This method uses the Windows Management Interface to obtain its information. Therefore,
        /// the list may be different to the list obtained using the GetPortNames() method which
        /// uses other techniques.
        /// <para>On Windows 7, this method shows to return normal COM ports, but not those
        /// associated with a modem driver.</para>
        /// </remarks>
        /// <returns>An array of serial ports for the current computer.</returns>
        public PortDescription[] GetPortDescriptions()
        {
            PortDescription[] ports;
            try {
                ports = m_Dll.serial_getports(m_Handle);
            } catch (System.EntryPointNotFoundException) {
                ports = null;
            }
            if (ports == null) return GetRuntimePortDescriptions();

            return ports;
        }

        private PortDescription[] GetRuntimePortDescriptions()
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            PortDescription[] portdescs = new PortDescription[ports.Length];

            for (int i = 0; i < ports.Length; i++) {
                portdescs[i] = new PortDescription(ports[i], "");
            }
            return portdescs;
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
                int baud;
                if (m_Dll.serial_getbaud(m_Handle, out baud) == -1) ThrowException();
                return baud;
            }
            set
            {
                if (m_Dll.serial_setbaud(m_Handle, value) == -1) ThrowException();
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
                int databits;
                if (m_Dll.serial_getdatabits(m_Handle, out databits) == -1) ThrowException();
                return databits;
            }
            set
            {
                if (m_Dll.serial_setdatabits(m_Handle, value) == -1) ThrowException();
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
                Parity parity;
                if (m_Dll.serial_getparity(m_Handle, out parity) == -1) ThrowException();
                return parity;
            }
            set
            {
                if (m_Dll.serial_setparity(m_Handle, value) == -1) ThrowException();
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
                StopBits stopbits;
                if (m_Dll.serial_getstopbits(m_Handle, out stopbits) == -1) ThrowException();
                return stopbits;
            }
            set
            {
                if (m_Dll.serial_setstopbits(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets a value if null bytes should be discarded or not.
        /// </summary>
        /// <value>
        ///   <c>true</c> if null bytes should be discarded; otherwise, <c>false</c>.
        /// </value>
        public bool DiscardNull
        {
            get
            {
                bool discardNull;
                if (m_Dll.serial_getdiscardnull(m_Handle, out discardNull) == -1) ThrowException();
                return discardNull;
            }
            set
            {
                if (m_Dll.serial_setdiscardnull(m_Handle, value) == -1) ThrowException();
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
                int parityReplace;
                if (m_Dll.serial_getparityreplace(m_Handle, out parityReplace) == -1) ThrowException();
                return (byte)parityReplace;
            }
            set
            {
                if (m_Dll.serial_setparityreplace(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether transmission should still
        /// be sent when the input buffer is full and if the XOff character has been sent.
        /// </summary>
        /// <value>
        ///   <c>true</c> if transmission should continue after the input buffer
        ///   is within <see cref="XOffLimit"/> bytes of being full and the driver has sent the
        ///   XOff character; otherwise, <c>false</c> that transmission should stop
        ///   and only continue when the input buffer is within <see cref="XOnLimit"/> bytes of
        ///   being empty and the driver has sent the XOn character.
        /// </value>
        public bool TxContinueOnXOff
        {
            get
            {
                bool txContinueOnXOff;
                if (m_Dll.serial_gettxcontinueonxoff(m_Handle, out txContinueOnXOff) == -1) ThrowException();
                return txContinueOnXOff;
            }
            set
            {
                if (m_Dll.serial_settxcontinueonxoff(m_Handle, value) == -1) ThrowException();
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
                int xoffLimit;
                if (m_Dll.serial_getxofflimit(m_Handle, out xoffLimit) == -1) ThrowException();
                return xoffLimit;
            }
            set
            {
                if (m_Dll.serial_setxofflimit(m_Handle, value) == -1) ThrowException();
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
                int xonLimit;
                if (m_Dll.serial_getxonlimit(m_Handle, out xonLimit) == -1) ThrowException();
                return xonLimit;
            }
            set
            {
                if (m_Dll.serial_setxonlimit(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the break state of the serial port.
        /// </summary>
        /// <value>
        ///   <c>true</c> if in the break state; otherwise, <c>false</c>.
        /// </value>
        public bool BreakState
        {
            get
            {
                bool breakState;
                if (m_Dll.serial_getbreak(m_Handle, out breakState) == -1) ThrowException();
                return breakState;
            }
            set
            {
                if (m_Dll.serial_setbreak(m_Handle, value) == -1) ThrowException();
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
            set { }
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
            set { }
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
        ///   <c>true</c> if carrier detect pin is active; otherwise, <c>false</c>.
        /// </value>
        public bool CDHolding
        {
            get
            {
                bool cdHolding;
                if (m_Dll.serial_getdcd(m_Handle, out cdHolding) == -1) ThrowException();
                return cdHolding;
            }
        }

        /// <summary>
        /// Gets the state of the Clear To Send pin on the serial port.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the clear to send pin is active; otherwise, <c>false</c>.
        /// </value>
        public bool CtsHolding
        {
            get
            {
                bool ctsHolding;
                if (m_Dll.serial_getcts(m_Handle, out ctsHolding) == -1) ThrowException();
                return ctsHolding;
            }
        }

        /// <summary>
        /// Gets the state of the Data Set Ready pin on the serial port.
        /// </summary>
        /// <value>
        ///   <c>true</c> if data set ready pin is active; otherwise, <c>false</c>.
        /// </value>
        public bool DsrHolding
        {
            get
            {
                bool dsrHolding;
                if (m_Dll.serial_getdsr(m_Handle, out dsrHolding) == -1) ThrowException();
                return dsrHolding;
            }
        }

        /// <summary>
        /// Gets the state of the Ring Indicator pin on the serial port.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ring indicator state is active; otherwise, <c>false</c>.
        /// </value>
        public bool RingHolding
        {
            get
            {
                bool ringHolding;
                if (m_Dll.serial_getri(m_Handle, out ringHolding) == -1) ThrowException();
                return ringHolding;
            }
        }

        /// <summary>
        /// Gets or sets the Data Terminal Ready pin of the serial port.
        /// </summary>
        /// <value>
        ///   <c>true</c> if data terminal pin is active; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This pin only has an effect if handshaking for DTR/DTS is disabled.
        /// </remarks>
        public bool DtrEnable
        {
            get
            {
                bool dtrEnable;
                if (m_Dll.serial_getdtr(m_Handle, out dtrEnable) == -1) ThrowException();
                return dtrEnable;
            }
            set
            {
                if (m_Dll.serial_setdtr(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets or sets the Request To Send pin of the serial port.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [RTS enable]; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This pin only has an effect if the handshaking for RTS/CTS is disabled.
        /// </remarks>
        public bool RtsEnable
        {
            get
            {
                bool rtsEnable;
                if (m_Dll.serial_getrts(m_Handle, out rtsEnable) == -1) ThrowException();
                return rtsEnable;
            }
            set
            {
                if (m_Dll.serial_setrts(m_Handle, value) == -1) ThrowException();
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
                Handshake handshake;
                if (m_Dll.serial_gethandshake(m_Handle, out handshake) == -1) ThrowException();
                return handshake;
            }
            set
            {
                if (m_Dll.serial_sethandshake(m_Handle, value) == -1) ThrowException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the serial port has been opened.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property only indicates if the port has been opened and that the
        /// internal handle is valid.
        /// </remarks>
        public bool IsOpen
        {
            get
            {
                bool isOpen;
                if (m_Dll.serial_isopen(m_Handle, out isOpen) == -1) ThrowException();
                return isOpen;
            }
        }

        /// <summary>
        /// Discards the input queue buffer of the driver.
        /// </summary>
        public void DiscardInBuffer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Discards the output queue buffer of the driver.
        /// </summary>
        public void DiscardOutBuffer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the port settings and updates the properties of the object.
        /// </summary>
        public void GetPortSettings()
        {
            if (m_Dll.serial_getproperties(m_Handle) == -1) ThrowException();
        }

        /// <summary>
        /// Writes the settings of the serial port as set in this object.
        /// </summary>
        public void SetPortSettings()
        {
            if (m_Dll.serial_setproperties(m_Handle) == -1) ThrowException();
        }

        /// <summary>
        /// Opens the serial port specified by <see cref="PortName"/>.
        /// </summary>
        /// <remarks>
        /// Opening the serial port does not set any settings (such as baud rate, etc.)
        /// </remarks>
        public void Open()
        {
            if (m_Dll.serial_open(m_Handle) == -1) ThrowException();
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
            if (m_Dll.serial_close(m_Handle) == -1) ThrowException();
        }

        /// <summary>
        /// Creates the serial buffer suitable for monitoring.
        /// </summary>
        /// <param name="readBuffer">The read buffer size to allocate.</param>
        /// <param name="writeBuffer">The write buffer size to allocate.</param>
        /// <returns>A serial buffer object that can be given to <see cref="StartMonitor"/></returns>
        public SerialBuffer CreateSerialBuffer(int readBuffer, int writeBuffer)
        {
            return new SerialBuffer(readBuffer, writeBuffer, false);
        }

        private SerialBuffer m_Buffer;
        private Thread m_MonitorThread;
        private Thread m_PinThread;
        private string m_Name;
        private volatile bool m_IsRunning;
        private volatile bool m_MonitorPins;
        private ManualResetEvent m_StopRunning = new ManualResetEvent(false);

        /// <summary>
        /// Start the monitor thread, that will watch over the serial port.
        /// </summary>
        /// <param name="buffer">The buffer structure that should be used to read data into
        /// and write data from.</param>
        /// <param name="name">The name of the thread to use.</param>
        public void StartMonitor(SerialBuffer buffer, string name)
        {
            if (m_IsDisposed) throw new ObjectDisposedException("WinNativeSerial");
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (!IsOpen) throw new InvalidOperationException("Serial Port not open");

            m_Buffer = buffer;
            m_Name = name;
            m_StopRunning.Reset();

            try {
                m_IsRunning = true;
                m_MonitorThread = new Thread(new ThreadStart(ReadWriteThread));
                m_MonitorThread.Name = "UnixNativeSerial::Monitor_" + m_Name;
                m_MonitorThread.IsBackground = true;
                m_MonitorThread.Start();
            } catch {
                m_IsRunning = false;
                throw;
            }

            try {
                m_MonitorPins = true;
                m_PinThread = new Thread(new ThreadStart(PinChangeThread));
                m_PinThread.Name = "UnixNativeSerial::Pin_" + m_Name;
                m_PinThread.IsBackground = true;
                m_PinThread.Start();
            } catch {
                m_MonitorPins = false;
                throw;
            }
        }

        private void Stop()
        {
            SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "{0}: ReadWriteThread: Stopping Thread", m_Name);
            m_StopRunning.Set();
            InterruptReadWriteLoop();

            if (m_MonitorPins && m_PinThread != null) {
                int killcounter = 0;

                SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "{0}: PinChangeThread: Stopping Thread", m_Name);
                m_Dll.serial_abortwaitformodemevent(m_Handle);
                SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "{0}: PinChangeThread: Waiting for Thread", m_Name);
                while (killcounter < 3 && !m_PinThread.Join(100)) {
                    SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "{0}: PinChangeThread: Waiting for Thread, counter={1}", m_Name, killcounter);
                    killcounter++;
                }
                SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "{0}: PinChangeThread: Thread Stopped", m_Name);
                m_PinThread = null;
                m_MonitorPins = false;
            }

            if (m_MonitorThread != null) {
                SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "{0}: ReadWriteThread: Waiting for Thread", m_Name);
                m_MonitorThread.Join();
                SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "{0}: ReadWriteThread: Thread Stopped", m_Name);
                m_MonitorThread = null;
            }
        }

        private unsafe void ReadWriteThread()
        {
            WaitHandle[] handles = new WaitHandle[] {
                m_StopRunning,
                m_Buffer.Serial.ReadBufferNotFull,
                m_Buffer.Serial.WriteBufferNotEmpty
            };
            m_Buffer.WriteEvent += SerialBufferWriteEvent;

            while (m_IsRunning) {
                SerialReadWriteEvent rwevent = SerialReadWriteEvent.NoEvent;

                int handle = WaitHandle.WaitAny(handles, -1);
                switch (handle) {
                case 0: // StopRunning - Should abort
                    m_IsRunning = false;
                    continue;
                }

                // These are not in the switch statement to ensure that we can actually
                // read/write simultaneously.
                if (m_Buffer.Serial.ReadBufferNotFull.WaitOne(0)) {
                    rwevent |= SerialReadWriteEvent.ReadEvent;
                }
                if (m_Buffer.Serial.WriteBufferNotEmpty.WaitOne(0)) {
                    rwevent |= SerialReadWriteEvent.WriteEvent;
                }

                SerialReadWriteEvent result = m_Dll.serial_waitforevent(m_Handle, rwevent, 500);
                if (result == SerialReadWriteEvent.Error) {
                    SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Error, 0,
                        "{0}: ReadWriteThread: Error waiting for event; errno={1}; description={2}",
                        m_Name, m_Dll.errno, m_Dll.serial_error(m_Handle));
                    m_IsRunning = false;
                    continue;
                } else {
                    SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0,
                        "{0}: ReadWriteThread: serial_waitforevent({1}, {2}) == {3}",
                        m_Name, m_Handle, rwevent, result);
                }

                if (result.HasFlag(SerialReadWriteEvent.ReadEvent)) {
                    int rresult;
                    fixed (byte* b = m_Buffer.Serial.ReadBuffer.Array) {
                        byte* bo = b + m_Buffer.Serial.ReadBuffer.End;
                        int length = m_Buffer.Serial.ReadBuffer.WriteLength;
                        rresult = m_Dll.serial_read(m_Handle, (IntPtr)bo, length);
                        if (rresult == -1) {
                            SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Error, 0,
                                "{0}: ReadWriteThread: Error reading data; errno={1}; description={2}",
                                m_Name, m_Dll.errno, m_Dll.serial_error(m_Handle));
                            m_IsRunning = false;
                            continue;
                        } else {
                            SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0,
                                "{0}: ReadWriteThread: serial_read({1}, {2}, {3}) == {4}",
                                m_Name, m_Handle, (IntPtr)bo, length, rresult);
                            m_Buffer.Serial.ReadBufferProduce(rresult);
                        }
                    }
                    if (rresult > 0) OnDataReceived(this, new SerialDataReceivedEventArgs(SerialData.Chars));
                }

                if (result.HasFlag(SerialReadWriteEvent.WriteEvent)) {
                    int wresult;
                    fixed (byte * b = m_Buffer.Serial.WriteBuffer.Array) {
                        byte* bo = b + m_Buffer.Serial.WriteBuffer.Start;
                        int length = m_Buffer.Serial.WriteBuffer.ReadLength;
                        wresult = m_Dll.serial_write(m_Handle, (IntPtr)bo, length);
                        if (wresult == -1) {
                            SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Error, 0,
                                "{0}: ReadWriteThread: Error writing data; errno={1}; description={2}",
                                m_Name, m_Dll.errno, m_Dll.serial_error(m_Handle));
                            m_IsRunning = false;
                            continue;
                        } else {
                            SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0,
                                "{0}: ReadWriteThread: serial_write({1}, {2}, {3}) == {4}",
                                m_Name, m_Handle, (IntPtr)bo, length, wresult);
                            m_Buffer.Serial.WriteBufferConsume(wresult);
                            m_Buffer.Serial.TxEmptyEvent();
                        }
                    }
                }
            }
            m_Buffer.WriteEvent -= SerialBufferWriteEvent;

            // Clear the write buffer. Anything that's still in the driver serial buffer will continue to write. The I/O was cancelled
            // so no need to purge the actual driver itself.
            m_Buffer.Serial.Purge();

            // We must notify the stream that any blocking waits should abort.
            m_Buffer.Serial.DeviceDead();
        }

        private void SerialBufferWriteEvent(object sender, EventArgs e)
        {
            InterruptReadWriteLoop();
        }

        private void InterruptReadWriteLoop()
        {
            if (m_IsRunning && !m_Handle.Equals(IntPtr.Zero)) {
                int result = m_Dll.serial_abortwaitforevent(m_Handle);
                if (result == -1) {
                    SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Error, 0,
                        "{0}: ReadWriteThread: Error aborting event; errno={1}; description={2}",
                        m_Name, m_Dll.errno, m_Dll.serial_error(m_Handle));
                } else {
                    SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0,
                        "{0}: ReadWriteThread: serial_abortwaitforevent({1}) = {2}",
                        m_Name, m_Handle, result);
                }
            }
        }

        private const WaitForModemEvent c_ModemEvents =
            WaitForModemEvent.RingIndicator |
            WaitForModemEvent.ClearToSend |
            WaitForModemEvent.DataCarrierDetect |
            WaitForModemEvent.DataSetReady;

        private void PinChangeThread()
        {
            while (m_MonitorPins) {
                if (m_StopRunning.WaitOne(0)) {
                    m_MonitorPins = false;
                    continue;
                }

                SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0,
                    "{0}: PinChangeThread: Waiting", m_Name);
                WaitForModemEvent mevent = m_Dll.serial_waitformodemevent(m_Handle, c_ModemEvents);
                if (mevent == WaitForModemEvent.Error) {
                    SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Error, 0,
                        "{0}: PinChangeThread: Error aborting event; errno={1}; description={2}",
                        m_Name, m_Dll.errno, m_Dll.serial_error(m_Handle));
                    m_MonitorPins = false;
                    return;
                }

                if (mevent != WaitForModemEvent.None) {
                    SerialPinChange pins = SerialPinChange.NoChange;
                    if (mevent.HasFlag(WaitForModemEvent.ClearToSend)) pins |= SerialPinChange.CtsChanged;
                    if (mevent.HasFlag(WaitForModemEvent.DataCarrierDetect)) pins |= SerialPinChange.CDChanged;
                    if (mevent.HasFlag(WaitForModemEvent.DataSetReady)) pins |= SerialPinChange.DsrChanged;
                    if (mevent.HasFlag(WaitForModemEvent.RingIndicator)) pins |= SerialPinChange.Ring;
                    // TODO: Break not implemented

                    SerialTrace.TraceSer.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0,
                        "{0}: PinChangeThread: Event Received: {1}", m_Name, mevent);
                    OnPinChanged(this, new SerialPinChangedEventArgs(pins));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the thread for monitoring the serial port is running.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property differs slightly from <see cref="IsOpen"/>, as this returns status if
        /// the monitoring thread for reading/writing data is actually running. If the thread is
        /// not running for whatever reason, we can expect no data updates in the buffer provided
        /// to <see cref="StartMonitor(SerialBuffer, string)"/>.
        /// </remarks>
        public bool IsRunning { get { return m_IsRunning; } }

        /// <summary>
        /// Occurs when data is received, or the EOF character is detected by the driver.
        /// </summary>
        public event EventHandler<SerialDataReceivedEventArgs> DataReceived;

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
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources;
        /// <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_IsDisposed) {
                if (disposing) {
                    if (IsOpen) Close();
                }

                if (!m_Handle.Equals(IntPtr.Zero)) {
                    m_Dll.serial_terminate(m_Handle);
                    m_Handle = IntPtr.Zero;
                    m_Dll = null;
                }
                m_StopRunning.Dispose();
                m_StopRunning = null;
                m_IsDisposed = true;
            }
        }
        #endregion
    }
}
