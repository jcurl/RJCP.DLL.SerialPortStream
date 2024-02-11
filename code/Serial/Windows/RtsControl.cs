namespace RJCP.IO.Ports.Serial.Windows
{
    /// <summary>
    /// RTS (Request to Send) to use.
    /// </summary>
    internal enum RtsControl
    {
        /// <summary>
        /// Disable RTS line.
        /// </summary>
        Disable = 0,

        /// <summary>
        /// Enable the RTS line.
        /// </summary>
        Enable = 1,

        /// <summary>
        /// RTS Handshaking.
        /// </summary>
        Handshake = 2,

        /// <summary>
        /// RTS Toggling.
        /// </summary>
        Toggle = 3
    }
}
