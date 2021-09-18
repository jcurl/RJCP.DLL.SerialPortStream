// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        [Flags]
        public enum FileShare
        {
            /// <summary>
            /// Prevents other processes from opening a file or device if they request to delete, read or write access.
            /// </summary>
            FILE_SHARE_NONE = 0x00000000,

            /// <summary>
            /// Enables subsequent open operations on an object to request read access.
            /// </summary>
            /// <remarks>
            /// Enables subsequent open operations on an object to request read access. Otherwise, other processes
            /// cannot open the object if they request read access. If this flag is not specified, but the object has
            /// been opened for read access, the function fails.
            /// </remarks>
            FILE_SHARE_READ = 0x00000001,

            /// <summary>
            /// Enables subsequent open operations on an object to request write access.
            /// </summary>
            /// <remarks>
            /// Enables subsequent open operations on an object to request write access. Otherwise, other processes
            /// cannot open the object if they request write access. If this flag is not specified, but the object has
            /// been opened for write access, the function fails.
            /// </remarks>
            FILE_SHARE_WRITE = 0x00000002,

            /// <summary>
            /// Enables subsequent open operations on an object to request delete access.
            /// </summary>
            /// <remarks>
            /// Enables subsequent open operations on an object to request delete access. Otherwise, other processes
            /// cannot open the object if they request delete access. If this flag is not specified, but the object has
            /// been opened for delete access, the function fails.
            /// </remarks>
            FILE_SHARE_DELETE = 0x00000004
        }
    }
}
