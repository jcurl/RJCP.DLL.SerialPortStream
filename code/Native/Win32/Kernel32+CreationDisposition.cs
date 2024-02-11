namespace RJCP.IO.Ports.Native.Win32
{
    internal static partial class Kernel32
    {
        public enum CreationDisposition
        {
            /// <summary>
            /// Creates a new file. The function fails if a specified file exists.
            /// </summary>
            CREATE_NEW = 1,

            /// <summary>
            /// Creates a new file, always.
            /// </summary>
            /// <remarks>
            /// If a file exists, the function overwrites the file, clears the existing attributes, combines the
            /// specified file attributes, and flags with FILE_ATTRIBUTE_ARCHIVE, but does not set the security
            /// descriptor that the SECURITY_ATTRIBUTES structure specifies.
            /// </remarks>
            CREATE_ALWAYS = 2,

            /// <summary>
            /// Opens a file. The function fails if the file does not exist.
            /// </summary>
            OPEN_EXISTING = 3,

            /// <summary>
            /// Opens a file, always.
            /// </summary>
            /// <remarks>
            /// If a file does not exist, the function creates a file as if dwCreationDisposition is CREATE_NEW.
            /// </remarks>
            OPEN_ALWAYS = 4,

            /// <summary>
            /// Opens a file and truncates it so that its size is 0 (zero) bytes. The function fails if the file does
            /// not exist.
            /// </summary>
            /// <remarks>The calling process must open the file with the GENERIC_WRITE access right.</remarks>
            TRUNCATE_EXISTING = 5
        }
    }
}
