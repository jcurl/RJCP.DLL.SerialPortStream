// $URL$
// $Id$

// Copyright © Jason Curl 2012-2013.
// See http://serialportstream.codeplex.com for license details (MS-PL License)

using System;
using System.Diagnostics;
using System.Text;

namespace RJCP.Datastructures
{
    /// <summary>
    /// A class to maintain how much time is remaining since the last reset, until
    /// expiry.
    /// </summary>
    /// <remarks>
    /// Often used when implementing timeouts in other methods, this class can
    /// provide the remaining time, in units of milliseconds, that can be used
    /// with many Operating System calls as an expiry time.
    /// <para>One example is the System.Threading.WaitHandle.WaitAny() which
    /// expects a timeout parameter. Call the Reset() method at the beginning
    /// of the timeout operation. Then on return of the function, if no other
    /// operation occurred, the RemainingTime() method should return 0 indicating
    /// that the timer has expired</para>
    /// <para>Another thread can be programmed to Reset() the timer class during
    /// a timeout operation, so that even if the result of Wait operation by the
    /// Operating system resulted in a timeout, a Reset(), which results in the
    /// RemainingTime() being more than 0 milliseconds, indicates that another
    /// wait operation should occur.</para>
    /// <para>Even if no expiry is to occur, but the Operating System function
    /// returns early, you can opt to restart the timeout operation which will
    /// then take into account the current time and reduce the timeout so that
    /// the operation ends as expected</para>
    /// </remarks>
    internal sealed class TimerExpiry
    {
        private Stopwatch m_StopWatch = new Stopwatch();
        private int m_Milliseconds;

        /// <summary>
        /// Constructor. Initialise expiry based on the current time
        /// </summary>
        /// <param name="milliseconds"></param>
        public TimerExpiry(int milliseconds)
        {
            Timeout = milliseconds;
        }

        /// <summary>
        /// The time for expiry on the next reset. -1 indicates no expiry
        /// </summary>
        public int Timeout
        {
            get { return m_Milliseconds; }
            set
            {
                m_StopWatch.Reset();
                if (value < 0) {
                    m_Milliseconds = -1;
                } else {
                    m_Milliseconds = value;
                    if (value > 0) m_StopWatch.Start();
                }
            }
        }

        /// <summary>
        /// Estimate the amount of time remaining from when this function is called
        /// until expiry
        /// </summary>
        /// <returns>The time to expiry in milliseconds</returns>
        public int RemainingTime()
        {
            if (m_Milliseconds < 0) return -1;

            long elapsed = m_StopWatch.ElapsedMilliseconds;
            if (elapsed >= m_Milliseconds) return 0;
            return (int)(m_Milliseconds - elapsed);
        }

        /// <summary>
        /// Test if the timer expiry has expired
        /// </summary>
        public bool Expired
        {
            get { return RemainingTime() == 0; }
        }

        /// <summary>
        /// Reset the timeout so it occurs with the given Timeout
        /// </summary>
        public void Reset()
        {
            if (m_Milliseconds > 0) {
                m_StopWatch.Reset();
                m_StopWatch.Start();
            }
        }
    }
}
