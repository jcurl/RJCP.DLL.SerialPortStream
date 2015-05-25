// $URL$
// $Id$

// Copyright © Jason Curl 2012-2014.
// See http://serialportstream.codeplex.com for license details (MS-PL License)

namespace RJCP.IO.Ports
{
    using System;
    using System.Text;

    /// <summary>
    /// A class containing information about a serial port.
    /// </summary>
    public class PortDescription
    {
        /// <summary>
        /// The name of the port
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// Description about the serial port.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="port">The name of the port.</param>
        /// <param name="description">Description about the serial port.</param>
        public PortDescription(string port, string description)
        {
            Port = port;
            Description = description;
        }
    }
}
