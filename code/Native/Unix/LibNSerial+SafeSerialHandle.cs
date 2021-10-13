// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Unix
{
    using System;
    using System.Runtime.InteropServices;

    internal static partial class LibNSerial
    {
        public class SafeSerialHandle : SafeHandle
        {
            public SafeSerialHandle() : base(IntPtr.Zero, true) { }

            public override bool IsInvalid
            {
                get
                {
                    return handle.Equals(IntPtr.Zero);
                }
            }

            protected override bool ReleaseHandle()
            {
                Dll.serial_terminate(handle);
                return true;
            }
        }
    }
}