// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Windows
{
    using System;
    using Native.Win32;

    internal class CommErrorEventArgs : EventArgs
    {
        private Kernel32.ComStatErrors m_EventType;

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
