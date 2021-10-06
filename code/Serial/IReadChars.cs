// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System.Text;

    /// <summary>
    /// An interface exposing methods for reading characters from a byte buffer.
    /// </summary>
    public interface IReadChars
    {
        /// <summary>
        /// Gets or sets the encoding to use when reading the byte buffer.
        /// </summary>
        /// <value>The encoding to use when reading the byte buffer.</value>
        Encoding Encoding { get; set; }

        /// <summary>
        /// Waits for the next character for reading to be available.
        /// </summary>
        /// <param name="timeout">The timeout, in milliseconds.</param>
        /// <returns>
        /// Is <see langword="true"/> if a new character is available, <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// This method doesn't wait for a single character to read, but for the next available character to add to the
        /// internal buffer. It is used often after <see cref="ReadTo(string, out string)"/> which might not have found
        /// the data, and this method can be used to wait for new data to arrive.
        /// </remarks>
        bool WaitForReadChar(int timeout);

        /// <summary>
        /// Reads characters from the underlying byte buffer into the specified character buffer.
        /// </summary>
        /// <param name="buffer">The character buffer to write the results.</param>
        /// <param name="offset">The offset into the character buffer to write to.</param>
        /// <param name="count">The length of data to write into <paramref name="buffer"/>.</param>
        /// <returns>The number of characters written into the buffer.</returns>
        int Read(char[] buffer, int offset, int count);

        /// <summary>
        /// Reads the next available character.
        /// </summary>
        /// <returns>The UTF16 character as an integer.</returns>
        int ReadChar();

        /// <summary>
        /// Reads the byte buffer up to a given string (the read-to string).
        /// </summary>
        /// <param name="text">The text to read to.</param>
        /// <param name="line">The line that was read up until the <paramref name="text"/>.</param>
        /// <returns>Is <see langword="true"/> if data was read, <see langword="false"/> otherwise.</returns>
        bool ReadTo(string text, out string line);

        /// <summary>
        /// Reads the existing byte buffer into a string and return it.
        /// </summary>
        /// <returns>The currently received character data received as a string.</returns>
        string ReadExisting();
    }
}
