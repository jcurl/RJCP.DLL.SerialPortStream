// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Serial
{
    using System;
    using Buffer;

    internal sealed class SerialWriteBuffer : MemoryWriteBuffer
    {
        public SerialWriteBuffer(int length, bool pinned)
            : base(length, pinned) { }

        public event EventHandler WriteEvent;

        private void OnWriteEvent(object sender, EventArgs args)
        {
            EventHandler handler = WriteEvent;
            if (handler != null) {
                handler(sender, args);
            }
        }

        protected override void OnWrite(int count)
        {
            OnWriteEvent(this, new EventArgs());
        }
    }
}
