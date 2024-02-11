namespace RJCP.IO.Ports.Serial.Windows
{
    using System;
    using System.Runtime.Versioning;
    using Native.Win32;

    [SupportedOSPlatform("windows")]
    internal class CommEventArgs : EventArgs
    {
        private readonly Kernel32.SerialEventMask m_EventType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventType">The event results.</param>
        public CommEventArgs(Kernel32.SerialEventMask eventType)
        {
            m_EventType = eventType;
        }

        /// <summary>
        /// The event bit field.
        /// </summary>
        public Kernel32.SerialEventMask EventType
        {
            get { return m_EventType; }
        }
    }
}
