namespace RJCP.IO.Ports.Serial.Windows
{
    /// <summary>
    /// Type of DTR (Data Terminal Ready) control to use.
    /// </summary>
    internal enum DtrControl
    {
        /// <summary>
        /// Disable DTR line.
        /// </summary>
        Disable = 0,

        /// <summary>
        /// Enable DTR line.
        /// </summary>
        Enable = 1,

        /// <summary>
        /// DTR Handshaking.
        /// </summary>
        Handshake = 2
    }
}
