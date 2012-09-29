// $URL$
// $Id$
using System;
using System.Text;
using System.Diagnostics;

namespace RJCP.Datastructures
{
    /// <summary>
    /// A simple datastructure to manage an array as a circular buffer
    /// </summary>
    /// <remarks>
    /// This class provides simple methods for abstracting a circular buffer. A circular buffer
    /// allows for faster access of data by avoiding potential copy operations for data that
    /// is at the beginning.
    /// </remarks>
    /// <typeparam name="T">Type to use for the array</typeparam>
    [DebuggerDisplay("Start = {Start}; Length = {Length}; Free = {Free}")]
    internal class CircularBuffer<T>
    {
        /// <summary>
        /// Circular buffer itself. Exposed by property "Array"
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private T[] m_Array;

        /// <summary>
        /// Start index into the buffer. Exposed by property "Start"
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int m_Start;

        /// <summary>
        /// Length of data in circular buffer. Exposed by property "Length"
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int m_Count;

        /// <summary>
        /// Allocate an Array of type T[] of particular capacity
        /// </summary>
        /// <param name="capacity">Size of array to allocate</param>
        public CircularBuffer(int capacity)
        {
            m_Array = new T[capacity];
            m_Start = 0;
            m_Count = 0;
        }

        /// <summary>
        /// Circular buffer based on an already allocated array
        /// </summary>
        /// <remarks>
        /// The array is used as the storage for the circular buffer. No copy of the array
        /// is made. The initial index in the circular buffer is index 0 in the array. The
        /// array is assumed to be completely used (i.e. it is initialised with zero bytes
        /// Free).
        /// </remarks>
        /// <param name="array">Array (zero indexed) to allocate</param>
        public CircularBuffer(T[] array)
        {
            m_Array = array;
            m_Start = 0;
            m_Count = array.Length;
        }

        /// <summary>
        /// Circular buffer based on an already allocated array
        /// </summary>
        /// <remarks>
        /// The array is used as the storage for the circular buffer. No copy of the array
        /// is made, only a reference. The initial index in the array is 0. The value 
        /// <c>count</c> sets the initial length of the array. So an initial <c>count</c>
        /// of zero would imply an empty circular buffer.
        /// </remarks>
        /// <param name="array">Array (zero indexed) to allocate</param>
        /// <param name="count">Length of data in array, beginning from offset 0</param>
        public CircularBuffer(T[] array, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("count", "must be positive");
            if (count > array.Length)
                throw new ArgumentOutOfRangeException("Count (" + count.ToString() + 
                    ") exceeds the array boundaries (" + array.Length.ToString()+")");

            m_Array = array;
            m_Start = 0;
            m_Count = count;
        }

        /// <summary>
        /// Circular buffer based on an already allocated array
        /// </summary>
        /// <remarks>
        /// The array is used as the storage for the circular buffer. No copy of the array
        /// is made, only a reference. The <c>offset</c> is defined to be the first entry in the
        /// circular buffer. This may be any value from zero to the last index 
        /// (<c>Array.Length - 1</c>). The value <c>count</c> is the amount of data in the
        /// array, and it may cause wrapping (so that by setting offset near the end, a value
        /// of count may be set so that data can be considered at the end and beginning of
        /// the array given).
        /// </remarks>
        /// <param name="array">Array (zero indexed) to allocate</param>
        /// <param name="offset">Offset of first byte in the array</param>
        /// <param name="count">Length of data in array, wrapping to the start of the array</param>
        public CircularBuffer(T[] array, int offset, int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("count", "must be positive");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "must be positive");
            if (count > array.Length)
                throw new ArgumentOutOfRangeException("Count (" + count.ToString() +
                    ") exceeds the array boundaries (" + array.Length.ToString() + ")");
            if (offset >= array.Length)
                throw new ArgumentOutOfRangeException("Offset (" + offset.ToString() +
                    ") exceeds the array boundaries (" + (array.Length - 1).ToString() + ")");

            m_Array = array;
            m_Start = offset;
            m_Count = count;
        }

        /// <summary>
        /// Get start index into array where data begins
        /// </summary>
        public int Start { get { return m_Start; } }

