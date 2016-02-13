// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using System;
    using System.Diagnostics;

    public partial class SerialPortStream
    {
        private static TraceSource m_Trace = new TraceSource("IO.Ports.SerialPortStream");
        private static TraceSource m_TraceRT = new TraceSource("IO.Ports.SerialPortStream_ReadTo");
    }
}
