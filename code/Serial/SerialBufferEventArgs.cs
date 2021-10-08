// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System;

    /// <summary>
    /// An event argument for when the user reads or writes data for the low level serial API.
    /// </summary>
    public class SerialBufferEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerialBufferEventArgs"/> class.
        /// </summary>
        /// <param name="bytes">The number of bytes read or written to the buffers.</param>
        public SerialBufferEventArgs(int bytes)
        {
            Bytes = bytes;
        }

        /// <summary>
        /// Gets the number of bytes read or written to the buffers.
        /// </summary>
        /// <value>The number of bytes read or written to the buffers.</value>
        public int Bytes { get; private set; }
    }
}
