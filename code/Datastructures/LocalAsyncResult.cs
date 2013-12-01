// $URL$
// $Id$

// Copyright © Jason Curl 2012-2013.
// See http://serialportstream.codeplex.com for license details (MS-PL License)

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RJCP.IO
{
    /// <summary>
    /// Provides a local implementation of an IAsyncResult
    /// </summary>
    internal class LocalAsync : IAsyncResult, IDisposable
    {
        private object m_State;
        private Lazy<ManualResetEvent> m_LazyHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAsync"/> class.
        /// </summary>
        /// <param name="state">The state object given by the user in a BeginWrite/EndWrite method</param>
        /// <param name="handle">The handle which indicates the object is signalled</param>
        /// <remarks>
        /// Provide a WaitHandle if you have one that will be signalled when the I/O operation is finished.
        /// You must still set the IsCompleted flag before you execute the users callback. If you do not
        /// provide a handle, but instead give <b>null</b> for <paramref name="handle"/>, then this object
        /// will create a ManualResetEvent for you automatically, that will be signalled when you set the
        /// <see cref="IsCompleted"/> property, or reset when you clear the property.
        /// </remarks>
        public LocalAsync(object state)
        {
            m_State = state;
            m_LazyHandle = new Lazy<ManualResetEvent>(() => {
                return new ManualResetEvent(m_IsCompleted);
            });
        }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information
        /// about an asynchronous operation.
        /// </summary>
        /// <returns>A user-defined object that qualifies or contains
        /// information about an asynchronous operation.</returns>
        public object AsyncState
        {
            get { return m_State; }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle" /> that
        /// is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>A <see cref="T:System.Threading.WaitHandle" /> that is
        /// used to wait for an asynchronous operation to complete.</returns>
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return m_LazyHandle.Value;
            }
        }

        private bool m_IsSynch;

        /// <summary>
        /// Gets a value that indicates whether the asynchronous
        /// operation completed synchronously.
        /// </summary>
        /// <returns>true if the asynchronous operation completed synchronously;
        /// otherwise, false.</returns>
        public bool CompletedSynchronously
        {
            get { return m_IsSynch; }
            internal set { m_IsSynch = value; }
        }

        private volatile bool m_IsCompleted;

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>true if the operation is complete; otherwise, false.</returns>
        public bool IsCompleted
        {
            get { return m_IsCompleted; }
            set
            {
                m_IsCompleted = value;
                if (m_LazyHandle.IsValueCreated) {
                    if (value) {
                        m_LazyHandle.Value.Set();
                    } else {
                        m_LazyHandle.Value.Reset();
                    }
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (m_LazyHandle.IsValueCreated) {
                m_LazyHandle.Value.Dispose();
            }
        }
    }

    internal class LocalAsync<T> : LocalAsync
    {
        private T m_Result;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAsync"/> class.
        /// </summary>
        /// <param name="state">The state object given by the user in a BeginWrite/EndWrite method</param>
        /// <param name="handle">The handle which indicates the object is signalled</param>
        /// <remarks>
        /// Provide a WaitHandle if you have one that will be signalled when the I/O operation is finished.
        /// You must still set the IsCompleted flag before you execute the users callback. If you do not
        /// provide a handle, but instead give <b>null</b> for <paramref name="handle"/>, then this object
        /// will create a ManualResetEvent for you automatically, that will be signalled when you set the
        /// <see name="IsCompleted"/> property, or reset when you clear the property.
        /// </remarks>
        public LocalAsync(object state) : base(state) { }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        internal T Result
        {
            get { return m_Result; }
            set { m_Result = value; }
        }
    }
}
