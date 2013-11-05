// $URL$
// $Id$

// Copyright © Jason Curl 2012
// See http://serialportstream.codeplex.com for license details (MS-PL License)

//#define STRESSTEST
#define PL2303_WORKAROUNDS

namespace RJCP.IO.Ports
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Win32.SafeHandles;
    using RJCP.Datastructures;

    public partial class SerialPortStream
    {
        private sealed partial class NativeSerialPort : IDisposable
        {
            /// <summary>
            /// Managed Overlapped IO
            /// </summary>
            public sealed class CommOverlappedIo : IDisposable
            {
                #region Local variables
                /// <summary>
                /// Handle to the already opened Com Port
                /// </summary>
                private SafeFileHandle m_ComPortHandle;

                /// <summary>
                /// The OverlappedIoThread
                /// </summary>
                private Thread m_Thread;

                /// <summary>
                /// Read and Write buffers that are pinned
                /// </summary>
                private OverlappedIoState m_Buffers;

                /// <summary>
                /// Object to use for locking during concurrent access to the read buffer
                /// </summary>
                private object m_ReadLock = new object();

                /// <summary>
                /// Object to use for locking during concurrent access to the write buffer
                /// </summary>
                private object m_WriteLock = new object();

                /// <summary>
                /// Event to abort OverlappedIoThread (for OverlappedIoThread)
                /// </summary>
                private ManualResetEvent m_StopRunning = new ManualResetEvent(false);

                /// <summary>
                /// Overlapped I/O for WaitCommEvent() finished (for OverlappedIoThread)
                /// </summary>
                private ManualResetEvent m_SerialCommEvent = new ManualResetEvent(false);

                /// <summary>
                /// Overlapped I/O for ReadFile() finished (for OverlappedIoThread)
                /// </summary>
                private ManualResetEvent m_ReadEvent = new ManualResetEvent(false);

                /// <summary>
                /// Overlapped I/O for WriteFile() finished (for OverlappedIoThread)
                /// </summary>
                private ManualResetEvent m_WriteEvent = new ManualResetEvent(false);

                /// <summary>
                /// OverlappedIoThread can write into the read buffer
                /// </summary>
                /// <remarks>
                /// Used for flow control with the OverlappedIoThread when reading data from the
                /// serial port. Set by OverlappedIoThread when the read buffer is full. Cleared
                /// by the main thread when data is read.
                /// </remarks>
                private ManualResetEvent m_ReadBufferNotFullEvent = new ManualResetEvent(true);

                /// <summary>
                /// OverlappedIoThread can read from the write buffer
                /// </summary>
                /// <remarks>
                /// Used for flow control with the OverlappedIoThread when writing data to the
                /// serial port. Cleared by OverlappedIoThread when the write buffer is empty.
                /// Set by the main thread when data is put in the buffer.
                /// </remarks>
                private ManualResetEvent m_WriteBufferNotEmptyEvent = new ManualResetEvent(false);

                /// <summary>
                /// Main thread can read from the read buffer
                /// </summary>
                /// <remarks>
                /// Used for flow control, then the main thread wants to read from the read buffer,
                /// it can wait on this object until the read buffer is not empty. Set by
                /// OverlappedIoThread when data is written into the read buffer. Cleared by the
                /// main thread when data is emptied in the read buffer.
                /// </remarks>
                private ManualResetEvent m_ReadBufferNotEmptyEvent = new ManualResetEvent(false);

                /// <summary>
                /// Main thread can Reset this event, then wait for the next byte to arrive
                /// </summary>
                private ManualResetEvent m_ReadBufferEvent = new ManualResetEvent(false);

                /// <summary>
                /// Main thread can write to the write buffer
                /// </summary>
                /// <remarks>
                /// Used for flow control, then the main thread wants to write into the write buffer,
                /// it can wait on this object until the write buffer is not full. Cleared by
                /// OverlappedIoThread when data is read from the write buffer. Set by the main thread
                /// when the write buffer is full.
                /// </remarks>
                private ManualResetEvent m_WriteBufferNotFullEvent = new ManualResetEvent(true);

                /// <summary>
                /// Indicator that the transmit buffer is empty
                /// </summary>
                private ManualResetEvent m_TxBufferEmpty = new ManualResetEvent(true);

                /// <summary>
                /// A EV_TXEMPTY event occurred, but the buffer wasn't empty. When the write is finished,
                /// we should empty the buffer
                /// </summary>
                private volatile bool m_TxEmptyEvent = false;

                /// <summary>
                /// Indicates that there is a byte available for reading
                /// </summary>
                /// <remarks>
                /// The WaitCommEvent() method indicates if a byte has been received (EV_RXCHAR). This
                /// is cleared when the read operation is finished.
                /// </remarks>
                // CA1805: false is the default value
                private bool m_ReadByteAvailable;

                /// <summary>
                /// Basic state machine managing the EOF byte
                /// </summary>
                /// <remarks>
                /// When the EV_RXFLAG is called, we first indicate that the byte is in the driver. This
                /// is to trigger a new ReadFile() operation (one is probably already under way, so we
                /// need to do it again). Then it's in the buffer and we can raise the event.
                /// </remarks>
                [Flags]
                private enum EofByte
                {
                    /// <summary>
                    /// The EOF byte has not been received
                    /// </summary>
                    NotReceived = 0,

                    /// <summary>
                    /// The EOF byte is now in our local buffer. Raise the event
                    /// </summary>
                    InBuffer = 1,

                    /// <summary>
                    /// The EOF byte is in the buffer of the driver (EV_RXFLAG received)
                    /// </summary>
                    InDriver = 2
                }

                /// <summary>
                /// Indicates that there is a EOF character that has arrived
                /// </summary>
                /// <remarks>
                /// The WaitCommEvent() method indicates if a byte has been received (RX_CHAR) covered
                /// by the variable m_ReadByteAvailable. If m_ReadByteAvailable, this indicates if a
                /// the EOF character has arrived (EV_RXFLAG)
                /// </remarks>
                private EofByte m_ReadByteEof = EofByte.NotReceived;

                /// <summary>
                /// A unique identifier for this instance, based on the handle of the serial port.
                /// </summary>
                private int m_DebugId = -1;
                #endregion

                #region Constructors
                public CommOverlappedIo() { }

                /// <summary>
                /// Constructor, based on an already opened serial port
                /// </summary>
                /// <param name="handle">The ComPort handle to use</param>
                public CommOverlappedIo(SafeFileHandle handle):this(handle, null) { }

                /// <summary>
                /// Constructor, copying properties from another instance
                /// </summary>
                /// <param name="handle">The ComPort handle to use</param>
                /// <param name="commio">Properties to use</param>
                public CommOverlappedIo(SafeFileHandle handle, CommOverlappedIo commio)
                {
                    m_ComPortHandle = handle;
                    m_DebugId = handle.DangerousGetHandle().ToInt32();
                    if (commio != null) {
                        m_ReadBufferSize = commio.m_ReadBufferSize;
                        m_WriteBufferSize = commio.m_WriteBufferSize;
                        CommEvent = commio.CommEvent;
                        CommErrorEvent = commio.CommErrorEvent;
                    }
                }
                #endregion

                #region Events
                /// <summary>
                /// Event Arguments for a WaitCommEvent() event
                /// </summary>
                public class CommEventArgs : EventArgs
                {
                    private NativeMethods.SerialEventMask m_EventType;

                    /// <summary>
                    /// Constructor
                    /// </summary>
                    /// <param name="eventType">The event results</param>
                    public CommEventArgs(NativeMethods.SerialEventMask eventType)
                    {
                        m_EventType = eventType;
                    }

                    /// <summary>
                    /// The event bitfield
                    /// </summary>
                    public NativeMethods.SerialEventMask EventType
                    {
                        get { return m_EventType; }
                    }
                }

                /// <summary>
                /// Event Arguments for a CommStatErrors result
                /// </summary>
                public class CommErrorEventArgs : EventArgs
                {
                    private NativeMethods.ComStatErrors m_EventType;

                    /// <summary>
                    /// Constructor
                    /// </summary>
                    /// <param name="eventType">ComStatErrors result</param>
                    public CommErrorEventArgs(NativeMethods.ComStatErrors eventType)
                    {
                        m_EventType = eventType;
                    }

                    /// <summary>
                    /// CommStatErrors result
                    /// </summary>
                    public NativeMethods.ComStatErrors EventType
                    {
                        get { return m_EventType; }
                    }
                }

                /// <summary>
                /// Delegate for the CommEvent
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                public delegate void CommEventHandler(object sender, CommEventArgs e);

                /// <summary>
                /// Event raised when a new serial event has occurred, results in a bitmap
                /// </summary>
                public event CommEventHandler CommEvent;

                /// <summary>
                /// Delegate for the Comm Error Event, occurred on EV_ERR or EV_RXCHAR
                /// </summary>
                /// <param name="sender"></param>
                /// <param name="e"></param>
                public delegate void CommErrorEventHandler(object sender, CommErrorEventArgs e);

                /// <summary>
                /// Event raised when a CommError is detected, occurred on EV_ERR or EV_RXCHAR
                /// </summary>
                public event CommErrorEventHandler CommErrorEvent;

                /// <summary>
                /// Calls the event when data is received
                /// </summary>
                /// <remarks>
                /// This method simply calls the event on the current thread that is running.
                /// No checks or serialization occurs.
                /// </remarks>
                /// <param name="e">Event details</param>
                private void OnCommEvent(NativeMethods.SerialEventMask e)
                {
                    if (e == 0) return;
                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "CommEvent: " + e.ToString());
                    if (CommEvent != null) CommEvent(this, new CommEventArgs(e));
                }

                /// <summary>
                /// Calls the event when an error is detected
                /// </summary>
                /// <remarks>
                /// This method simply calls the event on the current thread that is running.
                /// No checks or serialization occurs.
                /// </remarks>
                /// <param name="e">Event details</param>
                private void OnCommErrorEvent(NativeMethods.ComStatErrors e)
                {
                    if (e == 0) return;
                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "CommErrorEvent: " + e.ToString());
                    if (CommErrorEvent != null) CommErrorEvent(this, new CommErrorEventArgs(e));
                }
                #endregion

                #region Buffer Management
                /// <summary>
                /// Maintains buffers used for reading and writing
                /// </summary>
                private sealed class OverlappedIoState : IDisposable
                {
                    private CircularBuffer<byte> m_ReadBuffer;
                    private CircularBuffer<byte> m_WriteBuffer;

                    private GCHandle m_ReadHandle;
                    private GCHandle m_WriteHandle;

                    /// <summary>
                    /// Constructor, allocating space for the read and write buffers.
                    /// </summary>
                    /// <remarks>
                    /// The read and write buffers are pinned, so that the properties ReadBufferOffsetEnd
                    /// and WriteBufferOffsetStart can provide the addresses of the buffers, used for
                    /// Platform Invoke (P/Invoke).
                    /// </remarks>
                    /// <param name="readBuffer">Size of the read buffer to allocate</param>
                    /// <param name="writeBuffer">Size of the write buffer to allocate</param>
                    public OverlappedIoState(int readBuffer, int writeBuffer)
                    {
                        byte[] read = new byte[readBuffer];
                        m_ReadHandle = GCHandle.Alloc(read, GCHandleType.Pinned);
                        m_ReadBuffer = new CircularBuffer<byte>(read, 0);

                        byte[] write = new byte[writeBuffer];
                        m_WriteHandle = GCHandle.Alloc(write, GCHandleType.Pinned);
                        m_WriteBuffer = new CircularBuffer<byte>(write, 0);
                    }

                    /// <summary>
                    /// Get the read buffer object
                    /// </summary>
                    public CircularBuffer<byte> ReadBuffer { get { return m_ReadBuffer; } }

                    /// <summary>
                    /// Get the write buffer object
                    /// </summary>
                    public CircularBuffer<byte> WriteBuffer { get { return m_WriteBuffer; } }

                    /// <summary>
                    /// Get the address for where you can write into the read buffer
                    /// </summary>
                    public IntPtr ReadBufferOffsetEnd
                    {
                        get { return m_ReadHandle.AddrOfPinnedObject() + m_ReadBuffer.End; }
                    }

                    /// <summary>
                    /// Get the address for where you can read from the beginning of the write buffer
                    /// </summary>
                    public IntPtr WriteBufferOffsetStart
                    {
                        get { return m_WriteHandle.AddrOfPinnedObject() + m_WriteBuffer.Start; }
                    }

                    /// <summary>
                    /// Dispose resources for this object
                    /// </summary>
                    public void Dispose()
                    {
                        Dispose(true);
                        GC.SuppressFinalize(this);
                    }

                    /// <summary>
                    /// Dispose resources for this object
                    /// </summary>
                    /// <param name="disposing"><b>true</b> if we're disposing from the program, <b>false</b> if
                    /// disposing from the finalizer</param>
                    private void Dispose(bool disposing)
                    {
                        // This is a sealed class, so we have "private void" instead of "protected virtual"
                        if (disposing) {
                            // Dispose managed objects here.
                            m_ReadHandle.Free();
                            m_WriteHandle.Free();
                            m_ReadBuffer = null;
                            m_WriteBuffer = null;
                        }
                    }

                    ~OverlappedIoState()
                    {
                        Dispose(false);
                    }
                }

                private int m_ReadBufferSize = 1024 * 1024;

                /// <summary>
                /// Define the internal buffer size for reading when Start() is called
                /// </summary>
                /// <remarks>
                /// Modifying this value when buffers are already allocated will cause the buffers
                /// to be discarded (and any data along with it)
                /// </remarks>
                public int ReadBufferSize
                {
                    get { return m_ReadBufferSize; }
                    set
                    {
                        if (IsRunning) throw new InvalidOperationException("Serial I/O thread in progress");
                        if (value <= 0) throw new ArgumentOutOfRangeException("value", "ReadBufferSize must be greater than zero");

                        int newBufferSize;
                        if (value < 1024) {
                            newBufferSize = 1024;
                        } else {
                            newBufferSize = value;
                        }

                        if (m_ReadBufferSize != newBufferSize) {
                            if (m_Buffers != null) {
                                m_Buffers.Dispose();
                                m_Buffers = null;
                            }
                            m_ReadBufferSize = newBufferSize;
                        }
                    }
                }

                /// <summary>
                /// Gets the number of bytes of data in the receive buffer and that of the serial driver
                /// </summary>
                public int BytesToRead
                {
                    get
                    {
                        int bytes = 0;
                        lock (m_ReadLock) {
                            if (IsRunning) {
                                NativeMethods.ComStatErrors cErr;
                                NativeMethods.COMSTAT comStat = new NativeMethods.COMSTAT();
                                bool result = UnsafeNativeMethods.ClearCommError(m_ComPortHandle, out cErr, out comStat);
                                if (result) bytes += (int)comStat.cbInQue;
                            }
                            if (m_Buffers != null) {
                                bytes += m_Buffers.ReadBuffer.Length;
                            }
                        }
                        return bytes;
                    }
                }

                /// <summary>
                /// Gets the number of bytes in the internal read buffer only.
                /// </summary>
                /// <remarks>
                /// This value is independent of the actual number of bytes in the serial port
                /// hardware buffer. It will only return that which is currently obtained by
                /// the read thread.
                /// </remarks>
                public int BufferedBytesToRead
                {
                    get
                    {
                        if (m_Buffers != null) {
                            lock (m_ReadLock) {
                                return m_Buffers.ReadBuffer.Length;
                            }
                        }
                        return 0;
                    }
                }

                /// <summary>
                /// Wait for data to arrive in the read buffer
                /// </summary>
                /// <param name="timeout">Timeout in milliseconds</param>
                /// <returns><b>true</b> if data is available in the read buffer. <b>false</b>
                /// if there is no data within the specified timeout (only applicable if IsRunning
                /// is also true), else false with no timeout if no data is available</returns>
                public bool WaitForReadEvent(int timeout)
                {
                    if (IsRunning) {
                        if (!m_ReadBufferNotEmptyEvent.WaitOne(timeout)) {
                            // Timeout, no data available
                            return false;
                        }
                    } else {
                        if (m_Buffers == null) return false;
                        if (m_Buffers.ReadBuffer.ReadLength == 0) {
                            // No data in the buffer to read. No need to timeout, as we're not running
                            // and therefore don't expect more data
                            return false;
                        }
                    }

                    return true;
                }

                /// <summary>
                /// Wait for there to be at least <i>bytes</i> by a timeout
                /// </summary>
                /// <param name="bytes">Number of bytes expected to be in the input buffer</param>
                /// <param name="timeout">Timeout in milliseconds</param>
                /// <returns><b>true</b> if there is at least <i>bytes</i> number of data in the
                /// read buffer. <b>false</b> if there is not enough data within the timeout
                /// specified</returns>
                public bool WaitForReadEvent(int bytes, int timeout)
                {
                    if (IsRunning) {
                        TimerExpiry te = new TimerExpiry(timeout);
                        while (true) {
                            lock (m_ReadLock) {
                                if (m_Buffers.ReadBuffer.Length >= bytes) return true;
                                m_ReadBufferEvent.Reset();
                            }
                            if (!m_ReadBufferEvent.WaitOne(timeout)) return false;
                            timeout = te.RemainingTime();
                        }
                    } else {
                        if (m_Buffers == null) return false;
                        if (m_Buffers.ReadBuffer.Length < bytes) {
                            // No data in the buffer to read. No need to timeout, as we're not running
                            // and therefore don't expect more data
                            return false;
                        }
                    }
                    return true;
                }

                /// <summary>
                /// Read data from the buffered serial stream into the array provided.
                /// </summary>
                /// <param name="buffer">The buffer to receive the data</param>
                /// <param name="offset">Offset into the buffer where to start putting the data</param>
                /// <param name="count">Maximum number of bytes to read into the buffer</param>
                /// <returns>The actual number of bytes copied into the buffer</returns>
                public int Read(byte[] buffer, int offset, int count)
                {
                    if (m_Buffers == null) return 0;

                    // Copy the data from the buffer into the user buffer. The events also
                    // need to occur within the lock, to prevent the race condition that the
                    // serial thread could reset/set an event just as this object sets/resets
                    // the same event resulting in lost synchronisation.
                    lock (m_ReadLock) {
                        int bytes = m_Buffers.ReadBuffer.MoveTo(buffer, offset, count);
                        if (m_Buffers.ReadBuffer.Length == 0) {
                            m_ReadBufferNotEmptyEvent.Reset();
                        }

                        // The serial thread can now write into the read buffer
                        m_ReadBufferNotFullEvent.Set();
                        return bytes;
                    }
                }

                /// <summary>
                /// Synchronously reads one byte from the SerialPort input buffer.
                /// </summary>
                /// <returns>The byte, cast to an Int32, or -1 if the end of the stream has been read.</returns>
                public int ReadByte()
                {
                    if (m_Buffers == null) return -1;
                    if (m_Buffers.ReadBuffer.Length == 0) return -1;
                    int v;
                    lock (m_ReadLock) {
                        v = m_Buffers.ReadBuffer[0];
                        m_Buffers.ReadBuffer.Consume(1);
                        if (m_Buffers.ReadBuffer.Length == 0) {
                            m_ReadBufferNotEmptyEvent.Reset();
                        }
                        m_ReadBufferNotFullEvent.Set();
                    }
                    return v;
                }

                /// <summary>
                /// Read data from the buffered serial stream into the array provided, using the encoder
                /// given.
                /// </summary>
                /// <remarks>
                /// This function converts data in the buffered stream into the character array provided.
                /// The decoder provided is used for this conversion. The decoder may contain state from
                /// a previous encoding/decoding, so it is up to the main application to decide if this
                /// is acceptable or not.
                /// <para>The number of characters converted are returned, not the number of bytes that
                /// were consumed in the incoming buffer.</para>
                /// <para>You may receive notification that data is available to read with the
                /// WaitForReadEvent(), but still have no data returned. This could be the case in
                /// particular if a multibyte character is in the pipeline, but it hasn't been
                /// completely transmitted. For example, the Euro symbol is three bytes in the UTF8
                /// encoding scheme. If only one byte arrives, there is insufficient bytes to generate
                /// a single character. You should loop until a timeout occurs.</para>
                /// </remarks>
                /// <param name="buffer">Buffer convert data into</param>
                /// <param name="offset">Offset into buffer</param>
                /// <param name="count">Number of characters to write</param>
                /// <param name="decoder">The decoder to use</param>
                /// <returns>Number of characters copied into the buffer</returns>
                public int Read(char[] buffer, int offset, int count, Decoder decoder)
                {
                    int bytesUsed;
                    return Read(buffer, offset, count, decoder, out bytesUsed);
                }

                /// <summary>
                /// Read data from the buffered serial stream into the array provided, using the encoder
                /// given.
                /// </summary>
                /// <remarks>
                /// This function converts data in the buffered stream into the character array provided.
                /// The decoder provided is used for this conversion. The decoder may contain state from
                /// a previous encoding/decoding, so it is up to the main application to decide if this
                /// is acceptable or not.
                /// <para>The number of characters converted are returned, not the number of bytes that
                /// were consumed in the incoming buffer.</para>
                /// <para>You may receive notification that data is available to read with the
                /// WaitForReadEvent(), but still have no data returned. This could be the case in
                /// particular if a multibyte character is in the pipeline, but it hasn't been
                /// completely transmitted. For example, the Euro symbol is three bytes in the UTF8
                /// encoding scheme. If only one byte arrives, there is insufficient bytes to generate
                /// a single character. You should loop until a timeout occurs.</para>
                /// </remarks>
                /// <param name="buffer">Buffer convert data into</param>
                /// <param name="offset">Offset into buffer</param>
                /// <param name="count">Number of characters to write</param>
                /// <param name="decoder">The decoder to use</param>
                /// <param name="bytesUsed">Number of bytes consumed by the internal read buffer</param>
                /// <returns>Number of characters copied into the buffer</returns>
                public int Read(char[] buffer, int offset, int count, Decoder decoder, out int bytesUsed)
                {
                    if (m_Buffers == null) {
                        bytesUsed = 0;
                        return -1;
                    }

                    int cu;
                    bool completed;

                    lock (m_ReadLock) {
                        decoder.Convert(m_Buffers.ReadBuffer, buffer, offset, count, false, out bytesUsed, out cu, out completed);
                    }
                    return cu;
                }

                /// <summary>
                /// Read data from the buffered serial stream, using the encoder given
                /// </summary>
                /// <remarks>
                /// This function will convert to a single character using the current decoder. The
                /// decoder may maintain state from a previous Read(char[], ...) operation.
                /// <para>You may receive notification that data is available to read with the
                /// WaitForReadEvent(), but still have no data returned. This could be the case in
                /// particular if a multibyte character is in the pipeline, but it hasn't been
                /// completely transmitted. For example, the Euro symbol is three bytes in the UTF8
                /// encoding scheme. If only one byte arrives, there is insufficient bytes to generate
                /// a single character. You should loop until a timeout occurs.</para>
                /// </remarks>
                /// <param name="decoder">The decoder to use</param>
                /// <returns>The character interpreted, or -1 if no data available</returns>
                public int ReadChar(Decoder decoder)
                {
                    if (m_Buffers == null) return -1;

                    int bu;
                    int cu;
                    bool completed;
                    char[] c = new char[1];

                    lock (m_ReadLock) {
                        decoder.Convert(m_Buffers.ReadBuffer, c, 0, 1, false, out bu, out cu, out completed);
                    }
                    if (cu != 1) return -1;
                    return c[0];
                }

                /// <summary>
                /// Convert a single character in the read buffer
                /// </summary>
                /// <remarks>
                /// This method is designed for the ReadTo() method. One would read byte for byte, without
                /// actually updating the offsets into the buffer, building a string.
                /// <para>Normally, a single character will be read one at a time, from the first available
                /// byte until a complete string is available. The decoder used may maintain state, so the
                /// results given by this function is dependent on the state of the decoder. If a character
                /// is available, its value will be returned and on output the variable <i>bytesRead</i>
                /// will contain the number of bytes that the decoder reports as being used to obtain the
                /// single character. If no character is available in the buffer at the offset provided,
                /// -1 will be returned. The main application should therefore implement timeouts as
                /// necessary.</para>
                /// <para>Note, because of the way that decoders work, it is possible to get no characters
                /// back, but the number of bytes read via <i>bytesRead</i> may be non-zero.</para>
                /// </remarks>
                /// <param name="offset">The offset from the beginning of the read byte buffer to read from</param>
                /// <param name="decoder">The decoder to use to get a single character</param>
                /// <param name="bytesRead">If a character is read, the number of bytes consumed to generate
                /// this character</param>
                /// <returns>The character at the position defined by <i>offset</i></returns>
                public int PeekChar(int offset, Decoder decoder, out int bytesRead)
                {
                    bytesRead = 0;
                    if (m_Buffers == null) return -1;

                    int readlen = m_Buffers.ReadBuffer.Length;
                    if (offset >= readlen) return -1;

                    char[] onechar = new char[1];
                    int cu = 0;
                    bool complete = false;
                    while (cu < 1 && offset < readlen) {
                        int bu;
                        decoder.Convert(m_Buffers.ReadBuffer.Array, m_Buffers.ReadBuffer.ToArrayIndex(offset), 1,
                            onechar, 0, 1, false, out bu, out cu, out complete);
                        bytesRead += bu;
                        offset++;
                    }

                    if (cu == 0) return -1;
                    return onechar[0];
                }

                /// <summary>
                /// Discards data from the serial driver's receive buffer.
                /// </summary>
                /// <remarks>
                /// This function will discard the receive buffer of the SerialPortStream.
                /// </remarks>
                public void DiscardInBuffer()
                {
                    if (m_Buffers == null) return;

                    lock (m_ReadLock) {
                        // We do NOT issue a Reset() here. If there is a background ReadFile() in
                        // progress, a Reset() could cause corrupt data in the buffer as the ReadFile()
                        // will write to a different portion of data and then produce the bytes. A
                        // Consume() advances the pointers, so that when the IO thread calls Produce(),
                        // pointers are advanced correctly to the data that was actually written.
                        m_Buffers.ReadBuffer.Consume(m_Buffers.ReadBuffer.Length);
                        m_ReadBufferNotFullEvent.Set();
                        UnsafeNativeMethods.PurgeComm(m_ComPortHandle, NativeMethods.PurgeFlags.PURGE_RXABORT | NativeMethods.PurgeFlags.PURGE_RXCLEAR);
                        m_ReadBufferNotEmptyEvent.Reset();
                    }
                }

                /// <summary>
                /// Discards data from the read buffer, but won't discard data already received by the
                /// driver
                /// </summary>
                /// <remarks>
                /// This method will only discard data that has already been read from the driver
                /// and cached locally. It will not discard data in the driver's buffer.
                /// </remarks>
                /// <param name="bytes">Number of bytes to discard</param>
                public void DiscardInBuffer(int bytes)
                {
                    if (m_Buffers == null) return;

                    lock (m_ReadLock) {
                        m_Buffers.ReadBuffer.Consume(bytes);
                        m_ReadBufferNotFullEvent.Set();
                    }
                }

                private int m_WriteBufferSize = 128 * 1024;

                /// <summary>
                /// Define the internal buffer size for writing when Start() is called
                /// </summary>
                /// <remarks>
                /// Modifying this value when buffers are already allocated will cause the buffers
                /// to be discarded (and any data along with it)
                /// </remarks>
                public int WriteBufferSize
                {
                    get { return m_WriteBufferSize; }
                    set
                    {
                        if (IsRunning) throw new InvalidOperationException("Serial I/O thread in progress");
                        if (value <= 0) throw new ArgumentOutOfRangeException("value", "WriteBufferSize must be greater than zero");

                        int newBufferSize;
                        if (value < 1024) {
                            newBufferSize = 1024;
                        } else {
                            newBufferSize = value;
                        }

                        if (m_WriteBufferSize != newBufferSize) {
                            if (m_Buffers != null) {
                                m_Buffers.Dispose();
                                m_Buffers = null;
                            }
                            m_WriteBufferSize = newBufferSize;
                        }
                    }
                }

                /// <summary>
                /// Gets the number of bytes of data in the transmit buffer.
                /// </summary>
                public int BytesToWrite
                {
                    get
                    {
                        int bytes = 0;
                        if (IsRunning) {
                            NativeMethods.ComStatErrors cErr;
                            NativeMethods.COMSTAT comStat = new NativeMethods.COMSTAT();
                            bool result = UnsafeNativeMethods.ClearCommError(m_ComPortHandle, out cErr, out comStat);
                            if (result) bytes += (int)comStat.cbOutQue;
                        }
                        if (m_Buffers != null) {
                            lock (m_WriteLock) {
                                bytes += m_Buffers.WriteBuffer.Length;
                            }
                        }
                        return bytes;
                    }
                }

                /// <summary>
                /// Wait for sufficient buffer space to be available in the write buffer witin a timeout
                /// </summary>
                /// <param name="count">The number of bytes that should be available</param>
                /// <param name="timeout">The timeout in milliseconds</param>
                /// <returns><b>true</b> if there is at least <c>count</c> bytes availalbe in the write
                /// buffer within <c>timeout</c> milliseconds. <b>false</b> is always returned if no
                /// buffers have been allocated or if the serial thread isn't running (without a timeout)
                /// </returns>
                public bool WaitForWriteEvent(int count, int timeout)
                {
                    if (m_Buffers == null || !IsRunning) return false;

                    int free;
                    lock (m_WriteLock) {
                        // If we can't write the entire buffer to the WRITE buffer, we should wait
                        free = m_Buffers.WriteBuffer.Free;
                        if (free < count) m_WriteBufferNotFullEvent.Reset();
                    }

                    // Wait until we can write the full amount of data to the buffer
                    TimerExpiry exp = new TimerExpiry(timeout);
                    while (free < count) {
                        if (!m_WriteBufferNotFullEvent.WaitOne(exp.RemainingTime())) {
                            return false;
                        } else {
                            lock (m_WriteLock) {
                                // If we can't write the entire buffer to the WRITE buffer, we should wait
                                free = m_Buffers.WriteBuffer.Free;
                                if (free < count) m_WriteBufferNotFullEvent.Reset();
                            }
                        }
                    }

                    return true;
                }

                /// <summary>
                /// Wait for when the write operation is complete
                /// </summary>
                /// <param name="timeout">Time to wait for the buffers to be empty</param>
                /// <returns><b>true</b> if the write buffers completed within the timeout period. <b>false</b> otherwise.</returns>
                public bool WaitForWriteEmptyEvent(int timeout)
                {
                    if (m_Buffers == null || !IsRunning) return true;

                    if (!m_TxBufferEmpty.WaitOne(timeout)) return false;
                    return true;
                }

                /// <summary>
                /// Write the given data into the buffered serial stream for sending over the serial port
                /// </summary>
                /// <remarks>
                /// Data is copied from the array provided into the local stream buffer. It does
                /// not guarantee that data will be sent over the serial port. So long as there is
                /// enough local buffer space to accept the write of count bytes, this function
                /// will succeed. In case that the buffered serial stream doesn't have enough data,
                /// the missing data will be silently ignored, with the return value being the number
                /// of bytes that were copied into the local buffer.
                /// </remarks>
                /// <param name="buffer">The buffer containing data to send</param>
                /// <param name="offset">Offset into the array buffer where data begins</param>
                /// <param name="count">Number of bytes to copy into the local buffer</param>
                /// <exception cref="TimeoutException">Not enough bufferspace was made available
                /// before the timeout expired</exception>
                /// <returns>The number of bytes copied into the buffer</returns>
                public int Write(byte[] buffer, int offset, int count)
                {
                    if (m_Buffers == null || !IsRunning) throw new InvalidOperationException("Serial I/O Thread not running");

                    int bytes;
                    // Write the data into the buffer
                    lock (m_WriteLock) {
                        bytes = m_Buffers.WriteBuffer.Append(buffer, offset, count);
                        // The serial thread can now send data to the serial port
                        m_WriteBufferNotEmptyEvent.Set();
                        m_TxEmptyEvent = false;
                    }
                    return bytes;
                }

                /// <summary>
                /// Discards data from the serial driver's receive buffer.
                /// </summary>
                /// <remarks>
                /// This function will discard the receive buffer of the SerialPortStream.
                /// </remarks>
                public void DiscardOutBuffer()
                {
                    lock (m_WriteLock) {
                        // We do NOT issue a Reset() here. If there is a background ReadFile() in
                        // progress, a Reset() could cause corrupt data in the buffer as the ReadFile()
                        // will write to a different portion of data and then produce the bytes. A
                        // Consume() advances the pointers, so that when the IO thread calls Produce(),
                        // pointers are advanced correctly to the data that was actually written.
                        m_Buffers.ReadBuffer.Consume(m_Buffers.WriteBuffer.Length);
                        m_WriteBufferNotEmptyEvent.Reset();
                        UnsafeNativeMethods.PurgeComm(m_ComPortHandle, NativeMethods.PurgeFlags.PURGE_TXABORT | NativeMethods.PurgeFlags.PURGE_TXCLEAR);
                        m_WriteBufferNotFullEvent.Set();
                    }
                }
                #endregion

                #region Thread Control
                /// <summary>
                /// Start the I/O thread
                /// </summary>
                public void Start()
                {
                    if (m_Buffers == null) m_Buffers = new OverlappedIoState(m_ReadBufferSize, m_WriteBufferSize);

                    // Set the timeouts
                    NativeMethods.COMMTIMEOUTS timeouts = new NativeMethods.COMMTIMEOUTS();
                    // We read only the data that is buffered
#if PL2303_WORKAROUNDS
                    timeouts.ReadIntervalTimeout = -1;
                    timeouts.ReadTotalTimeoutConstant = 100;
                    timeouts.ReadTotalTimeoutMultiplier = 0;
#else
                    // Non-asynchronous behaviour
                    timeouts.ReadIntervalTimeout = -1;
                    timeouts.ReadTotalTimeoutConstant = 0;
                    timeouts.ReadTotalTimeoutMultiplier = 0;
#endif
                    // We have no timeouts when writing
                    timeouts.WriteTotalTimeoutMultiplier = 0;
                    timeouts.WriteTotalTimeoutConstant = 500;

                    bool result = UnsafeNativeMethods.SetCommTimeouts(m_ComPortHandle, ref timeouts);
                    if (!result) {
                        throw new IOException("Couldn't set CommTimeouts", Marshal.GetLastWin32Error());
                    }

                    m_Thread = new Thread(new ThreadStart(OverlappedIoThread));
                    m_Thread.IsBackground = true;
                    m_Thread.Start();
                }

                /// <summary>
                /// Cancel pending I/O, stop the I/O thread, wait and then return
                /// </summary>
                public void Stop()
                {
                    if (IsRunning) {
                        m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "OverlappedIO: Stopping Thread");
                        m_StopRunning.Set();
                        m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "OverlappedIO: Waiting for Thread");
                        m_Thread.Join();
                        m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "OverlappedIO: Thread Stopped");
                        m_Thread = null;

                        // We clear the write buffer, as we're no longer running. But there still might be data to read.
                        m_Buffers.WriteBuffer.Reset();
                    }
                }

                /// <summary>
                /// Test if the I/O thread is running
                /// </summary>
                public bool IsRunning
                {
                    get { return m_Thread != null && (m_Thread.IsAlive); }
                }
                #endregion
                
                /// <summary>
                /// Entry point to the I/O thread
                /// </summary>
                private void OverlappedIoThread()
                {
                    // WaitCommEvent
                    bool serialCommPending = false;
                    m_SerialCommEvent.Reset();
                    NativeOverlapped serialCommOverlapped = new NativeOverlapped();
                    serialCommOverlapped.EventHandle = m_SerialCommEvent.SafeWaitHandle.DangerousGetHandle();

                    // ReadFile
                    bool readPending = false;
                    m_ReadEvent.Reset();
                    NativeOverlapped readOverlapped = new NativeOverlapped();
                    readOverlapped.EventHandle = m_ReadEvent.SafeWaitHandle.DangerousGetHandle();

                    // WriteFile
                    bool writePending = false;
                    m_WriteEvent.Reset();
                    NativeOverlapped writeOverlapped = new NativeOverlapped();
                    m_ReadByteAvailable = false;
                    writeOverlapped.EventHandle = m_WriteEvent.SafeWaitHandle.DangerousGetHandle();

                    // Set up the types of serial events we want to see
                    UnsafeNativeMethods.SetCommMask(m_ComPortHandle,
                        NativeMethods.SerialEventMask.EV_BREAK |
                        NativeMethods.SerialEventMask.EV_CTS |
                        NativeMethods.SerialEventMask.EV_DSR |
                        NativeMethods.SerialEventMask.EV_ERR |
                        NativeMethods.SerialEventMask.EV_RING |
                        NativeMethods.SerialEventMask.EV_RLSD |
                        NativeMethods.SerialEventMask.EV_RXCHAR |
                        NativeMethods.SerialEventMask.EV_TXEMPTY |
                        NativeMethods.SerialEventMask.EV_EVENT1 |
                        NativeMethods.SerialEventMask.EV_EVENT2 |
                        NativeMethods.SerialEventMask.EV_PERR |
                        NativeMethods.SerialEventMask.EV_RX80FULL |
                        NativeMethods.SerialEventMask.EV_RXFLAG);

                    bool result;
                    NativeMethods.SerialEventMask commEventMask = 0;

                    bool running = true;
                    uint bytes;

                    while (running) {
                        List<WaitHandle> handles = new List<WaitHandle>(10);
                        handles.Add(m_StopRunning);

#if PL2303_WORKAROUNDS
                        // - - - - - - - - - - - - - - - - - - - - - - - - -
                        // PROLIFIC PL23030 WORKAROUND
                        // - - - - - - - - - - - - - - - - - - - - - - - - -
                        // If we have a read pending, we don't request events
                        // for reading data. To do so will result in errors. 
                        // Have no idea why.
                        if (!readPending) {
                            UnsafeNativeMethods.SetCommMask(m_ComPortHandle,
                                NativeMethods.SerialEventMask.EV_BREAK |
                                NativeMethods.SerialEventMask.EV_CTS |
                                NativeMethods.SerialEventMask.EV_DSR |
                                NativeMethods.SerialEventMask.EV_ERR |
                                NativeMethods.SerialEventMask.EV_RING |
                                NativeMethods.SerialEventMask.EV_RLSD |
                                NativeMethods.SerialEventMask.EV_RXCHAR |
                                NativeMethods.SerialEventMask.EV_TXEMPTY |
                                NativeMethods.SerialEventMask.EV_EVENT1 |
                                NativeMethods.SerialEventMask.EV_EVENT2 |
                                NativeMethods.SerialEventMask.EV_PERR |
                                NativeMethods.SerialEventMask.EV_RX80FULL |
                                NativeMethods.SerialEventMask.EV_RXFLAG);
                        } else {
                            UnsafeNativeMethods.SetCommMask(m_ComPortHandle,
                                NativeMethods.SerialEventMask.EV_BREAK |
                                NativeMethods.SerialEventMask.EV_CTS |
                                NativeMethods.SerialEventMask.EV_DSR |
                                NativeMethods.SerialEventMask.EV_ERR |
                                NativeMethods.SerialEventMask.EV_RING |
                                NativeMethods.SerialEventMask.EV_RLSD |
                                NativeMethods.SerialEventMask.EV_TXEMPTY |
                                NativeMethods.SerialEventMask.EV_EVENT1 |
                                NativeMethods.SerialEventMask.EV_EVENT2 |
                                NativeMethods.SerialEventMask.EV_PERR);
                        }
#endif

                        // commEventMask is on the stack, and is therefore fixed
                        if (!serialCommPending) serialCommPending = DoWaitCommEvent(out commEventMask, ref serialCommOverlapped);
                        if (serialCommPending) handles.Add(m_SerialCommEvent);

                        if (!readPending) {
                            if (!m_ReadBufferNotFullEvent.WaitOne(0)) {
                                m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "SerialThread: Read Buffer Full");
                                handles.Add(m_ReadBufferNotFullEvent);
                            } else { 
                                readPending = DoReadEvent(ref readOverlapped);
                            }
                        }
                        if (readPending) handles.Add(m_ReadEvent);

                        if (!writePending) {
                            if (!m_WriteBufferNotEmptyEvent.WaitOne(0)) {
                                handles.Add(m_WriteBufferNotEmptyEvent);
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
                                m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "SerialThread: Thread closing");
                                result = UnsafeNativeMethods.CancelIo(m_ComPortHandle);
                                if (!result) {
                                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Warning, m_DebugId, 
                                        "SerialThread: CancelIo error {0}", Marshal.GetLastWin32Error());
                                }
                                running = false;
                            } else if (whandles[ev] == m_SerialCommEvent) {
                                result = UnsafeNativeMethods.GetOverlappedResult(m_ComPortHandle, ref serialCommOverlapped, out bytes, false);
                                int e = Marshal.GetLastWin32Error();
                                if (result) {
                                    ProcessWaitCommEvent(commEventMask);
                                } else {
                                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Error, m_DebugId,
                                        "SerialThread: Overlapped WaitCommEvent() error {0}", e);
                                }
                                serialCommPending = false;
                            } else if (whandles[ev] == m_ReadEvent) {
                                result = UnsafeNativeMethods.GetOverlappedResult(m_ComPortHandle, ref readOverlapped, out bytes, false);
                                int e = Marshal.GetLastWin32Error();
                                if (result) {
                                    ProcessReadEvent(bytes);
                                } else {
                                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Error, m_DebugId,
                                        "SerialThread: Overlapped ReadFile() error {0}", e);
                                }
                                readPending = false;
                            } else if (whandles[ev] == m_ReadBufferNotFullEvent) {
                                // The read buffer is no longer full. We just loop back to the beginning to test if we
                                // should read or not.
                            } else if (whandles[ev] == m_WriteEvent) {
                                result = UnsafeNativeMethods.GetOverlappedResult(m_ComPortHandle, ref writeOverlapped, out bytes, false);
                                int e = Marshal.GetLastWin32Error();
                                if (result) {
                                    ProcessWriteEvent(bytes);
                                } else {
                                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Error, m_DebugId,
                                        "SerialThread: Overlapped WriteFile() error {0}", e);
                                }
                                writePending = false;
                            } else if (whandles[ev] == m_WriteBufferNotEmptyEvent) {
                                // The write buffer is no longer empty. We just loop back to the beginning to test if we
                                // should write or not.
                            }
                        }

