namespace RJCP.IO.Ports.Serial
{
    /// <summary>
    /// Configuration settings specific for a Windows Serial Port
    /// </summary>
    public interface IWinNativeSettings
    {
        /// <summary>
        /// Gets or sets the maximum time allowed to elapse before the arrival of the next byte on the communications
        /// line, in milliseconds.
        /// </summary>
        /// <value>
        /// The maximum time allowed to elapse before the arrival of the next byte on the communications line, in
        /// milliseconds.
        /// </value>
        /// <remarks>
        /// If the interval between the arrival of any two bytes exceeds this amount, the ReadFile operation is
        /// completed and any buffered data is returned. A value of zero indicates that interval time-outs are not used.
        /// </remarks>
        int ReadIntervalTimeout { get; set; }

        /// <summary>
        /// Gets or sets a constant used to calculate the total time-out period for read operations, in milliseconds.
        /// </summary>
        /// <value>A constant used to calculate the total time-out period for read operations, in milliseconds.</value>
        /// <remarks>
        /// For each read operation, this value is added to the product of the <see cref="ReadTotalTimeoutMultiplier"/>
        /// member and the requested number of bytes. A value of zero for both the
        /// <see cref="ReadTotalTimeoutMultiplier"/> and <see cref="ReadTotalTimeoutConstant"/> members indicates that
        /// total time-outs are not used for read operations.
        /// </remarks>
        int ReadTotalTimeoutConstant { get; set; }

        /// <summary>
        /// Gets or sets the multiplier used to calculate the total time-out period for read operations, in
        /// milliseconds.
        /// </summary>
        /// <value>
        /// The multiplier used to calculate the total time-out period for read operations, in milliseconds.
        /// </value>
        /// <remarks>
        /// For each read operation, this value is multiplied by the requested number of bytes to be read.
        /// </remarks>
        int ReadTotalTimeoutMultiplier { get; set; }

        /// <summary>
        /// Gets or sets a constant used to calculate the total time-out period for write operations, in milliseconds.
        /// </summary>
        /// <value>A constant used to calculate the total time-out period for write operations, in milliseconds.</value>
        /// <remarks>
        /// For each write operation, this value is added to the product of the
        /// <see cref="WriteTotalTimeoutMultiplier"/> member and the number of bytes to be written. A value of zero for
        /// both the <see cref="WriteTotalTimeoutMultiplier"/> and <see cref="WriteTotalTimeoutConstant"/> members
        /// indicates that total time-outs are not used for write operations.
        /// </remarks>
        int WriteTotalTimeoutConstant { get; set; }

        /// <summary>
        /// Gets or sets the multiplier used to calculate the total time-out period for write operations, in
        /// milliseconds.
        /// </summary>
        /// <value>
        /// The multiplier used to calculate the total time-out period for write operations, in milliseconds.
        /// </value>
        /// <remarks>For each write operation, this value is multiplied by the number of bytes to be written.</remarks>
        int WriteTotalTimeoutMultiplier { get; set; }
    }
}
