// $URL$
// $Id$

// Copyright © Jason Curl 2012
// See http://serialportstream.codeplex.com for license details (MS-PL License)

using System;
using System.Diagnostics;
using System.Text;

namespace RJCP.IO.Ports
{
    public partial class SerialPortStream
    {
        private static TraceSource m_Trace = new TraceSource("IO.Ports.SerialPortStream");
        private static TraceSource m_TraceRT = new TraceSource("IO.Ports.SerialPortStream_ReadTo");
    }
}
