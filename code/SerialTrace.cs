// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using System;
    using System.Diagnostics;

    internal static class SerialTrace
    {
        private static object s_SyncRoot = new object();
        private static volatile TraceSource s_Trace;
        private static volatile TraceSource s_TraceRT;

        /// <summary>
        /// Gets the trace source object for serial port logging.
        /// </summary>
        /// <value>
        /// The trace source object for serial port logging.
        /// </value>
        public static TraceSource TraceSer
        {
            get
            {
                if (s_Trace == null) {
                    lock (s_SyncRoot) {
                        if (s_Trace == null) {
                            s_Trace = new TraceSource("IO.Ports.SerialPortStream");
                        }
                    }
                }
                return s_Trace;
            }
        }

        /// <summary>
        /// Gets the trace source object for the ReadTo line implementation.
        /// </summary>
        /// <value>
        /// The trace source object for the ReadTo line implementation.
        /// </value>
        public static TraceSource TraceRT
        {
            get
            {
                if (s_TraceRT == null) {
                    lock (s_SyncRoot) {
                        if (s_TraceRT == null) {
                            s_TraceRT = new TraceSource("IO.Ports.SerialPortStream_ReadTo");
                        }
                    }
                }
                return s_TraceRT;
            }
        }

        private static int s_RefCounter;

        /// <summary>
        /// Adds a reference to using the tracing objects.
        /// </summary>
        /// <remarks>
        /// When you add a reference to this object, you should remove the reference with the
        /// <see cref="Close"/> method. When all references are gone, the trace objects are
        /// also closed.
        /// </remarks>
        public static void AddRef()
        {
            lock (s_SyncRoot) {
                s_RefCounter++;
            }
        }

        /// <summary>
        /// Closes trace instances.
        /// </summary>
        public static void Close()
        {
            lock (s_SyncRoot) {
                if (s_RefCounter == 1) {
                    if (s_TraceRT != null) {
                        s_TraceRT.Close();
                        s_TraceRT = null;
                    }
                    if (s_Trace != null) {
                        s_Trace.Close();
                        s_Trace = null;
                    }
                }
                if (s_RefCounter > 0) --s_RefCounter;
            }
        }
    }
}
