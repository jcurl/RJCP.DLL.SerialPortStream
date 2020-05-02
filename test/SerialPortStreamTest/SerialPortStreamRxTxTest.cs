// Copyright © Jason Curl 2012-2020
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.SerialPortStreamTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using NUnit.Framework;

    /// <summary>
    /// Test sending and receiving data via two serial ports.
    /// </summary>
    /// <remarks>
    /// You will need to have two serial ports connected to each other on the same computer using a NULL modem cable.
    /// Alternatively, you can use a software emulated serial port, such as com0com for tests.
    /// <para>
    /// You need to update the variables c_SourcePort and c_DestPort to be the names of the two serial ports.
    /// </para>
    /// </remarks>
    [TestFixture(Category = "SerialPortStream")]
    [Timeout(10000)]
    public class SerialPortStreamRxTxTest
    {
        private readonly string SourcePort = SerialConfiguration.SourcePort;
        private readonly string DestPort = SerialConfiguration.DestPort;
        private const int TimeOut = 300;

        [Test]
        [Timeout(20000)]
        public void SendReceive()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                // Send Maximum data in one go
                byte[] sendbuf = new byte[src.WriteBufferSize];
                Random r = new Random();
                r.NextBytes(sendbuf);
                src.Write(sendbuf, 0, sendbuf.Length);

                // Receive sent data
                int rcv = 0;
                int c = 0;
                byte[] rcvbuf = new byte[sendbuf.Length + 10];
                while (rcv < rcvbuf.Length) {
                    Console.WriteLine("Begin Receive: Offset=" + rcv + "; Count=" + (rcvbuf.Length - rcv));
                    int b = dst.Read(rcvbuf, rcv, rcvbuf.Length - rcv);
                    if (b == 0) {
                        if (c == 0) break;
                        c++;
                    } else {
                        c = 0;
                    }
                    rcv += b;
                }

                bool dump = false;
                if (rcv != sendbuf.Length) {
                    Console.WriteLine("Read length not the same as the amount of data sent (got " + rcv + " bytes)");
                    dump = true;
                }
                for (int i = 0; i < sendbuf.Length; i++) {
                    if (sendbuf[i] != rcvbuf[i]) {
                        Console.WriteLine("Comparison failure at " + i);
                        dump = true;
                        break;
                    }
                }

                if (dump) {
                    Console.WriteLine("Send Buffer DUMP");
                    for (int i = 0; i < sendbuf.Length; i++) {
                        Console.WriteLine(sendbuf[i].ToString("X2"));
                    }

                    Console.WriteLine("Receive Buffer DUMP");
                    for (int i = 0; i < rcv; i++) {
                        Console.WriteLine(rcvbuf[i].ToString("X2"));
                    }
                }
                src.Close();
                dst.Close();
                Assert.That(dump, Is.False, "Error in transfer");
            }
        }

        private class SendReceiveAsyncState
        {
            public ManualResetEvent finished = new ManualResetEvent(false);
            public SerialPortStream src;
            public SerialPortStream dst;
            public byte[] sendBuf;
            public byte[] recvBuf;
            public int rcv;
        }

        [Test]
        [Timeout(20000)]
        public void SendReceiveWithBeginEnd()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                SendReceiveAsyncState state = new SendReceiveAsyncState {
                    src = src,
                    dst = dst,
                    sendBuf = new byte[src.WriteBufferSize],
                    recvBuf = new byte[src.WriteBufferSize + 10]
                };
                Random r = new Random();
                r.NextBytes(state.sendBuf);

                // Here we start the read and write in parallel. The read will wait up to c_Timeout for the first byte.
                dst.BeginRead(state.recvBuf, 0, state.recvBuf.Length, SendReceiveAsyncReadComplete, state);
                src.BeginWrite(state.sendBuf, 0, state.sendBuf.Length, SendReceiveAsyncWriteComplete, state);
                if (!state.finished.WaitOne(30000)) {
                    Assert.Fail("BeginWrite/BeginRead test case timeout");
                }
            }
        }

        private void SendReceiveAsyncWriteComplete(IAsyncResult ar)
        {
            SendReceiveAsyncState state = (SendReceiveAsyncState)ar.AsyncState;
            state.src.EndWrite(ar);
        }

        private void SendReceiveAsyncReadComplete(IAsyncResult ar)
        {
            SendReceiveAsyncState state = (SendReceiveAsyncState)ar.AsyncState;
            int bytes = state.dst.EndRead(ar);

            if (bytes != 0) {
                state.rcv += bytes;
                if (state.rcv < state.recvBuf.Length) {
                    Console.WriteLine("Begin Receive: Offset=" + state.rcv + "; Count=" + (state.recvBuf.Length - state.rcv));
                    state.dst.BeginRead(state.recvBuf, state.rcv, state.recvBuf.Length - state.rcv, SendReceiveAsyncReadComplete, state);
                } else {
                    state.finished.Set();
                    Assert.Fail("Received more data than expected");
                }
            } else {
                if (state.rcv != state.sendBuf.Length) {
                    state.finished.Set();
                    Assert.Fail("Received more/less data than expected: {0} expected; {1} received", state.sendBuf.Length, state.rcv);
                }
                state.finished.Set();
            }
        }

        [Test]
        public void SendAndFlush1()
        {
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                // Required for com0com to send the data (the remote side must receive it)
                dst.Open();

                src.Open();
                src.WriteLine("Connected");
                src.Flush();
            }
        }

        [Test]
        public void SendAndFlush2()
        {
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                // Required for com0com to send the data (the remote side must receive it)
                dst.Open();

                Console.WriteLine("1. Open()");
                src.Open();
                Console.WriteLine("2. WriteLine()");
                src.WriteLine("Connected");
                Console.WriteLine("3. Sleep()");
                Thread.Sleep(100);
                Console.WriteLine("4. WriteLine()");
                src.WriteLine("Disconnected");
                Console.WriteLine("5. Flush()");
                src.Flush();
                Console.WriteLine("6. Dispose()");
            }
        }

        [Test]
        public void Flush()
        {
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;
                src.Open(); Assert.That(src.IsOpen, Is.True);
                dst.Open(); Assert.That(dst.IsOpen, Is.True);

                byte[] sdata = new byte[512];
                for (int i = 0; i < sdata.Length; i++) {
                    sdata[i] = (byte)(64 + i % 48);
                }

                // It should take 512 * 10 / 115200 s = 44ms to send, timeout of 300ms.
                src.Write(sdata, 0, sdata.Length);
                src.Flush();
                Assert.That(src.BytesToWrite, Is.EqualTo(0));

                src.Write(sdata, 0, sdata.Length);
                src.Flush();
                Assert.That(src.BytesToWrite, Is.EqualTo(0));
            }
        }

        [Test]
        public void WaitForRxCharEventOn1Byte()
        {
            using (ManualResetEvent rxChar = new ManualResetEvent(false))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.Open(); dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;

                dst.ReceivedBytesThreshold = 1;
                dst.DataReceived += (s, a) => { rxChar.Set(); };
                src.WriteByte(0x00);
                Assert.That(rxChar.WaitOne(1000), Is.True);
            }
        }

        [Test]
        public void WaitForRxCharEventOn2Bytes()
        {
            using (ManualResetEvent rxChar = new ManualResetEvent(false))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.Open(); dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;

                dst.ReceivedBytesThreshold = 2;
                dst.DataReceived += (s, a) => { rxChar.Set(); };
                src.WriteByte(0x00);
                Assert.That(rxChar.WaitOne(1000), Is.False);
                src.WriteByte(0x01);
                Assert.That(rxChar.WaitOne(1000), Is.True);
            }
        }

        [Test]
        public void WaitForRxCharEventOnEofChar()
        {
            using (ManualResetEvent rxChar = new ManualResetEvent(false))
            using (ManualResetEvent eofChar = new ManualResetEvent(false))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.Open(); dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;

                dst.ReceivedBytesThreshold = 5;
                dst.DataReceived += (s, a) => {
                    if (a.EventType == SerialData.Chars) rxChar.Set();
                    if (a.EventType == SerialData.Eof) eofChar.Set();
                };
                src.WriteByte(0x00);
                Assert.That(rxChar.WaitOne(500), Is.False);
                Assert.That(eofChar.WaitOne(0), Is.False);
                src.WriteByte(0x01);
                Assert.That(rxChar.WaitOne(500), Is.False);
                Assert.That(eofChar.WaitOne(0), Is.False);
                src.WriteByte(0x1A);
                Assert.That(rxChar.WaitOne(500), Is.False);
                Assert.That(eofChar.WaitOne(0), Is.True);
                eofChar.Reset();
                src.WriteByte(0x02);
                Assert.That(rxChar.WaitOne(500), Is.False);
                Assert.That(eofChar.WaitOne(0), Is.False);
                src.WriteByte(0x03);
                Assert.That(rxChar.WaitOne(500), Is.True);
                Assert.That(eofChar.WaitOne(0), Is.False);
            }
        }

        [Test]
        public void WaitForCtsChangedEvent()
        {
            using (ManualResetEvent cts = new ManualResetEvent(false))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.Open(); dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;

                Thread.Sleep(300);   // Wait for events to be cleared after opening.

                dst.PinChanged += (s, a) => {
                    if (a.EventType.HasFlag(SerialPinChange.CtsChanged)) cts.Set();
                };

                // By default, the RTS is enabled. So we toggle it to off.
                src.RtsEnable = false;

                Assert.That(cts.WaitOne(500), Is.True);
            }
        }

        [Test]
        public void WaitForDsrChangedEvent()
        {
            using (ManualResetEvent dsr = new ManualResetEvent(false))
            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open(); src.WriteTimeout = TimeOut; src.ReadTimeout = TimeOut;
                dst.Open(); dst.WriteTimeout = TimeOut; dst.ReadTimeout = TimeOut;

                Thread.Sleep(300);   // Wait for events to be cleared after opening.

                dst.PinChanged += (s, a) => {
                    if (a.EventType.HasFlag(SerialPinChange.DsrChanged)) dsr.Set();
                };

                // By default, the DTR is enabled. So we toggle it to off.
                src.DtrEnable = false;

                Assert.That(dsr.WaitOne(500), Is.True);
            }
        }

        [Test]
        public void WriteWhenClosedException()
        {
            byte[] buffer = new byte[256];

            using (SerialPortStream src = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                Assert.That(() => {
                    src.Write(buffer, 0, buffer.Length);
                }, Throws.TypeOf<InvalidOperationException>());
            }
        }

        [Test]
        [Timeout(60000)]
        public void ReadDataEvent()
        {
            const int blockSize = 8192;
            const int testTotalBytes = 344 * 1024;  // 344kB of data = 30s (344*1024*10/115200 =~ 30.6s)

            int startTick = Environment.TickCount;

            using (ManualResetEvent finished = new ManualResetEvent(false))
            using (SerialPortStream serialSource = new SerialPortStream(SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream serialDest = new SerialPortStream(DestPort, 115200, 8, Parity.None, StopBits.One)) {
                serialSource.ReadBufferSize = blockSize;
                serialSource.WriteBufferSize = blockSize;
                serialDest.ReadBufferSize = blockSize;
                serialDest.WriteBufferSize = blockSize;

                byte[] readBuffer = new byte[blockSize];
                byte[] writeBuffer = new byte[blockSize];

                int totalBytes = 0;
                serialDest.DataReceived += (s, e) => {
                    int bytes = serialDest.Read(readBuffer, 0, readBuffer.Length);
                    totalBytes += bytes;
                    Console.WriteLine("===> {0}: EventType: {1}, bytes read = {2}, total read = {3}",
                        Environment.TickCount - startTick, e.EventType, bytes, totalBytes);
                    if (totalBytes >= testTotalBytes) finished.Set();
                };
                serialDest.ErrorReceived += (s, e) => {
                    Console.WriteLine("===> {0}: EventType: {1}", Environment.TickCount - startTick, e.EventType);
                };

                serialSource.Open();
                serialDest.Open();

                int writeBytes = 0;
                while (writeBytes < testTotalBytes) {
                    serialSource.Write(writeBuffer, 0, writeBuffer.Length);
                    writeBytes += writeBuffer.Length;
                    Console.WriteLine("===> {0}: Write {1} bytes; Written {2}; Total {3}",
                        Environment.TickCount - startTick, writeBuffer.Length, writeBytes, testTotalBytes);
                }
                serialSource.Flush();

                // Should only need enough time to wait for the last 8192 bytes to be written.
                Assert.That(finished.WaitOne(10000), Is.True);
            }
        }

        [Test]
        public void SendReceiveBoundaries()
        {
            using (var sp = new SerialPortStream(SourcePort, 56700))
            using (var spReceive = new SerialPortStream(DestPort, 56700)) {
                var bufferSize = 250;

                // get data which is a bit larger than twice the size of the buffer (0x20000*2)
                var data = Enumerable.Range(0, (sp.WriteBufferSize * 2) + 0x100).Select(e => (byte)(e % 0xFF)).ToArray();
                var size = data.Length; // 0x40100
                var position = 0;
                sp.Open();
                spReceive.Open();

                List<int> positions = new List<int>();
                while (size - position > bufferSize) {
                    positions.Add(position);
                    if (position >= sp.WriteBufferSize - bufferSize) {
                        Console.WriteLine("data[{0:x}] = {1:x}", position, data[position]);
                    }
                    sp.Write(data, position, bufferSize);
                    position += bufferSize;
                }
                positions.Add(position);
                if (data.Length - position > 0) {
                    sp.Write(data, position, data.Length - position);
                }

                sp.Flush();

                byte[] receiveData = ReceiveData(spReceive, size);
                Assert.That(Compare(data, receiveData), Is.True);

                sp.Close();
                spReceive.Close();
            }
        }

        private byte[] ReceiveData(SerialPortStream sp, int size)
        {
            var buffer = new byte[size];
            var dataReceived = 0;
            while (dataReceived < size) {
                dataReceived += sp.Read(buffer, dataReceived, size - dataReceived);
            }
            return buffer;
        }

        private struct Difference
        {
            public Difference(int position, byte sent, byte received) : this()
            {
                Position = $"0x{position:X5}";
                Sent = $"0x{sent:X2}";
                Received = $"0x{received:X2}";
            }

            public string Position { get; set; }

            public string Sent { get; set; }

            public string Received { get; set; }
        }

        private bool Compare(byte[] data, byte[] receivedData)
        {
            bool equal = data.SequenceEqual(receivedData);
            Console.WriteLine("Are data the same: {0}", equal);

            List<Difference> differences = new List<Difference>();
            for (int i = 0; i < data.Length; i++) {
                var sent = data[i];
                var received = receivedData[i];
                if (sent != received) {
                    differences.Add(new Difference(i, sent, received));
                }
            }

            foreach (Difference item in differences) {
                Console.WriteLine("Pos: {0}, Tx: {1}, Rx: {2}", item.Position, item.Received, item.Sent);
            }
            return equal;
        }
    }
}
