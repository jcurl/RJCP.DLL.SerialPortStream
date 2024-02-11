﻿namespace RJCP.IO.Ports.Native.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Threading;
    using Microsoft.Win32.SafeHandles;

    [SuppressUnmanagedCodeSecurity]
    [SupportedOSPlatform("windows")]
    internal static partial class Kernel32
    {
        /// <summary>
        /// Set wProvSpec1 to COMMPROP_INITIALIZED to indicate that wPacketLength is valid before a call to
        /// GetCommProperties().
        /// </summary>
        public const uint COMMPROP_INITIALIZED = 0xE73CF52E;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW", ExactSpelling = true)]
        public static extern SafeFileHandle CreateFile(string lpFileName,
            FileAccess dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes,
            CreationDisposition dwCreationDisposition, FileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool WriteFile(SafeFileHandle hFile, IntPtr lpBuffer,
            uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, ref NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool ReadFile(SafeFileHandle hFile, IntPtr lpBuffer,
            uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, ref NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetOverlappedResult(SafeFileHandle hFile,
           [In] ref NativeOverlapped lpOverlapped, out uint lpNumberOfBytesTransferred, bool bWait);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetCommProperties(SafeFileHandle hFile, out CommProp lpCommProp);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetCommModemStatus(SafeFileHandle hFile, out ModemStat lpModemStat);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetCommMask(SafeFileHandle hFile, out SerialEventMask lpEvtMask);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetCommMask(SafeFileHandle hFile, SerialEventMask dwEvtMask);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool WaitCommEvent(SafeFileHandle hFile, out SerialEventMask lpEvtMask,
            [In] ref NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetCommTimeouts(SafeFileHandle hFile, out COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetCommTimeouts(SafeFileHandle hFile, [In] ref COMMTIMEOUTS lpCommTimeouts);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool ClearCommError(SafeFileHandle hFile, out ComStatErrors lpErrors, out COMSTAT lpStat);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool ClearCommError(SafeFileHandle hFile, out ComStatErrors lpErrors, IntPtr lpStat);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool PurgeComm(SafeFileHandle hFile, PurgeFlags dwFlags);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool GetCommState(SafeFileHandle hFile, ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetCommState(SafeFileHandle hFile, [In] ref DCB lpDCB);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool CancelIo(SafeFileHandle hFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern FileType GetFileType(SafeFileHandle hFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetCommBreak(SafeFileHandle hFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool ClearCommBreak(SafeFileHandle hFile);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool EscapeCommFunction(SafeFileHandle hFile, ExtendedFunctions dwFunc);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool SetupComm(SafeFileHandle hFile, int dwInQueue, int dwOutQueue);
    }
}
