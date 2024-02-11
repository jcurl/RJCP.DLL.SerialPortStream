﻿namespace RJCP.IO.Ports.Serial
{
    /// <summary>
    /// Interface for accessing serial based streams.
    /// </summary>
    /// <typeparam name="T">The type for configuring the native serial.</typeparam>
    public interface INativeSerial<out T> : INativeSerial
    {
        /// <summary>
        /// Gets a reference to the configurable settings for the serial port.
        /// </summary>
        /// <value>The configurable settings for the serial port.</value>
        T Settings { get; }
    }
}
