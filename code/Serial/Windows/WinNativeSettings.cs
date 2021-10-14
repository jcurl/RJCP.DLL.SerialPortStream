// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial.Windows
{
    using System;

    internal class WinNativeSettings : IWinNativeSettings
    {
        private int m_ReadIntervalTimeout = 10;

        public int ReadIntervalTimeout
        {
            get { return m_ReadIntervalTimeout; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(ReadIntervalTimeout));
                m_ReadIntervalTimeout = value;
            }
        }

        private int m_ReadTotalTimeoutConstant = 100;

        public int ReadTotalTimeoutConstant
        {
            get { return m_ReadTotalTimeoutConstant; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(ReadTotalTimeoutConstant));
                m_ReadTotalTimeoutConstant = value;
            }
        }

        private int m_ReadTotalTimeoutMultiplier;

        public int ReadTotalTimeoutMultiplier
        {
            get { return m_ReadTotalTimeoutMultiplier; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(ReadTotalTimeoutMultiplier));
                m_ReadTotalTimeoutMultiplier = value;
            }
        }

        private int m_WriteTotalTimeoutConstant = 500;

        public int WriteTotalTimeoutConstant
        {
            get { return m_WriteTotalTimeoutConstant; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(WriteTotalTimeoutConstant));
                m_WriteTotalTimeoutConstant = value;
            }
        }

        private int m_WriteTotalTimeoutMultiplier = 0;

        public int WriteTotalTimeoutMultiplier
        {
            get { return m_WriteTotalTimeoutMultiplier; }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(WriteTotalTimeoutMultiplier));
                m_WriteTotalTimeoutMultiplier = value;
            }
        }
    }
}
