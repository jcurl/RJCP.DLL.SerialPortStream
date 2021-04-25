namespace System.IO.Ports
{
    using System;

    /// <summary>
    /// Class SerialPort compatibility layer for .NET Stanard 1.5
    /// </summary>
    internal class SerialPort
    {
        /// <summary>
        /// Gets the port names.
        /// </summary>
        /// <returns>An empty array.</returns>
        /// <remarks>
        /// As .NET Standard doesn't contain this functionality, we provide it for the
        /// compiler and do nothing.
        /// </remarks>
        public static string[] GetPortNames()
        {
            return new string[0];
        }
    }
}
