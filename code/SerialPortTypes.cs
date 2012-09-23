// $URL$
// $Id$
using System;
using System.Collections.Generic;
using System.Text;

namespace RJCP.IO.Ports
{
    /// <summary>
    /// The type of parity to use
    /// </summary>
    public enum Parity
    {
        /// <summary>
        /// No parity
        /// </summary>
        None = 0,

        /// <summary>
        /// Odd parity
        /// </summary>
        Odd = 1,

        /// <summary>
        /// Even parity
        /// </summary>
        Even = 2,

        /// <summary>
        /// Mark parity
        /// </summary>
        Mark = 3,

        /// <summary>
        /// Space parity
        /// </summary>
        Space = 4
    }

    /// <summary>
    /// Number of stop bits to use
    /// </summary>
    public enum StopBits
    {
        /// <summary>
        /// One stop bit
        /// </summary>
        One = 0,

        /// <summary>
        /// 1.5 stop bits
        /// </summary>
        One5 = 1,

        /// <summary>
        /// Two stop bits
        /// </summary>
        Two = 2
    }

    /// <summary>
    /// Handshaking mode to use
    /// </summary>
    [Flags]
    public enum Handshake
    {
        /// <summary>
        /// No handshaking
        /// </summary>
        None = 0,

        /// <summary>
        /// Software handshaking
        /// </summary>
        XON = 1,

        /// <summary>
        /// Hardware handshaking (RTS/CTS)
        /// </summary>
        RTS = 2,

        /// <summary>
        /// Hardware handshaking (DTR/DSR) (uncommon)
        /// </summary>
        DTR = 4,

        /// <summary>
        /// RTS and Software handshaking
        /// </summary>
        RTS_XON = RTS | XON,

        /// <summary>
        /// DTR and Software handshaking (uncommon)
        /// </summary>
        DTR_XON = DTR | XON,

        /// <summary>
        /// Hardware handshaking with RTS/CTS and DTR/DSR (uncommon)
        /// </summary>
        DTR_RTS = DTR | RTS,

        /// <summary>
        /// Hardware handshaking with RTS/CTS and DTR/DSR and Software handshaking (uncommon)
        /// </summary>
        DTR_RTS_XON = DTR | RTS | XON
    }

    #region Events
    /// <summary>
    /// Event related information on DataReceived
    /// </summary>
    public enum SerialData
    {
        /// <summary>
        /// At least a single byte has been received
        /// </summary>
        Chars = 0x0001,

        /// <summary>
        /// The EOF character has been detected
        /// </summary>
        Eof = 0x0002
    }

    /// <summary>
    /// Event Args for DataReceived
    /// </summary>
    public class SerialDataReceivedEventArgs  : EventArgs
    {
        private SerialData m_EventType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventType">Event that occurred</param>
        public SerialDataReceivedEventArgs(SerialData eventType)
        {
            m_EventType = eventType;
        }

        /// <summary>
        /// The event type for DataReceived
        /// </summary>
        public SerialData EventType
        {
            get { return m_EventType; }
        }
    }

    /// <summary>
    /// Delegate for DataReceived events
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Event arguments</param>
    public delegate void SerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e);

    /// <summary>
    /// Event related informaiton on ErrorReceived
    /// </summary>
    // Values are obtained from ClearCommErrors() Win32API
    public enum SerialError
    {
        /// <summary>
        /// Driver buffer has reached 80% full
        /// </summary>
        RXOver = 0x0001,

        /// <summary>
        /// Driver has detected an overflow
        /// </summary>
        Overrun = 0x0002,

        /// <summary>
        /// Parity error detected
        /// </summary>
        RXParity = 0x0004,

        /// <summary>
        /// Frame error detected
        /// </summary>
        Frame = 0x0008,

        /// <summary>
        /// Transmit buffer is full
        /// </summary>
        TXFull = 0x0100
    }

    /// <summary>
    /// Event Args for ErrorReceived
    /// </summary>
    public class SerialErrorReceivedEventArgs : EventArgs
    {
        private SerialError m_EventType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventType">Event that occurred</param>
        public SerialErrorReceivedEventArgs(SerialError eventType)
        {
            m_EventType = eventType;
        }

        /// <summary>
        /// The event type for ErrorReceived
        /// </summary>
        public SerialError EventType
        {
            get { return m_EventType; }
        }
    }

    /// <summary>
    /// Delegate for ErrorReceived events
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Event arguments</param>
    public delegate void SerialErrorReceivedEventHandler(object sender, SerialErrorReceivedEventArgs e); 

    /// <summary>
    /// Event related data on PinChanged
    /// </summary>
    public enum SerialPinChange
    {
        /// <summary>
        /// Clear To Send signal has changed
        /// </summary>
        CtsChanged = 0x08,

        /// <summary>
        /// Data Set Ready signal has changed
        /// </summary>
        DsrChanged = 0x10,

        /// <summary>
        /// Carrier Detect signal has changed
        /// </summary>
        CDChanged = 0x20,

        /// <summary>
        /// Break detected
        /// </summary>
        Break = 0x40,

        /// <summary>
        /// Ring signal has changed
        /// </summary>
        Ring = 0x100
    }

    /// <summary>
    /// Event Args for PinChanged
    /// </summary>
    public class SerialPinChangedEventArgs : EventArgs
    {
        private SerialPinChange m_EventType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="eventType">Event that occurred</param>
        public SerialPinChangedEventArgs(SerialPinChange eventType)
        {
            m_EventType = eventType;
        }

        /// <summary>
        /// The event type for ErrorReceived
        /// </summary>
        public SerialPinChange EventType
        {
            get { return m_EventType; }
        }
    }

    /// <summary>
    /// Delegate for PinChanged events
    /// </summary>
    /// <param name="sender">Object raising the event</param>
    /// <param name="e">Event arguments</param>
    public delegate void SerialPinChangedEventHandler(object sender, SerialPinChangedEventArgs e); 
#endregion
}