        /// <summary>
        /// Get end index into array where data ends
        /// </summary>
        public int End { get { return (m_Start + m_Count) % m_Array.Length; } }

        /// <summary>
        /// Get total length of data in array
        /// </summary>
        public int Length { get { return m_Count; } }

        /// <summary>
        /// Get total free data in array
        /// </summary>
        public int Free { get { return m_Array.Length - m_Count; } }

        /// <summary>
        /// Get the total capacity of the array
        /// </summary>
        public int Capacity { get { return m_Array.Length; } }

        /// <summary>
        /// Convert an index from the start of the data to read to an array index
        /// </summary>
        /// <param name="index">Index in data</param>
        /// <returns>Index in array</returns>
        public int ToArrayIndex(int index) { return (m_Start + index) % m_Array.Length; }

        /// <summary>
        /// Get maximum amount of data that can be written in one operation
        /// </summary>
        /// <remarks>
        /// This function is useful if you need to pass the array to another function that will
        /// then fill the contents. You would pass <c>End</c> as the offset for writing data
        /// and <c>WriteLength</c> as the count. Then either the array is completely full, or
        /// until the end of the array.
        /// <para>Such a property is necessary in case that the free space wraps around the
        /// buffer. By calling X.Write(b.Array, b.End, b.WriteLength); b.Consume(b.WriteLength);
        /// and then testing if data is still free when we write again.</para>
        /// </remarks>
        public int WriteLength
        {
            get 
            {
                if (m_Start + m_Count >= m_Array.Length) return m_Array.Length - m_Count;
                return m_Array.Length - m_Start - m_Count;
            }
        }

        /// <summary>
        /// Get maximum amount of data that can be read in one operation
        /// </summary>
        /// <remarks>
        /// This function is useful if you need to pass the array to another function that will
        /// use the contents of the array. You would pass <c>Start</c> as the offset for reading
        /// data and <c>ReadLength</c> as the count. Then based on the amount of data operated
        /// on, you would free space with <c>Consume(ReadLength)</c>
        /// </remarks>
        public int ReadLength
        {
            get 
            {
                if (m_Start + m_Count >= m_Array.Length) return m_Array.Length - m_Start;
                return m_Count;
            }
        }

        /// <summary>
        /// Given an offset, calculate the length of data that can be read until the end of the
        /// block
        /// </summary>
        /// <remarks>
        /// Similar to the property <c>ReadLength</c>, this function takes an argument <c>offset</c>
        /// which is used to determine the length of data that can be read from that offset, until
        /// either the end of the block, or the end of the buffer.
        /// <para>This function is useful if you want to read a block of data, not starting from
        /// the offset 0 (and you don't want to consume the data before hand to reach an offset
        /// of zero)</para>
        /// <para>The example below, will calculate a checksum from the third byte in the block
        /// for the length of data. If the block to read from offset 3 can be done in one
        /// operation, it will do so. Else it must be done in two operations, first from offset
        /// 3 to the end, then from offset 0 for the remaining data</para>
        /// </remarks>
        /// <example>
        /// UInt16 crc;
        /// if (buffer.GetReadBlock(3) >= length - 3) {
        ///     crc = crc16.Compute(buffer.Array, buffer.ToArrayIndex(3), length - 3);
        /// } else {
        ///     crc = crc16.Compute(buffer.Array, buffer.ToArrayIndex(3), buffer.ReadLength - 3);
        ///     crc = crc16.Compute(crc, buffer.Array, 0, length - buffer.ReadLength);
        /// }
        /// </example>
        /// <param name="offset">Offset</param>
        /// <returns>Length</returns>
        public int GetReadBlock(int offset)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must be zero or greater");
            if (offset >= m_Count) return 0;

            int s = (m_Start + offset) % m_Array.Length;
            int c = m_Count - offset;

