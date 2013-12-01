// $URL$
// $Id$

// Copyright © Jason Curl 2012-2013
// See http://serialportstream.codeplex.com for license details (MS-PL License)

namespace RJCP.IO.Ports
{
    using System;
    using System.Diagnostics;
    using System.Text;

    public partial class SerialPortStream
    {
        private static TraceSource m_Trace = new TraceSource("IO.Ports.SerialPortStream");
        private static TraceSource m_TraceRT = new TraceSource("IO.Ports.SerialPortStream_ReadTo");
    }
}
