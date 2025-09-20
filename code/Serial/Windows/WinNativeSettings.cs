namespace RJCP.IO.Ports.Serial.Windows
{
    using System;
    using System.Runtime.Versioning;

    [SupportedOSPlatform("windows")]
    internal class WinNativeSettings : IWinNativeSettings
    {
        private int m_ReadIntervalTimeout = 10;

        public int ReadIntervalTimeout
        {
            get { return m_ReadIntervalTimeout; }
            set
            {
                if (value != System.Threading.Timeout.Infinite)
                    ThrowHelper.ThrowIfNegative(value, nameof(ReadIntervalTimeout));

                m_ReadIntervalTimeout = value;
            }
        }

        private int m_ReadTotalTimeoutConstant = 100;

        public int ReadTotalTimeoutConstant
        {
            get { return m_ReadTotalTimeoutConstant; }
            set
            {
                ThrowHelper.ThrowIfNegative(value, nameof(ReadTotalTimeoutConstant));
                m_ReadTotalTimeoutConstant = value;
            }
        }

        private int m_ReadTotalTimeoutMultiplier;

        public int ReadTotalTimeoutMultiplier
        {
            get { return m_ReadTotalTimeoutMultiplier; }
            set
            {
                ThrowHelper.ThrowIfNegative(value, nameof(ReadTotalTimeoutMultiplier));
                m_ReadTotalTimeoutMultiplier = value;
            }
        }

        private int m_WriteTotalTimeoutConstant = 0;

        public int WriteTotalTimeoutConstant
        {
            get { return m_WriteTotalTimeoutConstant; }
            set
            {
                ThrowHelper.ThrowIfNegative(value, nameof(WriteTotalTimeoutConstant));
                m_WriteTotalTimeoutConstant = value;
            }
        }

        private int m_WriteTotalTimeoutMultiplier = 0;

        public int WriteTotalTimeoutMultiplier
        {
            get { return m_WriteTotalTimeoutMultiplier; }
            set
            {
                ThrowHelper.ThrowIfNegative(value, nameof(WriteTotalTimeoutMultiplier));
                m_WriteTotalTimeoutMultiplier = value;
            }
        }
    }
}
