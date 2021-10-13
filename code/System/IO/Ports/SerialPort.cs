namespace System.IO.Ports
{
    /// <summary>
    /// SerialPort compatibility layer for .NET Standard 2.1
    /// </summary>
    internal static class SerialPort
    {
        /// <summary>
        /// Gets the port names.
        /// </summary>
        /// <returns>An empty array.</returns>
        /// <remarks>
        /// As .NET Standard doesn't contain this functionality, we provide it for the compiler and do nothing.
        /// </remarks>
        public static string[] GetPortNames()
        {
            return Array.Empty<string>();
        }
    }
}
