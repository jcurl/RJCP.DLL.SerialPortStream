namespace RJCP.IO.Ports.Native.Windows
{
    using System;

    internal class CommEventArgs : EventArgs
    {
        private NativeMethods.SerialEventMask m_EventType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventType">The event results.</param>
        public CommEventArgs(NativeMethods.SerialEventMask eventType)
        {
            m_EventType = eventType;
        }

        /// <summary>
        /// The event bit field.
        /// </summary>
        public NativeMethods.SerialEventMask EventType
        {
            get { return m_EventType; }
        }
    }
}
