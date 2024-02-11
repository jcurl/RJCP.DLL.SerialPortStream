namespace RJCP.IO.Ports.Serial
{
    using System;

    internal class VirtualSerialBuffer : SerialBuffer, IVirtualSerialBuffer
    {
        public event EventHandler<SerialBufferEventArgs> WriteEvent;

        private void OnWriteEvent(object sender, SerialBufferEventArgs args)
        {
            EventHandler<SerialBufferEventArgs> handler = WriteEvent;
            if (handler != null) handler(sender, args);
        }

        public event EventHandler<SerialBufferEventArgs> ReadEvent;

        private void OnReadEvent(object sender, SerialBufferEventArgs args)
        {
            EventHandler<SerialBufferEventArgs> handler = ReadEvent;
            if (handler != null) handler(sender, args);
        }

        public event EventHandler<SerialDataReceivedEventArgs> DataReceived;

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            EventHandler<SerialDataReceivedEventArgs> handler = DataReceived;
            if (handler != null) handler(sender, args);
        }

        private bool m_ReadRegistered;
        private bool m_WriteRegistered;

        protected override SerialReadBuffer GetSerialReadBuffer(bool pinned)
        {
            VirtualSerialReadBuffer buffer = new VirtualSerialReadBuffer(ReadBufferSize) {
                Encoding = Encoding
            };
            buffer.ReadEvent += OnReadEvent;
            buffer.DataReceived += OnDataReceived;
            m_ReadRegistered = true;
            return buffer;
        }

        protected override SerialWriteBuffer GetSerialWriteBuffer(bool pinned)
        {
            VirtualSerialWriteBuffer buffer = new VirtualSerialWriteBuffer(WriteBufferSize);
            buffer.WriteEvent += OnWriteEvent;
            m_WriteRegistered = true;
            return buffer;
        }

        public int ReadSentData(byte[] buffer, int offset, int count)
        {
            if (!m_WriteRegistered) return 0;
            return ((VirtualSerialWriteBuffer)SerialWrite).ReadSentData(buffer, offset, count);
        }

        public int WriteReceivedData(byte[] buffer, int offset, int count)
        {
            if (!m_ReadRegistered) return 0;
            return ((VirtualSerialReadBuffer)SerialRead).WriteReceivedData(buffer, offset, count);
        }

        public int SentDataLength
        {
            get
            {
                if (!m_WriteRegistered) return 0;
                return ((VirtualSerialWriteBuffer)SerialWrite).SentDataLength;
            }
        }

        public int SentDataFree
        {
            get
            {
                if (!m_WriteRegistered) return 0;
                return ((VirtualSerialWriteBuffer)SerialWrite).SentDataFree;
            }
        }

        public int ReceivedDataLength
        {
            get
            {
                if (!m_ReadRegistered) return 0;
                return ((VirtualSerialReadBuffer)SerialRead).ReceivedDataLength;
            }
        }

        public int ReceivedDataFree
        {
            get
            {
                if (!m_ReadRegistered) return 0;
                return ((VirtualSerialReadBuffer)SerialRead).ReceivedDataFree;
            }
        }
    }
}
