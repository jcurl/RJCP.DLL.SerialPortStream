namespace RJCP.IO.Ports
{
    using System;

    /// <summary>
    /// EventArgs for DataReceived.
    /// </summary>
    public class SerialDataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventType">Event that occurred.</param>
        public SerialDataReceivedEventArgs(SerialData eventType)
        {
            EventType = eventType;
        }

        /// <summary>
        /// The event type for DataReceived.
        /// </summary>
        public SerialData EventType { get; private set; }
    }
}
