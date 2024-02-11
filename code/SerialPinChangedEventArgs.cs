namespace RJCP.IO.Ports
{
    using System;

    /// <summary>
    /// EventArgs for PinChanged.
    /// </summary>
    public class SerialPinChangedEventArgs : EventArgs
    {
        private readonly SerialPinChange m_EventType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventType">Event that occurred.</param>
        public SerialPinChangedEventArgs(SerialPinChange eventType)
        {
            m_EventType = eventType;
        }

        /// <summary>
        /// The event type for ErrorReceived.
        /// </summary>
        public SerialPinChange EventType
        {
            get { return m_EventType; }
        }
    }
}
