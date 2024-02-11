namespace RJCP.IO.Ports
{
    /// <summary>
    /// The type of parity to use.
    /// </summary>
    public enum Parity
    {
        /// <summary>
        /// No parity.
        /// </summary>
        None = 0,

        /// <summary>
        /// Odd parity.
        /// </summary>
        Odd = 1,

        /// <summary>
        /// Even parity.
        /// </summary>
        Even = 2,

        /// <summary>
        /// Mark parity.
        /// </summary>
        Mark = 3,

        /// <summary>
        /// Space parity.
        /// </summary>
        Space = 4
    }
}
