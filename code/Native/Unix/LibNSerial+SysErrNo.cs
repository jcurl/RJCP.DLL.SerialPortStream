// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Unix
{
    internal static partial class LibNSerial
    {
        /// <summary>
        /// Mapped C-Library errors to constants for Managed Code
        /// </summary>
        /// <remarks>>
        /// This table must be identical to the enumeration values defined in the C-Sources
        /// netfx.h and netfx.c.
        /// </remarks>
        public enum SysErrNo
        {
            /// <summary>
            /// No error detected.
            /// </summary>
            NETFX_OK = 0,

            /// <summary>
            /// ArgumentException
            /// </summary>
            NETFX_EINVAL = 1,

            /// <summary>
            /// UnauthorizedAccessException
            /// </summary>
            NETFX_EACCES = 2,

            /// <summary>
            /// OutOfMemoryException
            /// </summary>
            NETFX_ENOMEM = 3,

            /// <summary>
            /// InvalidOperationException
            /// </summary>
            NETFX_EBADF = 4,

            /// <summary>
            /// PlatformNotSupportedException
            /// </summary>
            NETFX_ENOSYS = 5,

            /// <summary>
            /// IOException
            /// </summary>
            NETFX_EIO = 6,

            /// <summary>
            /// No error.
            /// </summary>
            NETFX_EAGAIN = 7,

            /// <summary>
            /// No error.
            /// </summary>
            NETFX_EWOULDBLOCK = 8,

            /// <summary>
            /// No error.
            /// </summary>
            NETFX_EINTR = 9,

            /// <summary>
            /// Unmapped error. InvalidOperationException
            /// </summary>
            NETFX_UNKNOWN = -1
        }
    }
}