﻿// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial.Windows
{
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using Microsoft.Win32.SafeHandles;
    using Native.Win32;

    /// <summary>
    /// Abstracts the Win32 API GetCommProperties().
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class CommProperties
    {
        private readonly SafeFileHandle m_ComPortHandle;
        private Kernel32.CommProp m_CommProp = new Kernel32.CommProp();

        internal CommProperties(SafeFileHandle handle)
        {
            m_ComPortHandle = handle;
            m_CommProp.wPacketLength = 64;
            m_CommProp.dwProvSpec1 = Kernel32.COMMPROP_INITIALIZED;
        }

        public void GetCommProperties()
        {
            if (!Kernel32.GetCommProperties(m_ComPortHandle, out m_CommProp)) {
                throw new IOException("Unable to get Serial Port properties", Marshal.GetLastWin32Error());
            }
        }

        public int MaxTxQueue { get { return (int)m_CommProp.dwMaxTxQueue; } }
        public int MaxRxQueue { get { return (int)m_CommProp.dwMaxRxQueue; } }
        public int CurrentTxQueue { get { return (int)m_CommProp.dwCurrentTxQueue; } }
        public int CurrentRxQueue { get { return (int)m_CommProp.dwCurrentRxQueue; } }

        public bool IsSettableParity { get { return m_CommProp.dwSettableParams[(int)Kernel32.SettableParams.SP_PARITY]; } }
        public bool IsSettableBaud { get { return m_CommProp.dwSettableParams[(int)Kernel32.SettableParams.SP_BAUD]; } }
        public bool IsSettableDataBits { get { return m_CommProp.dwSettableParams[(int)Kernel32.SettableParams.SP_DATABITS]; } }
        public bool IsSettableStopbits { get { return m_CommProp.dwSettableParams[(int)Kernel32.SettableParams.SP_STOPBITS]; } }
        public bool IsSettableHandshaking { get { return m_CommProp.dwSettableParams[(int)Kernel32.SettableParams.SP_HANDSHAKING]; } }
        public bool IsSettableParityCheck { get { return m_CommProp.dwSettableParams[(int)Kernel32.SettableParams.SP_PARITY_CHECK]; } }
        public bool IsSettableRlsd { get { return m_CommProp.dwSettableParams[(int)Kernel32.SettableParams.SP_RLSD]; } }

        public bool IsDtrDsrSupported { get { return m_CommProp.dwProvCapabilities[(int)Kernel32.ProvCapabilities.PCF_DTRDSR]; } }
        public bool IsRtsCtsSupported { get { return m_CommProp.dwProvCapabilities[(int)Kernel32.ProvCapabilities.PCF_RTSCTS]; } }
        public bool IsRlsdSupported { get { return m_CommProp.dwProvCapabilities[(int)Kernel32.ProvCapabilities.PCF_RLSD]; } }
        public bool IsParityCheckSupported { get { return m_CommProp.dwProvCapabilities[(int)Kernel32.ProvCapabilities.PCF_PARITY_CHECK]; } }
        public bool IsXOnXOffSupported { get { return m_CommProp.dwProvCapabilities[(int)Kernel32.ProvCapabilities.PCF_XONXOFF]; } }
        public bool IsXCharSupported { get { return m_CommProp.dwProvCapabilities[(int)Kernel32.ProvCapabilities.PCF_SETXCHAR]; } }
        public bool IsTotalTimeoutsSupported { get { return m_CommProp.dwProvCapabilities[(int)Kernel32.ProvCapabilities.PCF_TOTALTIMEOUTS]; } }
        public bool IsIntervalTimeoutsSupported { get { return m_CommProp.dwProvCapabilities[(int)Kernel32.ProvCapabilities.PCF_INTTIMEOUTS]; } }
        public bool IsSpecialCharsSupported { get { return m_CommProp.dwProvCapabilities[(int)Kernel32.ProvCapabilities.PCF_SPECIALCHARS]; } }
        public bool Is16BitSupported { get { return m_CommProp.dwProvCapabilities[(int)Kernel32.ProvCapabilities.PCF_16BITMODE]; } }

        public bool IsUnspecifiedType { get { return m_CommProp.dwProvSubType == Kernel32.ProvSubType.PST_UNSPECIFIED; } }
        public bool IsRs232Type { get { return m_CommProp.dwProvSubType == Kernel32.ProvSubType.PST_RS232; } }
        public bool IsParallelPortType { get { return m_CommProp.dwProvSubType == Kernel32.ProvSubType.PST_PARALLELPORT; } }
        public bool IsRs422Type { get { return m_CommProp.dwProvSubType == Kernel32.ProvSubType.PST_RS422; } }
        public bool IsRs423Type { get { return m_CommProp.dwProvSubType == Kernel32.ProvSubType.PST_RS423; } }
        public bool IsRs499Type { get { return m_CommProp.dwProvSubType == Kernel32.ProvSubType.PST_RS449; } }
        public bool IsModemType { get { return m_CommProp.dwProvSubType == Kernel32.ProvSubType.PST_MODEM; } }
        public bool IsFaxType { get { return m_CommProp.dwProvSubType == Kernel32.ProvSubType.PST_FAX; } }

        /// <summary>
        /// Check if the number of data bits is settable.
        /// </summary>
        /// <param name="databits">The number of data bits the user wants to set.</param>
        /// <returns><see langword="true"/> if the number of data bits is indicated as supported.</returns>
        public bool IsValidDataBits(int databits)
        {
            if ((databits < 5 || databits > 8) && databits != 16) return false;
            switch (databits) {
            case 5: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableData.DATABITS_5];
            case 6: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableData.DATABITS_6];
            case 7: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableData.DATABITS_7];
            case 8: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableData.DATABITS_8];
            case 16: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableData.DATABITS_16];
            default: return false;
            }
        }

        /// <summary>
        /// Check if the number of stop bits is settable.
        /// </summary>
        /// <param name="stopbits">The number of stop bits the user wants to set.</param>
        /// <returns><see langword="true"/> if the number of stop bits is indicated as supported.</returns>
        public bool IsValidStopBits(StopBits stopbits)
        {
            switch (stopbits) {
            case StopBits.One: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableStopParity.STOPBITS_10];
            case StopBits.One5: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableStopParity.STOPBITS_15];
            case StopBits.Two: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableStopParity.STOPBITS_20];
            default: return false;
            }
        }

        /// <summary>
        /// Check if the parity is supported.
        /// </summary>
        /// <param name="parity">The parity the user wants to set.</param>
        /// <returns><see langword="true"/> if the parity is indicated as supported.</returns>
        public bool IsValidParity(Parity parity)
        {
            switch (parity) {
            case Parity.None: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableStopParity.PARITY_NONE];
            case Parity.Odd: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableStopParity.PARITY_ODD];
            case Parity.Even: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableStopParity.PARITY_EVEN];
            case Parity.Mark: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableStopParity.PARITY_MARK];
            case Parity.Space: return m_CommProp.dwSettableDataStopParity[(int)Kernel32.SettableStopParity.PARITY_SPACE];
            default: return false;
            }
        }

        /// <summary>
        /// Check if the baud rate value given is settable.
        /// </summary>
        /// <remarks>
        /// This function relies on the <c>dwSettableBaud</c> parameter. It does not rely on the <c>dwMaxBaud</c>
        /// parameter as if they don't match, then this is an inconsistency in the Windows serial driver.
        /// </remarks>
        /// <param name="baudrate">The baud rate to check for.</param>
        /// <returns><see langword="true"/> if the baud rate is indicated to be supported.</returns>
        public bool IsValidBaud(int baudrate)
        {
            if (baudrate <= 0) return false;
            switch (baudrate) {
            case 75: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_075];
            case 110: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_110];
            case 134: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_134_5];
            case 150: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_150];
            case 300: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_300];
            case 600: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_600];
            case 1200: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_1200];
            case 1800: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_1800];
            case 2400: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_2400];
            case 4800: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_4800];
            case 7200: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_7200];
            case 9600: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_9600];
            case 14400: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_14400];
            case 19200: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_19200];
            case 38400: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_38400];
            case 56000: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_56K];
            case 57600: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_57600];
            case 115200: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_115200];
            case 128000: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_128K];
            default: return m_CommProp.dwSettableBaud[(int)Kernel32.MaxBaud.BAUD_USER];
            }
        }
    }
}