            if (s + c >= m_Array.Length) return m_Array.Length - s;
            return c;
        }

        /// <summary>
        /// Consume array elements (freeing space from the beginning) updating pointers in the circular buffer
        /// </summary>
        /// <remarks>
        /// This method advances the internal pointers for <i>Start</i> based on the <i>length</i>
        /// that should be consumed. The pointer <i>End</i> does not change. It is important that
        /// this method does not <i>Reset()</i> the buffer in case that all data is consumed. A
        /// common scenario with Streams is to write into the buffer using asynchronous I/O. If a
        /// <i>Reset()</i> occurs during an asynchronous I/O <i>ReadFile()</i>, the <i>End</i>
        /// pointer is also changed, so that when a <i>Produce()</i> occurs on completion of the
        /// <i>ReadFile()</i> operation, the pointers are updated, but not using the pointers
        /// before the <i>Reset()</i>. No crash would occur (so long as the underlying array is
        /// pinned), but data corruption would occur if this method were not used in this particular
        /// scenario.
        /// </remarks>
        /// <param name="length">Amount of data to consume</param>
        public void Consume(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "must be positive");
            if (length > m_Count) 
                throw new ArgumentOutOfRangeException("Can't consume more data than exists: Length=" + 
                    m_Count.ToString() + "; Consume=" + length.ToString());

            // Note, some implementations may rely on the pointers being correctly advanced also in
            // the case that data is consumed.
            m_Count -= length;
            m_Start = (m_Start + length) % m_Array.Length;
        }

        /// <summary>
        /// Produce bytes (allocating space at the end) updating pointers in the circular buffer
        /// </summary>
        /// <param name="length"></param>
        public void Produce(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "must be positive");
            if (m_Count + length > m_Array.Length)
                throw new ArgumentOutOfRangeException("length", "Can't produce more data than buffer size: Free=" +
                    Free.ToString() + "; Produce=" + length.ToString());
            m_Count += length;
        }

        /// <summary>
        /// Revert elements produced to the end of the circular buffer
        /// </summary>
        /// <param name="length"></param>
        public void Revert(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "must be positive");
            if (m_Count < length) throw new ArgumentOutOfRangeException("length", "must be less than number of elements in the circular buffer");

            m_Count -= length;
        }

        /// <summary>
        /// Reset the pointers in the circular buffer
        /// </summary>
        public void Reset()
        {
            m_Count = 0;
            m_Start = 0;
        }

        /// <summary>
        /// Get the reference to the array that's allocated
        /// </summary>
        public T[] Array { get { return m_Array; } }

        /// <summary>
        /// Access an element in the array using the Start as index 0
        /// </summary>
        /// <param name="index">Index into the array referenced from <i>Start</i></param>
        /// <returns>Contents of the array</returns>
        public T this[int index]
        {
            get { return m_Array[(m_Start + index) % m_Array.Length]; }
            set { m_Array[(m_Start + index) % m_Array.Length] = value; }
        }

        /// <summary>
        /// Copy data from array to the end of this circular buffer and update the length
        /// </summary>
        /// <remarks>
        /// Data is copied to the end of the Circular Buffer. The amount of data
        /// that could be copied is dependent on the amount of free space. The result
        /// is the number of elements from the <c>buffer</c> array that is copied
        /// into the Circular Buffer. Pointers in the circular buffer are updated
        /// appropriately.
        /// </remarks>
        /// <param name="array">Array to copy from</param>
        /// <returns>Number of bytes copied</returns>
        public int Append(T[] array)
        {
            if (array == null) throw new ArgumentNullException("array");
            return Append(array, 0, array.Length);
        }

        /// <summary>
        /// Copy data from array to the end of this circular buffer and update the length
        /// </summary>
        /// <remarks>
        /// Data is copied to the end of the Circular Buffer. The amount of data
        /// that could be copied is dependent on the amount of free space. The result
        /// is the number of elements from the <c>buffer</c> array that is copied
        /// into the Circular Buffer. Pointers in the circular buffer are updated
        /// appropriately.
        /// </remarks>
        /// <param name="array">Array to copy from</param>
        /// <param name="offset">Offset to copy data from</param>
        /// <param name="count">Length of data to copy</param>
        /// <returns>Number of bytes copied</returns>
        public int Append(T[] array, int offset, int count)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (offset < 0 || count < 0) throw new ArgumentOutOfRangeException("Offset/Count must be positive");
            if (offset + count > array.Length) throw new ArgumentException("Parameters exceed array boundary");
            if (m_Count == Capacity) return 0;
            if (count == 0) return 0;

            if (count <= WriteLength) {
                System.Array.Copy(array, offset, m_Array, End, count);
                Produce(count);
                return count;
            } else {
                count = Math.Min(Free, count);
                System.Array.Copy(array, offset, m_Array, End, WriteLength);
                System.Array.Copy(array, WriteLength, m_Array, 0, count - WriteLength);
                Produce(count);
                return count;
            }
        }

        /// <summary>
        /// Copy data from the circular buffer to the end of this circular buffer
        /// </summary>
        /// <remarks>
        /// Data is copied to the end of the Circular Buffer. The amount of data
        /// that could be copied is dependent on the amount of free space. The result
        /// is the number of elements from the <c>buffer</c> array that is copied
        /// into the Circular Buffer. Pointers in the cirucular buffer are updated
        /// appropriately.
        /// </remarks>
        /// <param name="buffer">Buffer to append</param>
        /// <returns>Amount of data appended</returns>
        public int Append(CircularBuffer<T> buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            return Append(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Copy data from the circular buffer to the end of this circular buffer
        /// </summary>
        /// <remarks>
        /// Data is copied to the end of the Circular Buffer. The amount of data
        /// that could be copied is dependent on the amount of free space. The result
        /// is the number of elements from the <c>buffer</c> array that is copied
        /// into the Circular Buffer. Pointers in the cirucular buffer are updated
        /// appropriately.
        /// </remarks>
        /// <param name="buffer">Buffer to append</param>
        /// <param name="count">Number of bytes to append</param>
        /// <returns>Amount of data appended</returns>
        public int Append(CircularBuffer<T> buffer, int count)
        {
            return Append(buffer, 0, count);
        }

        /// <summary>
        /// Copy data from the circular buffer to the end of this circular buffer
        /// </summary>
        /// <remarks>
        /// Data is copied to the end of the Circular Buffer. The amount of data
        /// that could be copied is dependent on the amount of free space. The result
        /// is the number of elements from the <c>buffer</c> array that is copied
        /// into the Circular Buffer. Pointers in the cirucular buffer are updated
        /// appropriately.
        /// </remarks>
        /// <param name="buffer">Buffer to append</param>
        /// <param name="count">Number of bytes to append</param>
        /// <param name="offset">Offset into the buffer to start appending</param>
        /// <returns>Amount of data appended</returns>
        public int Append(CircularBuffer<T> buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0 || count < 0) throw new ArgumentOutOfRangeException("Offset/Count must be positive");
            if (offset + count > buffer.Length) throw new ArgumentException("Parameters exceed buffer boundary");
            if (m_Count == Capacity) return 0;
            if (count == 0) return 0;

            int o = (buffer.Start + offset) % buffer.Capacity;
            int c = Math.Min(Free, count);
            int r = c;

            while (r > 0) {
                int rl = (o + r >= buffer.Capacity) ? (buffer.Capacity - o) : r;
                int cp = Math.Min(r, WriteLength);
                cp = Math.Min(cp, rl);
                System.Array.Copy(buffer.Array, o, m_Array, End, cp);
                Produce(cp);
                r -= cp;
                o = (o + cp) % buffer.Capacity;
            }
            return c;
        }

        /// <summary>
        /// Append a single element to the end of the Circular Buffer
        /// </summary>
        /// <param name="element">The element to add at the end of the buffer</param>
        /// <returns>Amount of data appended. 1 if successful, 0 if no space available</returns>
        public int Append(T element)
        {
            if (m_Count == Capacity) return 0;

            m_Array[this.End] = element;
            Produce(1);
            return 1;
        }

        /// <summary>
        /// Retrieve a single element from the Circular buffer and consume it
        /// </summary>
        /// <returns>The value at index 0</returns>
        public T Pop()
        {
            if (m_Count == 0) throw new InvalidOperationException("Circular Buffer is empty");
            T result = m_Array[m_Start];
            Consume(1);
            return result;
        }

        /// <summary>
        /// Copy data from the circular buffer to the array and then consume the data from the circular buffer
        /// </summary>
        /// <remarks>
        /// Data is copied to the first element in the array, up to the length
        /// of the array.
        /// </remarks>
        /// <param name="array">The array to copy the data to</param>
        /// <returns>The number of bytes that were moved</returns>
        public int MoveTo(T[] array)
        {
            int l = CopyTo(array);
            Consume(l);
            return l;
        }

        /// <summary>
        /// Copy data from the circular buffer to the array and then consume the data from the circular buffer
        /// </summary>
        /// <param name="array">The array to copy the data to</param>
        /// <param name="offset">Offset into the array to copy to</param>
        /// <param name="count">Amount of data to copy to</param>
        /// <returns>The number of bytes that were moved</returns>
        public int MoveTo(T[] array, int offset, int count)
        {
            int l = CopyTo(array, offset, count);
            Consume(l);
            return l;
        }

        /// <summary>
        /// Copy data from the circular buffer to the array
        /// </summary>
        /// <remarks>
        /// Data is copied from the first element in the array, up to the length
        /// of the array. The data from the Circular Buffer is <i>not</i> consumed. 
        /// You must do this yourself. Else use the MoveTo() method.
        /// </remarks>
        /// <param name="array">The array to copy the data to</param>
        /// <returns>The number of bytes that were copied</returns>
        public int CopyTo(T[] array)
        {
            if (array == null) throw new ArgumentNullException("array");
            return CopyTo(array, 0, array.Length);
        }

        /// <summary>
        /// Copy data from the circular buffer to the array
        /// </summary>
        /// <remarks>
        /// Data is copied from the circular buffer into the array specified, at the offset given.
        /// The data from the Circular Buffer is <i>not</i> consumed. You must do this yourself.
        /// Else use the MoveTo() method.
        /// </remarks>
        /// <param name="array">The array to copy the data to</param>
        /// <param name="offset">Offset into the array to copy to</param>
        /// <param name="count">Amount of data to copy to</param>
        /// <returns>The number of bytes that were copied</returns>
        public int CopyTo(T[] array, int offset, int count)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (count == 0) return 0;
            if (offset < 0 || count < 0) throw new ArgumentOutOfRangeException("offset / count", "Offset and Count must be positive");
            if (array.Length < offset + count) throw new ArgumentException("Offset and count exceed boundary length");

            int length = Math.Min(count, Length);
            if (ReadLength >= count) {
                // The block of data is one continuous block to copy
                System.Array.Copy(m_Array, Start, array, offset, length);
                return length;
            } else {
                // The block of data wraps over
                System.Array.Copy(m_Array, Start, array, offset, ReadLength);
                System.Array.Copy(m_Array, 0, array, offset + ReadLength, length - ReadLength);
                return length;
            }
        }
    }

    /// <summary>
    /// A set of useful extensions to the CircularBuffer for specific data types
    /// </summary>
    internal static class CircularBufferExtensions
    {
        /// <summary>
        /// Convert the contents of the circular buffer into a string
        /// </summary>
        /// <param name="buff">The circular buffer based on char</param>
        /// <returns>A string</returns>
        public static string GetString(this CircularBuffer<char> buff)
        {
            if (buff == null) return null;
            return buff.GetString(buff.Length);
        }

        /// <summary>
        /// Convert the contents of the circular buffer into a string
        /// </summary>
        /// <param name="buff">The circular buffer based on char</param>
        /// <param name="length">Number of characters to convert to a string</param>
        /// <returns>A string</returns>
        public static string GetString(this CircularBuffer<char> buff, int length)
        {
            if (buff == null) return null;
            if (length == 0) return string.Empty;
            if (length > buff.Length) length = buff.Length;
            if (buff.Start + length > buff.Capacity) {
                StringBuilder sb = new StringBuilder(length);
                sb.Append(buff.Array, buff.Start, buff.Capacity - buff.Start);
                sb.Append(buff.Array, 0, length + buff.Start - buff.Capacity);
                return sb.ToString();
            } else {
                return new string(buff.Array, buff.Start, length);
            }
        }

        /// <summary>
        /// Convert the contents of the circular buffer into a string
        /// </summary>
        /// <param name="buff">The circular buffer based on char</param>
        /// <param name="offset">The offset into the circular buffer</param>
        /// <param name="length">Number of characters to convert to a string</param>
        /// <returns>A string</returns>
        public static string GetString(this CircularBuffer<char> buff, int offset, int length)
        {
            if (buff == null) return null;
            if (length == 0) return string.Empty;
            if (offset > buff.Length) return string.Empty;
            if (offset + length > buff.Length) length = buff.Length - offset;

            int start = (buff.Start + offset) % buff.Capacity;
            if (start + length > buff.Capacity) {
                StringBuilder sb = new StringBuilder(length);
                sb.Append(buff.Array, start, buff.Capacity - start);
                sb.Append(buff.Array, 0, length + start - buff.Capacity);
                return sb.ToString();
            } else {
                return new string(buff.Array, start, length);
            }
        }

        /// <summary>
        /// Use a decoder to convert from a Circular Buffer of bytes into a char array
        /// </summary>
        /// <param name="decoder">The decoder to do the conversion.</param>
        /// <param name="bytes">The circular buffer of bytes to convert from.</param>
        /// <param name="chars">An array to store the converted characters.</param>
        /// <param name="charIndex">The first element of <i>chars</i> in which data is stored.</param>
        /// <param name="charCount">Maximum number of characters to write.</param>
        /// <param name="flush"><b>true</b> to indicate that no further data is to be converted; otherwise, <b>false</b>.</param>
        /// <param name="bytesUsed">When this method returns, contains the number of bytes that were
        /// used in the conversion. This parameter is passed uninitialized.</param>
        /// <param name="charsUsed">When this method returns, contains the number of characters from
        /// chars that were produced by the conversion. This parameter is passed uninitialized.</param>
        /// <param name="completed">When this method returns, contains true if all the characters
        /// specified by byteCount were converted; otherwise, false. This parameter is
        /// passed uninitialized.</param>
        public static void Convert(this Decoder decoder, CircularBuffer<byte> bytes, char[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            int bu;
            int cu;

            if (bytes == null) throw new ArgumentNullException("bytes", "Circular buffer bytes may not be null");
            if (chars == null) throw new ArgumentNullException("chars", "Array chars may not be null");
            if (charIndex < 0) throw new ArgumentOutOfRangeException("charIndex", "Negative offset provided");
            if (charCount < 0) throw new ArgumentOutOfRangeException("charCount", "Negative count provided");
            if (chars.Length - charIndex < charCount) throw new ArgumentException("charIndex and charCount exceed char buffer boundaries");

            completed = true;
            bytesUsed = 0;
            charsUsed = 0;

            if (bytes.ReadLength == 0) return;
            if (charCount == 0) {
                completed = false;
                return;
            }

            while (bytes.ReadLength > 0 && charCount > 0) {
                decoder.Convert(bytes.Array, bytes.Start, bytes.ReadLength, 
                    chars, charIndex, charCount, 
                    false, out bu, out cu, out completed);
                bytes.Consume(bu);
                bytesUsed += bu;
                charCount -= cu;
                charsUsed += cu;
                charIndex += cu;
                if (!completed) return;
            }

            if (flush) {
                // We don't use 'flush' in the loop above, as we really can't tell when the conversion
                // should be complete (without having to precalculate the number of bytes we need). So
                // if we have space in the output buffer, we do a last conversion (bytes.ReadLength
                // should be zero here).
                if (charCount > 0) {
                    decoder.Convert(bytes.Array, bytes.Start, bytes.ReadLength,
                        chars, charIndex, charCount,
                        true, out bu, out cu, out completed);
                    bytes.Consume(bu);
                    bytesUsed += bu;
                    charCount -= cu;
                    charsUsed += cu;
                    charIndex += cu;
                } else {
                    completed = false;
                }
            }
        }

        /// <summary>
        /// Use a decoder to convert from a Circular Buffer of bytes into a Circular Buffer of chars
        /// </summary>
        /// <param name="decoder">The decoder to do the conversion</param>
        /// <param name="bytes">The circular buffer of bytes to convert from</param>
        /// <param name="chars">The circular buffer of chars to convert to</param>
        /// <param name="charCount">Maximum number of characters to write</param>
        /// <param name="flush"><b>true</b> to indicate that no further data is to be converted; otherwise, <b>false</b>.</param>
        /// <param name="bytesUsed">When this method returns, contains the number of bytes that were
        /// used in the conversion. This parameter is passed uninitialized.</param>
        /// <param name="charsUsed">When this method returns, contains the number of characters from
        /// chars that were produced by the conversion. This parameter is passed uninitialized.</param>
        /// <param name="completed">When this method returns, contains true if all the characters
        /// specified by byteCount were converted; otherwise, false. This parameter is
        /// passed uninitialized.</param>
        public static void Convert(this Decoder decoder, CircularBuffer<byte> bytes, CircularBuffer<char> chars, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            int bu;
            int cu;

            if (bytes == null) throw new ArgumentNullException("bytes", "Circular buffer bytes may not be null");
            if (chars == null) throw new ArgumentNullException("chars", "Circular buffer chars may not be null");

            completed = true;
            bytesUsed = 0;
            charsUsed = 0;

            if (bytes.ReadLength == 0) return;
            if (charCount == 0) {
                completed = false;
                return;
            }

            charCount = Math.Min(chars.Free, charCount);
            while (bytes.ReadLength > 0 && charCount > 0) {
                decoder.Convert(bytes.Array, bytes.Start, bytes.ReadLength,
                    chars.Array, chars.End, Math.Min(chars.WriteLength, charCount),
                    false, out bu, out cu, out completed);
                bytes.Consume(bu);
                chars.Produce(cu);
                bytesUsed += bu;
                charCount -= cu;
                charsUsed += cu;
            }

            if (flush) {
                // We don't use 'flush' in the loop above, as we really can't tell when the conversion
                // should be complete (without having to precalculate the number of bytes we need). So
                // if we have space in the output buffer, we do a last conversion (bytes.ReadLength
                // should be zero here).
                if (charCount > 0) {
                    decoder.Convert(bytes.Array, bytes.Start, bytes.ReadLength,
                        chars.Array, chars.End, charCount,
                        true, out bu, out cu, out completed);
                    bytes.Consume(bu);
                    chars.Produce(cu);
                    bytesUsed += bu;
                    charCount -= cu;
                    charsUsed += cu;
                } else {
                    completed = false;
                }
            }
        }

        /// <summary>
        /// Use a decoder to convert from a Circular Buffer of bytes into a Circular Buffer of chars
        /// </summary>
        /// <param name="decoder">The decoder to do the conversion</param>
        /// <param name="bytes">The circular buffer of bytes to convert from</param>
        /// <param name="chars">The circular buffer of chars to convert to</param>
        /// <param name="flush"><b>true</b> to indicate that no further data is to be converted; otherwise, <b>false</b>.</param>
        /// <param name="bytesUsed">When this method returns, contains the number of bytes that were
        /// used in the conversion. This parameter is passed uninitialized.</param>
        /// <param name="charsUsed">When this method returns, contains the number of characters from
        /// chars that were produced by the conversion. This parameter is passed uninitialized.</param>
        /// <param name="completed">When this method returns, contains true if all the characters
        /// specified by byteCount were converted; otherwise, false. This parameter is
        /// passed uninitialized.</param>
        public static void Convert(this Decoder decoder, CircularBuffer<byte> bytes, CircularBuffer<char> chars, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            if (bytes == null) throw new ArgumentNullException("bytes", "Circular buffer bytes may not be null");
            if (chars == null) throw new ArgumentNullException("chars", "Circular buffer chars may not be null");

            int bu;
            int cu;

            completed = true;
            bytesUsed = 0;
            charsUsed = 0;

            if (bytes.ReadLength == 0) return;
            if (chars.WriteLength == 0) {
                completed = false;
                return;
            }

            while (bytes.ReadLength > 0 && chars.WriteLength > 0) {
                decoder.Convert(bytes.Array, bytes.Start, bytes.ReadLength,
                    chars.Array, chars.End, chars.WriteLength,
                    false, out bu, out cu, out completed);
                bytes.Consume(bu);
                chars.Produce(cu);
                bytesUsed += bu;
                charsUsed += cu;
            }
            if (flush) {
                // We don't use 'flush' in the loop above, as we really can't tell when the conversion
                // should be complete (without having to precalculate the number of bytes we need). So
                // if we have space in the output buffer, we do a last conversion (bytes.ReadLength
                // should be zero here).
                if (chars.WriteLength > 0) {
                    decoder.Convert(bytes.Array, bytes.Start, bytes.ReadLength,
                        chars.Array, chars.End, chars.WriteLength,
                        true, out bu, out cu, out completed);
                    bytes.Consume(bu);
                    chars.Produce(cu);
                    bytesUsed += bu;
                    charsUsed += cu;
                } else {
                    completed = false;
                }
            }
        }

        /// <summary>
        /// Converts an array of Unicode characters to a byte sequence storing the result in a circular buffer
        /// </summary>
        /// <param name="encoder">The encoder to use for the conversion</param>
        /// <param name="chars">An array of characters to convert</param>
        /// <param name="charIndex">The first element of <i>chars</i> to convert</param>
        /// <param name="charCount">The number of elements of <i>chars</i> to convert</param>
        /// <param name="bytes">Circular buffer where converted bytes are stored</param>
        /// <param name="flush"><b>true</b> to indicate no further data is to be converted; otherwise, false</param>
        /// <param name="charsUsed">When this method returns, contains the number of characters from
        /// chars that were produced by the conversion. This parameter is passed uninitialized.</param>
        /// <param name="bytesUsed">When this method returns, contains the number of bytes that were
        /// used in the conversion. This parameter is passed uninitialized.</param>
        /// <param name="completed">When this method returns, contains true if all the characters
        /// specified by byteCount were converted; otherwise, false. This parameter is
        /// passed uninitialized.</param>
        public static void Convert(this Encoder encoder, char[] chars, int charIndex, int charCount, CircularBuffer<byte> bytes, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
        {
            if (chars == null) throw new ArgumentNullException("chars", "chars may not be null");
            if (bytes == null) throw new ArgumentNullException("bytes", "Circular buffer bytes may not be null");
            if (charIndex < 0) throw new ArgumentOutOfRangeException("charIndex", "Negative offset provided");
            if (charCount < 0) throw new ArgumentOutOfRangeException("charCount", "Negative count provided");
            if (chars.Length - charIndex < charCount) throw new ArgumentException("charIndex and charCount exceed char buffer boundaries");

            int bu;
            int cu;

            completed = true;
            bytesUsed = 0;
            charsUsed = 0;
            if (charCount == 0) return;
            if (bytes.WriteLength == 0) {
                completed = false;
                return;
            }

            // The encoder will not cache UCS16 bytes in between. It converts one chraacter at a time. If there
            // is insufficient buffer space in the output, it will not fill it up.

            while (bytes.WriteLength > 0 && charCount > 0) {
                int bf = bytes.WriteLength;
                encoder.Convert(chars, charIndex, charCount, 
                    bytes.Array, bytes.End, bf,
                    false, out cu, out bu, out completed);
                charIndex += cu;
                charCount -= cu;
                bytesUsed += bu;
                charsUsed += cu;
                bytes.Produce(bu);

                if (!completed && bu < bf) {
                    // The decoder couldn't encode the complete byte, so we must manually convert
                    // the next character and split it up for the circular buffer
                    byte[] sc = new byte[128];
                    encoder.Convert(chars, charIndex, 1, sc, 0, sc.Length, false, out cu, out bu, out completed);
                    if (bu <= bytes.Free) {
                        charIndex += cu;
                        charCount -= cu;
                        bytesUsed += bu;
                        charsUsed += cu;
                        bytes.Append(sc, 0, bu);
                    } else {
                        // Couldn't atomically write the last byte, so we exit
                        charCount = 0;
                        completed = false;
                    }
                }
            }

            if (flush) {
                // Not implemented. Because I have no example where this is used to be tested. For example, 
                // UTF8 will not write partially to an output byte buffer if it doesn't contain enough
                // space, hence the workaround above with (!completed && bytesUsed < bf).
            }
        }
    }
}
