namespace RJCP.IO.Ports.Serial.Windows
{
    using System;
    using System.Runtime.Versioning;
    using Native.Win32;

    [SupportedOSPlatform("windows")]
    internal class CommErrorEventArgs : EventArgs
    {
        private readonly Kernel32.ComStatErrors m_EventType;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventType"><see cref="Kernel32.ComStatErrors"/> result.</param>
        public CommErrorEventArgs(Kernel32.ComStatErrors eventType)
        {
            m_EventType = eventType;
        }

        /// <summary>
        /// <see cref="Kernel32.ComStatErrors"/> result.
        /// </summary>
        public Kernel32.ComStatErrors EventType
        {
            get { return m_EventType; }
        }
    }
}