#if STRESSTEST
                        m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, 0, "STRESSTEST SerialThread: Stress Test Delay of 1000ms");
                        System.Threading.Thread.Sleep(1000);
                        Native.ComStatErrors e = new Native.ComStatErrors();
                        Native.COMSTAT stat = new Native.COMSTAT();
                        result = Native.ClearCommError(m_ComPortHandle, out e, out stat);
                        if (result) {
                            m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "STRESSTEST SerialThread: ClearCommError errors={0}", e);
                            m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "STRESSTEST SerialThread: ClearCommError stats flags={0}, Inqueue={1}, Outqueue={2}", stat.Flags, stat.cbInQue, stat.cbOutQue);
                        } else {
                            m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Warning, 0, "STRESSTEST SerialThread: ClearCommError error: {0}", Marshal.GetLastWin32Error());
                        }
#endif
                    }
                }

                /// <summary>
                /// Check if we should execute WaitCommEvent() and get the result if immediately available
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
                /// <param name="mask">The mask value if information is available immediately</param>
                /// <param name="overlap">The overlap structure to use</param>
                /// <returns>If the operation is pending or not</returns>
                private bool DoWaitCommEvent(out NativeMethods.SerialEventMask mask, ref NativeOverlapped overlap)
                {
                    bool result = UnsafeNativeMethods.WaitCommEvent(m_ComPortHandle, out mask, ref overlap);
                    int e = Marshal.GetLastWin32Error();
                    if (result) {
                        ProcessWaitCommEvent(mask);
                    } else {
                        if (e != WinError.ERROR_IO_PENDING) {
                            m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Error, m_DebugId, "SerialThread: DoWaitCommEvent: Result: {0}", e);
                            //throw new IOException("WaitCommEvent error", e);
                        }
                    }
                    return !result;
                }

                /// <summary>
                /// Do work based on the mask event that has occurred
                /// </summary>
                /// <param name="mask">The mask that was provided</param>
                private void ProcessWaitCommEvent(NativeMethods.SerialEventMask mask)
                {
                    if (mask != (int)0) {
                        m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "SerialThread: ProcessWaitCommEvent: {0}", mask);
                    }

                    // Reading a character
                    if ((mask & NativeMethods.SerialEventMask.EV_RXCHAR) != 0) {
                        m_ReadByteAvailable = true;
                    }
                    if ((mask & NativeMethods.SerialEventMask.EV_RXFLAG) != 0) {
                        m_ReadByteAvailable = true;
                        // An EOF character may already be in our buffer. We don't lock this, as only
                        // the Serial I/O thread reads/writes this flag.
                        m_ReadByteEof |= EofByte.InDriver;
                    }

                    // We don't raise an event for characters immediately, but only after the read operation
                    // is complete.
                    OnCommEvent(mask & ~(NativeMethods.SerialEventMask.EV_RXCHAR | NativeMethods.SerialEventMask.EV_RXFLAG));

                    if ((mask & NativeMethods.SerialEventMask.EV_TXEMPTY) != 0) {
                        lock (m_WriteLock) {
                            if (m_Buffers.WriteBuffer.Length == 0) {
                                m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "SerialThread: ProcessWaitCommEvent: TX-BUFFER empty"); 
                                m_TxBufferEmpty.Set();
                                m_TxEmptyEvent = false;
                            } else {
                                // Because the main event loop handles CommEvents before WriteEvents, it could be
                                // that a write event occurs immediately after, actually emptying the buffer.
                                m_TxEmptyEvent = true;
                            }
                        }
                    }

                    if ((mask & (NativeMethods.SerialEventMask.EV_RXCHAR | NativeMethods.SerialEventMask.EV_ERR)) != 0) {
                        NativeMethods.ComStatErrors comErr;
                        bool result = UnsafeNativeMethods.ClearCommError(m_ComPortHandle, out comErr, IntPtr.Zero);
                        int e = Marshal.GetLastWin32Error();
                        if (result) {
                            comErr = (NativeMethods.ComStatErrors)((int)comErr & 0x10F);
                            if (comErr != 0) {
                                OnCommErrorEvent(comErr);
                            }
                        } else {
                            m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, 
                                "SerialThread: ClearCommError: WINERROR {0}", e);
                        }
                    }
                }

                /// <summary>
                /// Check if we should ReadFile() and process the data if serial data is immediately
                /// available.
                /// </summary>
                /// <remarks>
                /// This function should be called if there is no existing pending read operation. It
                /// will check if there is data to read (indicated by the variable m_ReadByteAvailable,
                /// which is set by ProcessWaitCommEvent()) and then issue a ReadFile(). If the result
                /// indicates that asynchronous I/O is happening, <b>true</b> is returned. Else this
                /// function automatically calls ProcessReadEvent() with the number of bytes read. If
                /// an asynchronous operation is pending, then you should wait on the event in the
                /// overlapped structure not not call this function until GetOverlappedResult() has
                /// been called.
                /// </remarks>
                /// <param name="overlap">The overlap structure to use for reading</param>
                /// <returns>If the operation is pending or not</returns>
                private bool DoReadEvent(ref NativeOverlapped overlap)
                {
                    // If WaitCommEvent() hasn't been called, there's no data
                    if (!m_ReadByteAvailable) return false;

                    // Read Buffer is full, so can't write into it.
                    if (!m_ReadBufferNotFullEvent.WaitOne(0)) return false;

                    // As C# can't convert an offset in the array to a pointer, we have to do
                    // our own marshalling with (IntPtr)ReadBufferOffsetEnd that is the address
                    // at the end of the read buffer.
                    IntPtr bufPtr;
                    uint bufLen;
                    lock (m_ReadLock) {
                        bufPtr = m_Buffers.ReadBufferOffsetEnd;
                        bufLen = (uint)m_Buffers.ReadBuffer.WriteLength;
                    }

                    uint bufRead;
                    bool result = UnsafeNativeMethods.ReadFile(m_ComPortHandle, bufPtr, bufLen, out bufRead, ref overlap);
                    int e = Marshal.GetLastWin32Error();
                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, 
                        "SerialThread: DoReadEvent: ReadFile({0}, {1}, {2}) == {3}", 
                        m_ComPortHandle.DangerousGetHandle(), bufPtr, bufLen, result);
                    if (result) {
                        // MS Documentation for ReadFile() says that the 'bufRead' parameter should be NULL.
                        // However, in the case that the COMMTIMEOUTS is set up so that no wait is required
                        // (see COMMTIMEOUTS in Win32 API), this function will actually not perform an
                        // asynchronous I/O operation and return the number of bytes copied in bufRead. 
                        ProcessReadEvent(bufRead);
                    } else {
                        if (e != WinError.ERROR_IO_PENDING) {
                            m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Error, m_DebugId, "SerialThread: DoReadEvent: ReadFile() error {0}", e);
                            //throw new IOException("ReadFile error", e);
                        }
                    }
                    return !result;
                }

                /// <summary>
                /// Produce the number of bytes read in the buffer
                /// </summary>
                /// <remarks>
                /// If the number of bytes read is zero, this function should also be called, as it indicates
                /// that there are no more bytes pending. The recommendation from MS documentation for reading
                /// from the serial port indicates to read the buffer data, until a result of zero is given,
                /// which indicates to wait for the next receiving character.
                /// </remarks>
                /// <param name="bytes">Number of bytes read</param>
                private void ProcessReadEvent(uint bytes)
                {
                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "SerialThread: ProcessReadEvent: {0} bytes", bytes);
                    if (bytes == 0) {
                        m_ReadByteAvailable = false;
                    } else {
                        lock (m_ReadLock) {
                            m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, 
                                "SerialThread: ProcessReadEvent: End=" + m_Buffers.ReadBuffer.End + "; Bytes=" + bytes.ToString());
                            m_Buffers.ReadBuffer.Produce((int)bytes);
                            if (m_Buffers.ReadBuffer.Free == 0) {
                                m_ReadBufferNotFullEvent.Reset();
                            }
                            m_ReadBufferNotEmptyEvent.Set();
                            m_ReadBufferEvent.Set();
                        }

                        bool eof = (m_ReadByteEof & EofByte.InBuffer) != 0;
                        OnCommEvent(eof ? NativeMethods.SerialEventMask.EV_RXFLAG : NativeMethods.SerialEventMask.EV_RXCHAR);

                        // The EofByte is so designed, that we can use a bitshift to get to the next state.
                        // We don't lock this, as only the Serial I/O thread reads/writes this flag.
                        m_ReadByteEof = (EofByte)(((int)m_ReadByteEof << 1) & 0x03);
                    }
                }

                /// <summary>
                /// Check if we should WriteFile() and update buffers if serial data is immediately cached by driver
                /// </summary>
                /// <remarks>
                /// This function should be called if there is no existing pending write operation. If
                /// the result indicates that asynchronous I/O is happening, <b>true</b> is returned.
                /// Else this function automatically calls ProcessWriteEvent() with the number of bytes
                /// written. If an asynchronous operation is pending, then you should wait on the event
                /// in the overlapped structure not not call this function until GetOverlappedResult()
                /// has been called.
                /// </remarks>
                /// <param name="overlap">The overlap structure to use for writing</param>
                /// <returns>If the operation is pending or not</returns>
                private bool DoWriteEvent(ref NativeOverlapped overlap)
                {
                    IntPtr bufPtr;
                    uint bufLen;
                    lock (m_WriteLock) {
                        bufPtr = m_Buffers.WriteBufferOffsetStart;
                        bufLen = (uint)m_Buffers.WriteBuffer.ReadLength;
                    }

                    uint bufWrite;
                    m_TxBufferEmpty.Reset();
                    bool result = UnsafeNativeMethods.WriteFile(m_ComPortHandle, bufPtr, bufLen, out bufWrite, ref overlap);
                    int e = Marshal.GetLastWin32Error();
                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId,
                        "SerialThread: DoWriteEvent: WriteFile({0}, {1}, {2}, ...) == {3}",
                        m_ComPortHandle.DangerousGetHandle(), bufPtr, bufLen, result);
                    if (result) {
                        ProcessWriteEvent(bufWrite);
                    } else {
                        if (e != WinError.ERROR_IO_PENDING) {
                            m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Error, m_DebugId, "SerialThread: DoWriteEvent: WriteFile() error {0}", e);
                            //throw new IOException("WriteFile error", e);
                        }
                    }
                    return !result;
                }

                /// <summary>
                /// Consume the number of bytes written from the write buffer
                /// </summary>
                /// <param name="bytes">Number of bytes written to the driver</param>
                private void ProcessWriteEvent(uint bytes)
                {
                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "SerialThread: ProcessWriteEvent: {0} bytes", bytes);
                    if (bytes != 0) {
                        lock (m_WriteLock) {
                            m_Buffers.WriteBuffer.Consume((int)bytes);
                            if (m_Buffers.WriteBuffer.Length == 0) {
                                m_WriteBufferNotEmptyEvent.Reset();
                                if (m_TxEmptyEvent) {
                                    m_TxEmptyEvent = false;
                                    m_Trace.TraceEvent(System.Diagnostics.TraceEventType.Verbose, m_DebugId, "SerialThread: ProcessWriteEvent: TX-BUFFER empty");
                                    m_TxBufferEmpty.Set();
                                }
                            }
                            m_WriteBufferNotFullEvent.Set();
                        }
                    }
                }

                /// <summary>
                /// Dispose this object
                /// </summary>
                public void Dispose()
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }

                /// <summary>
                /// Dispose resources for this object
                /// </summary>
                /// <param name="disposing"><b>true</b> if we're disposing from the program, <b>false</b> if
                /// disposing from the finalizer</param>
                private void Dispose(bool disposing)
                {
                    if (disposing) {
                        if (IsRunning) Stop();
                        if (m_Buffers != null) m_Buffers.Dispose();
                        m_Buffers = null;

                        m_StopRunning.Dispose();
                        m_SerialCommEvent.Dispose();
                        m_ReadEvent.Dispose();
                        m_WriteEvent.Dispose();
                        m_ReadBufferNotFullEvent.Dispose();
                        m_WriteBufferNotEmptyEvent.Dispose();
                        m_ReadBufferNotEmptyEvent.Dispose();
                        m_WriteBufferNotFullEvent.Dispose();
                        m_ReadBufferEvent.Dispose();
                        m_TxBufferEmpty.Dispose();
                    }
                }

                ~CommOverlappedIo()
                {
                    Dispose(false);
                }
            }
        }
    }
}
