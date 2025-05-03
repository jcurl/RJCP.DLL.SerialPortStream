namespace RJCP.IO.Ports.Serial.Windows
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Threading;
    using Datastructures;
    using Microsoft.Win32.SafeHandles;
    using Native.Win32;
    using RJCP.Diagnostics.Trace;

    [SupportedOSPlatform("windows")]
    internal class CommOverlappedIo : IDisposable
    {
        private const uint MaxWriteBytes = 8192;

        #region Local variables
        /// <summary>
        /// Handle to the already opened COM Port.
        /// </summary>
        private readonly SafeFileHandle m_ComPortHandle;

        /// <summary>
        /// The OverlappedIoThread.
        /// </summary>
        private Thread m_Thread;

        /// <summary>
        /// Read and Write buffers that are pinned.
        /// </summary>
        private SerialBuffer m_Buffer;

        /// <summary>
        /// Event to abort OverlappedIoThread (for OverlappedIoThread).
        /// </summary>
        private readonly ManualResetEvent m_StopRunning = new(false);

        /// <summary>
        /// Overlapped I/O for WaitCommEvent() finished (for OverlappedIoThread).
        /// </summary>
        private readonly ManualResetEvent m_SerialCommEvent = new(false);

        /// <summary>
        /// Overlapped I/O for ReadFile() finished (for OverlappedIoThread).
        /// </summary>
        private readonly ManualResetEvent m_ReadEvent = new(false);

        /// <summary>
        /// Overlapped I/O for WriteFile() finished (for OverlappedIoThread).
        /// </summary>
        private readonly ManualResetEvent m_WriteEvent = new(false);

        /// <summary>
        /// Triggered to indicate to purge data in the write buffer by the OverlappedIO thread.
        /// </summary>
        private readonly AutoResetEvent m_WriteClearEvent = new(false);

        /// <summary>
        /// Triggered when the purge is complete from the OverlappedIO thread.
        /// </summary>
        private readonly AutoResetEvent m_WriteClearDoneEvent = new(false);

        /// <summary>
        /// Used by the OverlappedIO thread to finalise a purge of the write buffer if a write
        /// operation was previously pending.
        /// </summary>
        private bool m_PurgePending;

        /// <summary>
        /// Indicates if the OverlappedIO thread is running.
        /// </summary>
        private volatile bool m_IsRunning;

        /// <summary>
        /// Indicates that there is a byte available for reading.
        /// </summary>
        /// <remarks>
        /// The WaitCommEvent() method indicates if a byte has been received (EV_RXCHAR). This
        /// is cleared when the read operation is finished.
        /// </remarks>
        private bool m_ReadByteAvailable;

        /// <summary>
        /// Indicates that there is a EOF character that has arrived.
        /// </summary>
        /// <remarks>
        /// The WaitCommEvent() method indicates if a byte has been received (RX_CHAR) covered
        /// by the variable m_ReadByteAvailable. If m_ReadByteAvailable, this indicates if a
        /// the EOF character has arrived (EV_RXFLAG).
        /// </remarks>
        private bool m_ReadByteEof;

        /// <summary>
        /// Configuration settings specific for Windows.
        /// </summary>
        private readonly WinNativeSettings m_Settings;

        private readonly LogSource m_Log;

        private string m_Name;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CommOverlappedIo"/> class.
        /// </summary>
        /// <param name="handle">The serial port handle.</param>
        /// <param name="settings">Settings that should be used for Windows serial ports.</param>
        /// <param name="log">The <see cref="LogSource"/> which should be used for logging.</param>
        public CommOverlappedIo(SafeFileHandle handle, WinNativeSettings settings, LogSource log)
        {
            ThrowHelper.ThrowIfNull(log);
            ThrowHelper.ThrowIfNull(settings);

            m_Log = log;
            m_ComPortHandle = handle;
            m_Settings = settings;
        }
        #endregion

        #region Buffer Management
        /// <summary>
        /// Gets the number of bytes in the driver queue still to be read.
        /// </summary>
        /// <value>
        /// The bytes to read.
        /// </value>
        public int BytesToRead
        {
            get
            {
                if (!IsRunning) return 0;

                if (GetReceiveStats(out uint bytesInRecvQueue, out _)) {
                    return (int)bytesInRecvQueue;
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets the status of bytes received by the serial provider that haven't been read yet. If there is
        /// a failure in obtaining the information, zero is returned.
        /// </summary>
        /// <param name="bytesInRecvQueue">Output indicating number of bytes in queue but not read by ReadFile.</param>
        /// <param name="eofReceived">Output indicating whether an EOF character was received.</param>
        /// <returns><see langword="true"/> if the stats were received, otherwise <see langword="false"/>.</returns>
        /// <remarks>
        /// Getting this information has the side effect of processing and clearing any serial port
        /// errors and firing CommErrorEvent.
        /// </remarks>
        private bool GetReceiveStats(out uint bytesInRecvQueue, out bool eofReceived)
        {
            bytesInRecvQueue = 0;
            eofReceived = false;

            bool result = Kernel32.ClearCommError(m_ComPortHandle, out Kernel32.ComStatErrors cErr, out Kernel32.COMSTAT comStat);
            if (!result) {
                int w32err = Marshal.GetLastWin32Error();
                int hr = Marshal.GetHRForLastWin32Error();
                if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Error))
                    m_Log.TraceEvent(System.Diagnostics.TraceEventType.Error,
                        $"{m_Name}: SerialThread: BytesInSerialQueue: ClearCommError() error {w32err}");
                Marshal.ThrowExceptionForHR(hr);
                return false;
            }
            if (cErr != 0) OnCommErrorEvent(new CommErrorEventArgs(cErr));
            bytesInRecvQueue = comStat.cbInQue;
            eofReceived = ((comStat.Flags & Kernel32.ComStatFlags.Eof) == Kernel32.ComStatFlags.Eof);
            return true;
        }

        /// <summary>
        /// Gets the number bytes to of data in the transmit buffer.
        /// </summary>
        /// <value>
        /// The number of bytes in the transmit buffer.
        /// </value>
        public int BytesToWrite
        {
            get
            {
                bool result = Kernel32.ClearCommError(m_ComPortHandle, out _, out Kernel32.COMSTAT comStat);
                if (result) return (int)comStat.cbOutQue;
                return 0;
            }
        }

        /// <summary>
        /// Discards data from the serial driver's transmit buffer.
        /// </summary>
        /// <remarks>
        /// This function will discard the transmit buffer of the SerialPortStream.
        /// </remarks>
        public void DiscardOutBuffer()
        {
            if (IsRunning) {
                m_WriteClearEvent.Set();
                m_WriteClearDoneEvent.WaitOne(Timeout.Infinite);
            }
        }
        #endregion

        #region Thread Control
        /// <summary>
        /// Start the I/O thread.
        /// </summary>
        public void Start(SerialBuffer buffer, string name)
        {
            m_Buffer = buffer;
            m_Name = name;
            m_IsRunning = true;
            try {
                // Set the time outs
                Kernel32.COMMTIMEOUTS timeouts = new() {
                    ReadIntervalTimeout = m_Settings.ReadIntervalTimeout,
                    ReadTotalTimeoutConstant = m_Settings.ReadTotalTimeoutConstant,
                    ReadTotalTimeoutMultiplier = m_Settings.ReadTotalTimeoutMultiplier,
                    WriteTotalTimeoutMultiplier = m_Settings.WriteTotalTimeoutMultiplier,
                    WriteTotalTimeoutConstant = m_Settings.WriteTotalTimeoutConstant
                };

                bool result = Kernel32.SetCommTimeouts(m_ComPortHandle, ref timeouts);
                if (!result) throw new IOException("Couldn't set CommTimeouts", Marshal.GetLastWin32Error());

                m_Thread = new Thread(new ThreadStart(OverlappedIoThread)) {
                    Name = "SerialPortStream_" + m_Name,
                    IsBackground = true
                };
                m_Thread.Start();
            } catch {
                m_IsRunning = false;
                throw;
            }
        }

        /// <summary>
        /// Cancel pending I/O, stop the I/O thread, wait and then return.
        /// </summary>
        private void Stop()
        {
            if (m_Thread is not null) {
                if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                    m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose, $"{m_Name}: OverlappedIO: Stopping Thread");
                m_StopRunning.Set();
                if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                    m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose, $"{m_Name}: OverlappedIO: Waiting for Thread");
                m_Thread.Join();
                if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                    m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose, $"{m_Name}: OverlappedIO: Thread Stopped");
                m_Thread = null;
            }
        }

        /// <summary>
        /// Test if the I/O thread is running.
        /// </summary>
        public bool IsRunning { get { return m_IsRunning; } }
        #endregion

        private const Kernel32.SerialEventMask maskRead =
            Kernel32.SerialEventMask.EV_BREAK |
            Kernel32.SerialEventMask.EV_CTS |
            Kernel32.SerialEventMask.EV_DSR |
            Kernel32.SerialEventMask.EV_ERR |
            Kernel32.SerialEventMask.EV_RING |
            Kernel32.SerialEventMask.EV_RLSD |
            Kernel32.SerialEventMask.EV_RXCHAR |
            Kernel32.SerialEventMask.EV_RXFLAG;

        private const Kernel32.SerialEventMask maskReadPending =
            Kernel32.SerialEventMask.EV_BREAK |
            Kernel32.SerialEventMask.EV_CTS |
            Kernel32.SerialEventMask.EV_DSR |
            Kernel32.SerialEventMask.EV_ERR |
            Kernel32.SerialEventMask.EV_RING |
            Kernel32.SerialEventMask.EV_RLSD;

        private void OverlappedIoThread()
        {
            try {
                OverlappedIoThreadMainLoop();
            } catch (Exception ex) {
                if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Error))
                    m_Log.TraceEvent(System.Diagnostics.TraceEventType.Error,
                        $"{m_Name}: SerialThread: Died from {ex.Message}");
            } finally {
                m_IsRunning = false;

                // We must notify the stream that any blocking waits should abort.
                m_Buffer.Close();
            }
        }

        private void OverlappedIoThreadMainLoop()
        {
            // WaitCommEvent
            bool serialCommPending = false;
            bool serialCommError = false;
            m_SerialCommEvent.Reset();
            NativeOverlapped serialCommOverlapped = new();
#if NET6_0_OR_GREATER
            serialCommOverlapped.EventHandle = m_SerialCommEvent.GetSafeWaitHandle().DangerousGetHandle();
#else
            serialCommOverlapped.EventHandle = m_SerialCommEvent.SafeWaitHandle.DangerousGetHandle();
#endif
            // ReadFile
            bool readPending = false;
            m_ReadEvent.Reset();
            NativeOverlapped readOverlapped = new();
#if NET6_0_OR_GREATER
            readOverlapped.EventHandle = m_ReadEvent.GetSafeWaitHandle().DangerousGetHandle();
#else
            readOverlapped.EventHandle = m_ReadEvent.SafeWaitHandle.DangerousGetHandle();
#endif
            // WriteFile
            bool writePending = false;
            m_WriteEvent.Reset();
            NativeOverlapped writeOverlapped = new();
            m_ReadByteAvailable = false;
#if NET6_0_OR_GREATER
            writeOverlapped.EventHandle = m_WriteEvent.GetSafeWaitHandle().DangerousGetHandle();
#else
            writeOverlapped.EventHandle = m_WriteEvent.SafeWaitHandle.DangerousGetHandle();
#endif
            // SEt up the types of serial events we want to see.
            Kernel32.SetCommMask(m_ComPortHandle, maskRead);

            bool result;
            Kernel32.SerialEventMask commEventMask = 0;

            bool running = true;
            uint bytes;
            ReusableList<WaitHandle> handles = new(2, 7);

            while (running) {
                handles.Clear();
                handles.Add(m_StopRunning);
                handles.Add(m_WriteClearEvent);

                // - - - - - - - - - - - - - - - - - - - - - - - - -
                // PROLIFIC PL23030 WORKAROUND
                // - - - - - - - - - - - - - - - - - - - - - - - - -
                // If we have a read pending, we don't request events
                // for reading data. To do so will result in errors.
                // Have no idea why.
                if (readPending) {
                    Kernel32.SetCommMask(m_ComPortHandle, maskReadPending);
                } else {
                    Kernel32.SetCommMask(m_ComPortHandle, maskRead);

                    // While the comm event mask was set to ignore read events, data could have been written
                    // to the input queue. Check for that and if there are bytes waiting or EOF was received,
                    // set the appropriate flags.
                    if (GetReceiveStats(out uint bytesInQueue, out bool eofReceived) && (bytesInQueue > 0 || eofReceived)) {
                        // Tell DoReadEvent that there is data pending
                        m_ReadByteAvailable = true;
                        m_ReadByteEof |= eofReceived;
                    }
                }

                // commEventMask is on the stack, and is therefore fixed
                if (!serialCommError) {
                    try {
                        if (!serialCommPending)
                            serialCommPending = DoWaitCommEvent(out commEventMask, ref serialCommOverlapped);
                        if (serialCommPending)
                            handles.Add(m_SerialCommEvent);
                    } catch (IOException) {
                        // Some devices, such as the Arduino Uno with a CH340 on board don't support an overlapped
                        // WaitCommEvent. So if that occurs, we remember it and don't use it again. The Windows error
                        // returned was 87 (ERROR_INVALID_PARAMETER) was returned in that case. GetReceiveStats() did
                        // work, so we can still know of data pending by polling. But we won't get any other events,
                        // such as TX_EMPTY.
                        if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Warning))
                            m_Log.TraceEvent(System.Diagnostics.TraceEventType.Warning,
                                $"{m_Name}: SerialThread: Not processing WaitCommEvent events");
                        serialCommError = true;
                    }
                }

                if (!readPending) {
                    if (!m_Buffer.SerialRead.IsBufferNotFull) {
                        if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                            m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose, $"{m_Name}: SerialThread: Read Buffer Full");
                        handles.Add(m_Buffer.SerialRead.BufferNotFull);
                    } else {
                        readPending = DoReadEvent(ref readOverlapped);
                    }
                }
                if (readPending) handles.Add(m_ReadEvent);

                if (!writePending) {
                    if (!m_Buffer.SerialWrite.IsBufferNotEmpty) {
                        handles.Add(m_Buffer.SerialWrite.BufferNotEmpty);
                    } else {
                        writePending = DoWriteEvent(ref writeOverlapped);
                    }
                }
                if (writePending) handles.Add(m_WriteEvent);

                // We wait up to 100ms, in case we're not actually pending on anything. Normally, we should always be
                // pending on a Comm event. Just in case this is not so (and is a theoretical possibility), we will
                // slip out of this WaitAny() after 100ms and then restart the loop, effectively polling every 100ms in
                // worst case.
                WaitHandle[] whandles = handles.ToArray();
                int ev = WaitHandle.WaitAny(whandles, 100);

                if (ev != WaitHandle.WaitTimeout) {
                    if (whandles[ev] == m_StopRunning) {
                        if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                            m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose, $"{m_Name}: SerialThread: Thread closing");
                        result = Kernel32.CancelIo(m_ComPortHandle);
                        if (!result) {
                            int win32Error = Marshal.GetLastWin32Error();
                            int hr = Marshal.GetHRForLastWin32Error();
                            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Warning))
                                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Warning,
                                    $"{m_Name}: SerialThread: CancelIo error {win32Error}");
                            Marshal.ThrowExceptionForHR(hr);
                        }
                        running = false;
                    } else if (whandles[ev] == m_SerialCommEvent) {
                        result = Kernel32.GetOverlappedResult(m_ComPortHandle, ref serialCommOverlapped, out bytes, true);
                        if (!result) {
                            int win32Error = Marshal.GetLastWin32Error();
                            int hr = Marshal.GetHRForLastWin32Error();
                            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Error))
                                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Error,
                                    $"{m_Name}: SerialThread: Overlapped WaitCommEvent() error {win32Error}");
                            Marshal.ThrowExceptionForHR(hr);
                        }
                        ProcessWaitCommEvent(commEventMask);
                        serialCommPending = false;
                    } else if (whandles[ev] == m_ReadEvent) {
                        result = Kernel32.GetOverlappedResult(m_ComPortHandle, ref readOverlapped, out bytes, true);
                        if (!result) {
                            ProcessReadEventError(bytes);
                        } else {
                            ProcessReadEvent(bytes);
                        }
                        readPending = false;
                    } else if (whandles[ev] == m_Buffer.SerialRead.BufferNotFull) {
                        // The read buffer is no longer full. We just loop back to the beginning to test if we
                        // should read or not.
                    } else if (whandles[ev] == m_WriteEvent) {
                        result = Kernel32.GetOverlappedResult(m_ComPortHandle, ref writeOverlapped, out bytes, true);
                        if (!result) {
                            ProcessWriteEventError(bytes);
                        } else {
                            ProcessWriteEvent(bytes);
                        }
                        writePending = false;
                    } else if (whandles[ev] == m_Buffer.SerialWrite.BufferNotEmpty) {
                        // The write buffer is no longer empty. We just loop back to the beginning to test if we
                        // should write or not.
                    } else if (whandles[ev] == m_WriteClearEvent) {
                        if (writePending) {
                            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose, $"{m_Name}: SerialThread: PurgeComm() write pending");
                            m_PurgePending = true;
                            result = Kernel32.PurgeComm(m_ComPortHandle,
                                Kernel32.PurgeFlags.PURGE_TXABORT | Kernel32.PurgeFlags.PURGE_TXCLEAR);
                            if (!result) ProcessPurgeCommError();
                        } else {
                            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose, $"{m_Name}: SerialThread: Purged");
                            m_Buffer.SerialWrite.Purge();
                            m_WriteClearDoneEvent.Set();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if we should execute WaitCommEvent() and get the result if immediately available.
        /// </summary>
        /// <remarks>
        /// This function abstracts the Win32 API WaitCommEvent(). It assumes overlapped I/O.
        /// Therefore, when calling this function, you should ensure that the parameter <c>mask</c>
        /// and <c>overlap</c> are pinned for the duration of the overlapped I/O. Any easy way to
        /// do this is to allocate the variables on the stack and then pass them by reference to
        /// this function.
        /// <para>You should not call this function if a pending I/O operation for WaitCommEvent()
        /// is still open. It is an error otherwise.</para>
        /// </remarks>
        /// <param name="mask">The mask value if information is available immediately.</param>
        /// <param name="overlap">The overlap structure to use.</param>
        /// <returns>If the operation is pending or not.</returns>
        private bool DoWaitCommEvent(out Kernel32.SerialEventMask mask, ref NativeOverlapped overlap)
        {
            bool result = Kernel32.WaitCommEvent(m_ComPortHandle, out mask, ref overlap);
            if (!result) {
                int w32err = Marshal.GetLastWin32Error();
                int hr = Marshal.GetHRForLastWin32Error();
                if (w32err != WinError.ERROR_IO_PENDING) {
                    if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Error))
                        m_Log.TraceEvent(System.Diagnostics.TraceEventType.Error,
                            $"{m_Name}: SerialThread: DoWaitCommEvent: Result: {w32err}");
                    throw new IOException("WaitCommEvent overlapped exception", hr);
                }
            } else {
                ProcessWaitCommEvent(mask);
            }
            return !result;
        }

        /// <summary>
        /// Do work based on the mask event that has occurred.
        /// </summary>
        /// <param name="mask">The mask that was provided.</param>
        private void ProcessWaitCommEvent(Kernel32.SerialEventMask mask)
        {
            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose)) {
                if (mask != 0) {
                    m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                        $"{m_Name}: SerialThread: ProcessWaitCommEvent: {mask}");
                }
            }

            // Reading a character
            if ((mask & Kernel32.SerialEventMask.EV_RXCHAR) != 0) {
                m_ReadByteAvailable = true;
            }
            if ((mask & Kernel32.SerialEventMask.EV_RXFLAG) != 0) {
                m_ReadByteAvailable = true;
                m_ReadByteEof = true;
            }

            // We don't raise an event for characters immediately, but only after the read operation
            // is complete.
            OnCommEvent(new CommEventArgs(mask & ~(Kernel32.SerialEventMask.EV_RXCHAR | Kernel32.SerialEventMask.EV_RXFLAG)));

            if ((mask & (Kernel32.SerialEventMask.EV_RXCHAR | Kernel32.SerialEventMask.EV_ERR)) != 0) {
                bool result = Kernel32.ClearCommError(m_ComPortHandle, out Kernel32.ComStatErrors comErr, IntPtr.Zero);
                if (!result) {
                    int w32err = Marshal.GetLastWin32Error();
                    int hr = Marshal.GetHRForLastWin32Error();
                    if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                        m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                            $"{m_Name}: SerialThread: ClearCommError: WINERROR {w32err}");
                    Marshal.ThrowExceptionForHR(hr);
                } else {
                    comErr = (Kernel32.ComStatErrors)((int)comErr & 0x10F);
                    if (comErr != 0) {
                        OnCommErrorEvent(new CommErrorEventArgs(comErr));
                    }
                }
            }
        }

        /// <summary>
        /// Check if we should ReadFile() and process the data if serial data is immediately.
        /// available.
        /// </summary>
        /// <remarks>
        /// This function should be called if there is no existing pending read operation. It
        /// will check if there is data to read (indicated by the variable m_ReadByteAvailable,
        /// which is set by ProcessWaitCommEvent()) and then issue a ReadFile(). If the result
        /// indicates that asynchronous I/O is happening, <b>true</b> is returned. Else this
        /// function automatically calls ProcessReadEvent() with the number of bytes read. If
        /// an asynchronous operation is pending, then you should wait on the event in the
        /// overlapped structure not call this function until GetOverlappedResult() has
        /// been called.
        /// </remarks>
        /// <param name="overlap">The overlap structure to use for reading.</param>
        /// <returns>If the operation is pending or not.</returns>
        private bool DoReadEvent(ref NativeOverlapped overlap)
        {
            // If WaitCommEvent() hasn't been called, there's no data
            if (!m_ReadByteAvailable) return false;

            // Read Buffer is full, so can't write into it.
            if (!m_Buffer.SerialRead.IsBufferNotFull) return false;

            // As C# can't convert an offset in the array to a pointer, we have to do
            // our own marshalling with (IntPtr)ReadBufferOffsetEnd that is the address
            // at the end of the read buffer.
            IntPtr bufPtr;
            uint bufLen;
            lock (m_Buffer.SerialRead.Lock) {
                bufPtr = m_Buffer.SerialRead.BufferPtr;
                bufLen = (uint)m_Buffer.SerialRead.BufferWriteLength;
            }

            bool result = Kernel32.ReadFile(m_ComPortHandle, bufPtr, bufLen, out uint bufRead, ref overlap);
            int w32err = Marshal.GetLastWin32Error();
            int hr = Marshal.GetHRForLastWin32Error();
            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                    $"{m_Name}: SerialThread: DoReadEvent: ReadFile({m_ComPortHandle.DangerousGetHandle()}, {bufPtr}, {bufLen}) == {result}");
            if (!result) {
                if (w32err == WinError.ERROR_OPERATION_ABORTED) {
                    if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Information))
                        m_Log.TraceEvent(System.Diagnostics.TraceEventType.Information,
                           $"{m_Name}: SerialThread: DoReadEvent: ReadFile() error {w32err}");
                    return false;
                }
                if (w32err != WinError.ERROR_IO_PENDING) {
                    if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Error))
                        m_Log.TraceEvent(System.Diagnostics.TraceEventType.Error,
                            $"{m_Name}: SerialThread: DoReadEvent: ReadFile() error {w32err}");
                    // In case an unexpected error occurs here, we kill the thread and indicate an error.
                    // One reason for this happening is a device error, or the device is removed from the
                    // system.
                    Marshal.ThrowExceptionForHR(hr);
                }
            } else {
                // MS Documentation for ReadFile() says that the 'bufRead' parameter should be NULL.
                // However, in the case that the COMMTIMEOUTS is set up so that no wait is required
                // (see COMMTIMEOUTS in Win32 API), this function will actually not perform an
                // asynchronous I/O operation and return the number of bytes copied in bufRead.
                ProcessReadEvent(bufRead);
            }
            return !result;
        }

        /// <summary>
        /// Produce the number of bytes read in the buffer.
        /// </summary>
        /// <remarks>
        /// If the number of bytes read is zero, this function should also be called, as it indicates
        /// that there are no more bytes pending. The recommendation from MS documentation for reading
        /// from the serial port indicates to read the buffer data, until a result of zero is given,
        /// which indicates to wait for the next receiving character.
        /// </remarks>
        /// <param name="bytes">Number of bytes read.</param>
        private void ProcessReadEvent(uint bytes)
        {
            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                    $"{m_Name}: SerialThread: ProcessReadEvent: {bytes} bytes");
            if (bytes == 0) {
                m_ReadByteAvailable = false;
            } else {
                lock (m_Buffer.SerialRead.Lock) {
                    if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose)) {
                        m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                            $"{m_Name}: SerialThread: ProcessReadEvent: End={m_Buffer.SerialRead.BufferEnd}; Bytes={bytes}");
                    }
                    m_Buffer.SerialRead.Produce((int)bytes);
                }

                OnCommEvent(new CommEventArgs((m_ReadByteEof ? Kernel32.SerialEventMask.EV_RXFLAG : 0) | Kernel32.SerialEventMask.EV_RXCHAR));
                m_ReadByteEof = false;
            }
        }

        private void ProcessReadEventError(uint bytes)
        {
            int win32Error = Marshal.GetLastWin32Error();
            int hr = Marshal.GetHRForLastWin32Error();

            // Should never get ERROR_IO_PENDING, as this method is only called when the event is triggered.
            if (bytes == 0) {
                switch (win32Error) {
                case WinError.ERROR_OPERATION_ABORTED:
                    // ERROR_OPERATION_ABORTED may be caused by CancelIo or PurgeComm
                    if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                        m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                        "{0}: SerialThread: Overlapped ReadFile() error {1} bytes {2}", m_Name, win32Error, bytes);
                    return;
                case WinError.ERROR_HANDLE_EOF:
                    if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Information))
                        m_Log.TraceEvent(System.Diagnostics.TraceEventType.Information,
                        "{0}: SerialThread: Overlapped ReadFile() error {1} bytes {2}", m_Name, win32Error, bytes);
                    return;
                }
            }
            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Error))
                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Error,
                "{0}: SerialThread: Overlapped ReadFile() error {1} bytes {2}", m_Name, win32Error, bytes);
            Marshal.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Check if we should WriteFile() and update buffers if serial data is immediately cached by driver.
        /// </summary>
        /// <remarks>
        /// This function should be called if there is no existing pending write operation. If
        /// the result indicates that asynchronous I/O is happening, <b>true</b> is returned.
        /// Else this function automatically calls ProcessWriteEvent() with the number of bytes
        /// written. If an asynchronous operation is pending, then you should wait on the event
        /// in the overlapped structure not call this function until GetOverlappedResult()
        /// has been called.
        /// </remarks>
        /// <param name="overlap">The overlap structure to use for writing.</param>
        /// <returns>If the operation is pending or not.</returns>
        private bool DoWriteEvent(ref NativeOverlapped overlap)
        {
            IntPtr bufPtr;
            uint bufLen;
            lock (m_Buffer.SerialWrite.Lock) {
                bufPtr = m_Buffer.SerialWrite.BufferPtr;
                bufLen = Math.Min(MaxWriteBytes, (uint)m_Buffer.SerialWrite.BufferReadLength);
            }

            bool result = Kernel32.WriteFile(m_ComPortHandle, bufPtr, bufLen, out uint bufWrite, ref overlap);
            int win32err = Marshal.GetLastWin32Error();
            int hr = Marshal.GetHRForLastWin32Error();
            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                    $"{m_Name}: SerialThread: DoWriteEvent: WriteFile({m_ComPortHandle.DangerousGetHandle()}, {bufPtr}, {bufLen}, ...) == {result}");
            if (!result) {
                if (win32err == WinError.ERROR_OPERATION_ABORTED) {
                    if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Information))
                        m_Log.TraceEvent(System.Diagnostics.TraceEventType.Information,
                            $"{m_Name}: SerialThread: DoWriteEvent: WriteFile() error {win32err}");
                    return false;
                }
                if (win32err != WinError.ERROR_IO_PENDING) {
                    if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Error))
                        m_Log.TraceEvent(System.Diagnostics.TraceEventType.Error,
                            $"{m_Name}: SerialThread: DoWriteEvent: WriteFile() error {win32err}");
                    // In case an unexpected error occurs here, we kill the thread and indicate an error.
                    // One reason for this happening is a device error, or the device is removed from the
                    // system.
                    Marshal.ThrowExceptionForHR(hr);
                }
            } else {
                ProcessWriteEvent(bufWrite);
            }
            return !result;
        }

        /// <summary>
        /// Consume the number of bytes written from the write buffer.
        /// </summary>
        /// <param name="bytes">Number of bytes written to the driver.</param>
        private void ProcessWriteEvent(uint bytes)
        {
            if (m_PurgePending) {
                m_PurgePending = false;
                if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                    m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                        $"{m_Name}: SerialThread: ProcessWriteEvent: {bytes} bytes - Purged");

                m_Buffer.SerialWrite.Purge();
                m_WriteClearDoneEvent.Set();
            } else {
                if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                    m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                        $"{m_Name}: SerialThread: ProcessWriteEvent: {bytes} bytes");
                if (bytes != 0) {
                    lock (m_Buffer.SerialWrite.Lock) {
                        m_Buffer.SerialWrite.Consume((int)bytes);
                        if (m_Buffer.SerialWrite.BufferReadLength == 0) {
                            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                                    $"{m_Name}: SerialThread: ProcessWriteEvent: TX-BUFFER empty");
                        }
                    }
                }
            }
        }

        private void ProcessWriteEventError(uint bytes)
        {
            int win32Error = Marshal.GetLastWin32Error();
            int hr = Marshal.GetHRForLastWin32Error();

            if (bytes == 0) {
                switch (win32Error) {
                case WinError.ERROR_OPERATION_ABORTED:
                    // ERROR_OPERATION_ABORTED may be caused by CancelIo or PurgeComm
                    if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                        m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                        "{0}: SerialThread: Overlapped WriteFile() error {1} bytes {2}", m_Name, win32Error, bytes);
                    return;
                }
            }
            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Error))
                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Error,
                "{0}: SerialThread: Overlapped WriteFile() error {1} bytes {2}", m_Name, win32Error, bytes);
            Marshal.ThrowExceptionForHR(hr);
        }

        private void ProcessPurgeCommError()
        {
            int win32Error = Marshal.GetLastWin32Error();
            int hr = Marshal.GetHRForLastWin32Error();

            switch (win32Error) {
            case WinError.ERROR_OPERATION_ABORTED:
                if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                    m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                    "{0}: SerialThread: PurgeComm() error {1}", m_Name, win32Error);
                return;
            default:
                if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Error))
                    m_Log.TraceEvent(System.Diagnostics.TraceEventType.Error,
                    "{0}: SerialThread: PurgeComm() error {1}", m_Name, win32Error);
                Marshal.ThrowExceptionForHR(hr);
                break;
            }
        }

        #region Event Handling
        public event EventHandler<CommEventArgs> CommEvent;

        protected virtual void OnCommEvent(CommEventArgs args)
        {
            if (args.EventType == 0) return;

            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                    $"{m_Name}: CommEvent: {args.EventType}");

            EventHandler<CommEventArgs> handler = CommEvent;
            if (handler is not null) {
                handler(this, args);
            }
        }

        public event EventHandler<CommErrorEventArgs> CommErrorEvent;

        protected virtual void OnCommErrorEvent(CommErrorEventArgs args)
        {
            if (args.EventType == 0) return;

            if (m_Log.ShouldTrace(System.Diagnostics.TraceEventType.Verbose))
                m_Log.TraceEvent(System.Diagnostics.TraceEventType.Verbose,
                    $"{m_Name}: CommErrorEvent: {args.EventType}");

            EventHandler<CommErrorEventArgs> handler = CommErrorEvent;
            if (handler is not null) {
                handler(this, args);
            }
        }
        #endregion

        #region IDisposable Support
        private bool m_IsDisposed; // To detect redundant calls

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_IsDisposed) return;

            if (disposing) {
                Stop();
                m_StopRunning.Dispose();
                m_SerialCommEvent.Dispose();
                m_ReadEvent.Dispose();
                m_WriteEvent.Dispose();
                m_WriteClearEvent.Dispose();
                m_WriteClearDoneEvent.Dispose();
                CommErrorEvent = null;
                CommEvent = null;
            }
            m_IsDisposed = true;
        }
        #endregion
    }
}
