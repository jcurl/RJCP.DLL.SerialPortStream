namespace RJCP.IO.Ports.Native.Windows
{
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.InteropServices;
	using Microsoft.Win32.SafeHandles;

	internal sealed class CommTimeouts
	{
		private const string ReadIntervalTimeoutKey = nameof(CommTimeouts) + "." + nameof(NativeMethods.COMMTIMEOUTS.ReadIntervalTimeout);
		private const string ReadTotalTimeoutConstantKey = nameof(CommTimeouts) + "." + nameof(NativeMethods.COMMTIMEOUTS.ReadTotalTimeoutConstant);
		private const string ReadTotalTimeoutMultiplierKey = nameof(CommTimeouts) + "." + nameof(NativeMethods.COMMTIMEOUTS.ReadTotalTimeoutMultiplier);
		private const string WriteTotalTimeoutConstantKey = nameof(CommTimeouts) + "." + nameof(NativeMethods.COMMTIMEOUTS.WriteTotalTimeoutConstant);
		private const string WriteTotalTimeoutMultiplierKey = nameof(CommTimeouts) + "." + nameof(NativeMethods.COMMTIMEOUTS.WriteTotalTimeoutMultiplier);

		private SafeFileHandle m_ComPortHandle;
		private NativeMethods.COMMTIMEOUTS m_CommTimeouts;

		public CommTimeouts(SafeFileHandle comPortHandle, IDictionary<string, object> settings)
		{
			m_ComPortHandle = comPortHandle;

			// Set the time outs
			m_CommTimeouts = new NativeMethods.COMMTIMEOUTS()
			{
				// We read only the data that is buffered
#if PL2303_WORKAROUNDS
                    // Time out if data hasn't arrived in 10ms, or if the read takes longer than 100ms in total
                    ReadIntervalTimeout = 10,
                    ReadTotalTimeoutConstant = 100,
                    ReadTotalTimeoutMultiplier = 0,
#else
				// Non-asynchronous behaviour
				ReadIntervalTimeout = System.Threading.Timeout.Infinite,
				ReadTotalTimeoutConstant = 0,
				ReadTotalTimeoutMultiplier = 0,
#endif
				// We have no time outs when writing
				WriteTotalTimeoutMultiplier = 0,
				WriteTotalTimeoutConstant = 500
			};

			// Update the timeouts with any user-provided values
			SetPlatformSpecificSettings(settings);
		}

		public void GetCommTimeouts()
		{
			if (!UnsafeNativeMethods.GetCommTimeouts(m_ComPortHandle, ref m_CommTimeouts))
			{
				throw new IOException("Unable to get comm timeouts", Marshal.GetLastWin32Error());
			}
		}

		public void SetCommTimeouts()
		{
			if (!UnsafeNativeMethods.SetCommTimeouts(m_ComPortHandle, ref m_CommTimeouts))
			{
				throw new IOException("Unable to set comm timeouts", Marshal.GetLastWin32Error());
			}
		}

		public void GetPlatformSpecificSettings(IDictionary<string, object> settings)
		{
			settings[ReadIntervalTimeoutKey] = m_CommTimeouts.ReadIntervalTimeout;
			settings[ReadTotalTimeoutConstantKey] = m_CommTimeouts.ReadTotalTimeoutConstant;
			settings[ReadTotalTimeoutMultiplierKey] = m_CommTimeouts.ReadTotalTimeoutMultiplier;
			settings[WriteTotalTimeoutConstantKey] = m_CommTimeouts.WriteTotalTimeoutConstant;
			settings[WriteTotalTimeoutMultiplierKey] = m_CommTimeouts.WriteTotalTimeoutMultiplier;
		}

		public void SetPlatformSpecificSettings(IDictionary<string, object> settings)
		{
			UpdateInt32Setting(settings, ReadIntervalTimeoutKey, ref m_CommTimeouts.ReadIntervalTimeout);
			UpdateInt32Setting(settings, ReadTotalTimeoutConstantKey, ref m_CommTimeouts.ReadTotalTimeoutConstant);
			UpdateInt32Setting(settings, ReadTotalTimeoutMultiplierKey, ref m_CommTimeouts.ReadTotalTimeoutMultiplier);
			UpdateInt32Setting(settings, WriteTotalTimeoutConstantKey, ref m_CommTimeouts.WriteTotalTimeoutConstant);
			UpdateInt32Setting(settings, WriteTotalTimeoutMultiplierKey, ref m_CommTimeouts.WriteTotalTimeoutMultiplier);
		}

		private void UpdateInt32Setting(IDictionary<string, object> settings, string key, ref int value)
		{
			if (settings.TryGetValue(key, out var obj) && obj is int i) value = i;
		}
	}
}