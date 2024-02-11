namespace RJCP.IO.Ports.Native.Win32
{
    internal static partial class Kernel32
    {
        /// <summary>
        /// The file type of the specified file.
        /// </summary>
        public enum FileType
        {
            /// <summary>
            /// Either the type of the specified file is unknown, or the function failed.
            /// </summary>
            FILE_TYPE_UNKNOWN = 0x0000,

            /// <summary>
            /// The specified file is a disk file.
            /// </summary>
            FILE_TYPE_DISK = 0x0001,

            /// <summary>
            /// The specified file is a character file, typically an LPT device or a console.
            /// </summary>
            FILE_TYPE_CHAR = 0x0002,

            /// <summary>
            /// The specified file is a socket, a named pipe, or an anonymous pipe.
            /// </summary>
            FILE_TYPE_PIPE = 0x0003,

            /// <summary>
            /// Unused.
            /// </summary>
            FILE_TYPE_REMOTE = 0x8000
        }
    }
}
