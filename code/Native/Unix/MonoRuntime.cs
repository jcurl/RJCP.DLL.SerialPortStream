// Copyright © Jason Curl 2012-2016
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Unix
{
    using System;

    internal static class MonoRuntime
    {
        public static bool s_RuntimeFound = false;
        public static bool s_MonoRuntime = false;

        public static bool IsMonoRuntime()
        {
            if (s_RuntimeFound) return s_MonoRuntime;
            s_MonoRuntime = Type.GetType("Mono.Runtime") != null;
            s_RuntimeFound = true;
            return s_MonoRuntime;
        }
    }
}

