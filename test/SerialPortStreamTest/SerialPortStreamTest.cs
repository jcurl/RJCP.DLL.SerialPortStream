// $URL$
// $Id$

// Copyright © Jason Curl 2012-2014
// See http://serialportstream.codeplex.com for license details (MS-PL License)

using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RJCP.IO.Ports.SerialPortStreamTest
{
    /// <summary>
    /// Test sending and receiving data via two serial ports
    /// </summary>
    /// <remarks>
    /// You will need to have two serial ports connected to each other on the same computer
    /// using a NULL modem cable. Alternatively, you can use a software emulated serial
    /// port, such as com0com for tests.
    /// <para>You need to update the variables c_SourcePort and c_DestPort to be the names
    /// of the two serial ports.</para>
    /// </remarks>
    [TestClass]
    public class SerialPortStreamTest
    {
        private const string c_SourcePort = "CNCA0";
        private const string c_DestPort = "CNCB0";

        private const int c_Timeout = 300;

        /// <summary>
        /// Test the basic features of a serial port
        /// </summary>
        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_Basic()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 100;
                src.ReadTimeout = 100;

                Assert.IsTrue(src.CanRead);
                Assert.IsFalse(src.CanWrite);
                Assert.AreEqual(c_SourcePort, src.PortName);
                Assert.AreEqual(0, src.BytesToRead);
                Assert.AreEqual(0, src.BytesToWrite);

                src.Open();
                Assert.IsTrue(src.CanRead);
                Assert.IsTrue(src.CanWrite);
                Assert.IsTrue(src.IsOpen);

                src.Close();
                Assert.IsTrue(src.CanRead);
                Assert.IsFalse(src.CanWrite);
                Assert.IsFalse(src.IsOpen);
            }
        }

        /// <summary>
        /// Check that Open() and Close() can be called multiple times
        /// </summary>
        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_OpenClose()
        {
            SerialPortStream src;
            using (src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = 100;
                src.ReadTimeout = 100;

                Assert.IsTrue(src.CanRead);
                Assert.IsFalse(src.CanWrite);
                Assert.IsFalse(src.IsOpen);
                Assert.IsFalse(src.IsDisposed);

                src.Open();
                Assert.IsTrue(src.CanRead);
                Assert.IsTrue(src.CanWrite);
                Assert.IsTrue(src.IsOpen);
                Assert.IsFalse(src.IsDisposed);

                src.Close();
                Assert.IsTrue(src.CanRead);
                Assert.IsFalse(src.CanWrite);
                Assert.IsFalse(src.IsOpen);
                Assert.IsFalse(src.IsDisposed);

                src.Open();
                Assert.IsTrue(src.CanRead);
                Assert.IsTrue(src.CanWrite);
                Assert.IsTrue(src.IsOpen);
                Assert.IsFalse(src.IsDisposed);

                src.Close();
                Assert.IsTrue(src.CanRead);
                Assert.IsFalse(src.CanWrite);
                Assert.IsFalse(src.IsOpen);
                Assert.IsFalse(src.IsDisposed);
            }
            Assert.IsTrue(src.IsDisposed);
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_OpenInUse()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.Open();

                using (SerialPortStream s2 = new SerialPortStream(c_SourcePort, 9600, 8, Parity.None, StopBits.One)) {
                    bool err = false;
                    try {
                        s2.Open();
                    } catch {
                        err = true;
                    }
                    Assert.IsTrue(err, "Expected exception opening the port twice");
                }
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_GetPortSettings()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                src.GetPortSettings();

                SerialPortStream s2 = new SerialPortStream();
                bool err = false;
                try {
                    s2.GetPortSettings();
                } catch {
                    err = true;
                }
                Assert.IsTrue(err, "No exception raised when port not defined");
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_NewLine()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                bool exception = false;
                try {
                    src.NewLine = "";
                } catch (System.ArgumentException) {
                    exception = true;
                }
                Assert.IsTrue(exception, "Expected exception when setting newline to empty string");

                exception = false;
                try {
                    src.NewLine = null;
                } catch (System.ArgumentNullException) {
                    exception = true;
                }
                Assert.IsTrue(exception, "Expected exception when setting newline to empty string");
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_SendReceive()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_Timeout; src.ReadTimeout = c_Timeout;
                dst.WriteTimeout = c_Timeout; dst.ReadTimeout = c_Timeout;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                // Send Maximum data in one go
                byte[] sendbuf = new byte[src.WriteBufferSize];
#if true
                Random r = new Random();
                r.NextBytes(sendbuf);
#else
            for (int i = 0; i < sendbuf.Length; i++) {
                sendbuf[i] = (byte)((i % 77) + 1);
            }
#endif
                src.Write(sendbuf, 0, sendbuf.Length);

                // Receive sent data
                int rcv = 0;
                int c = 0;
                byte[] rcvbuf = new byte[sendbuf.Length + 10];
                while (rcv < rcvbuf.Length) {
                    Trace.WriteLine("Begin Receive: Offset=" + rcv.ToString() + "; Count=" + (rcvbuf.Length - rcv).ToString());
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
                    Trace.WriteLine("Read length not the same as the amount of data sent (got " + rcv.ToString() + " bytes)");
                    dump = true;
                }
                for (int i = 0; i < sendbuf.Length; i++) {
                    if (sendbuf[i] != rcvbuf[i]) {
                        Trace.WriteLine("Comparison failure at " + i.ToString());
                        dump = true;
                        break;
                    }
                }

                if (dump) {
                    Trace.WriteLine("Send Buffer DUMP");
                    for (int i = 0; i < sendbuf.Length; i++) {
                        Trace.WriteLine(sendbuf[i].ToString("X2"));
                    }

                    Trace.WriteLine("Receive Buffer DUMP");
                    for (int i = 0; i < rcv; i++) {
                        Trace.WriteLine(rcvbuf[i].ToString("X2"));
                    }
                }
                src.Close();
                dst.Close();
                Assert.IsFalse(dump, "Error in transfer");
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        [Timeout(2000)]
        public void SerialPortStream_SendAndFlush1()
        {
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                // Required for com0com to send the data (the remote side must receive it)
                dst.Open();

                src.Open();
                src.WriteLine("Connected");
                src.Flush();
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        [Timeout(2000)]
        public void SerialPortStream_SendAndFlush2()
        {
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One)) {
                // Required for com0com to send the data (the remote side must receive it)
                dst.Open();

                Trace.WriteLine("1. Open()");
                src.Open();
                Trace.WriteLine("2. WriteLine()");
                src.WriteLine("Connected");
                Trace.WriteLine("3. Sleep()");
                System.Threading.Thread.Sleep(100);
                Trace.WriteLine("4. WriteLine()");
                src.WriteLine("Disconnected");
                Trace.WriteLine("5. Flush()");
                src.Flush();
                Trace.WriteLine("6. Dispose()");
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_ListPorts()
        {
            bool result = true;

            Dictionary<string, bool> ports1 = new Dictionary<string, bool>();
            Dictionary<string, bool> ports2 = new Dictionary<string, bool>();

            foreach (PortDescription desc in SerialPortStream.GetPortDescriptions()) {
                Trace.WriteLine("GetPortDescriptions: " + desc.Port + "; Description: " + desc.Description);
                ports1.Add(desc.Port, false);
                ports2.Add(desc.Port, false);
            }

            foreach (string c in SerialPortStream.GetPortNames()) {
                Trace.WriteLine("GetPortNames: " + c);
                if (ports1.ContainsKey(c)) {
                    ports1[c] = true;
                } else {
                    Trace.WriteLine("GetPortNames() shows " + c + ", but not GetPortDescriptions()");
                    result = false;
                }
            }
            foreach (string c in ports1.Keys) {
                if (ports1[c] == false) {
                    Trace.WriteLine("GetPortDescriptions() shows " + c + ", but not GetPortnames()");
                    result = false;
                }
            }

            foreach (string c in System.IO.Ports.SerialPort.GetPortNames()) {
                Trace.WriteLine("SerialPort.GetPortNames: " + c);
                if (ports2.ContainsKey(c)) {
                    ports2[c] = true;
                } else {
                    Trace.WriteLine("System.IO.Ports.SerialPort.GetPortNames() shows " + c + ", but not GetPortDescriptions()");
                    result = false;
                }
            }
            foreach (string c in ports1.Keys) {
                if (ports2[c] == false) {
                    Trace.WriteLine("GetPortDescriptions() shows " + c + ", but not System.IO.Ports.SerialPort.GetPortNames()");
                    result = false;
                }
            }

            Assert.IsTrue(result);
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_WriteReadLine()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_Timeout; src.ReadTimeout = c_Timeout;
                dst.WriteTimeout = c_Timeout; dst.ReadTimeout = c_Timeout;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                string s;
                src.WriteLine("TestString");
                s = dst.ReadLine();
                Assert.AreEqual("TestString", s);
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_WriteReadLine_Timeout1()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_Timeout; src.ReadTimeout = c_Timeout;
                dst.WriteTimeout = c_Timeout; dst.ReadTimeout = c_Timeout;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                bool err = false;

                string s;
                src.Write("TestString");
                try {
                    s = dst.ReadLine();
                } catch (System.Exception e) {
                    if (e is TimeoutException) err = true;
                }
                Assert.IsTrue(err, "No timeout exception occurred");

                src.WriteLine("");
                s = dst.ReadLine();
                Assert.AreEqual("TestString", s);
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_WriteReadLine_Timeout2()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_Timeout; src.ReadTimeout = c_Timeout;
                dst.WriteTimeout = c_Timeout; dst.ReadTimeout = c_Timeout;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                bool err = false;

                string s;
                src.Write("Test");
                try {
                    s = dst.ReadLine();
                } catch (System.Exception e) {
                    if (e is TimeoutException) err = true;
                }
                Assert.IsTrue(err, "No timeout exception occurred");

                src.WriteLine("String");
                s = dst.ReadLine();
                Assert.AreEqual("TestString", s);
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_WriteReadLine_Multilines()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_Timeout; src.ReadTimeout = c_Timeout;
                dst.WriteTimeout = c_Timeout; dst.ReadTimeout = c_Timeout;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                bool err = false;

                string s;
                src.Write("Line1\nLine2\n");
                s = dst.ReadLine();
                Assert.AreEqual("Line1", s);
                s = dst.ReadLine();
                Assert.AreEqual("Line2", s);

                try {
                    s = dst.ReadLine();
                } catch (System.Exception e) {
                    if (e is TimeoutException) err = true;
                }
                Assert.IsTrue(err, "No timeout exception occurred");
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_WriteReadLine_CharForChar()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_Timeout; src.ReadTimeout = c_Timeout;
                dst.WriteTimeout = c_Timeout; dst.ReadTimeout = c_Timeout;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                bool err = false;
                string s = null;

                const string send = "A Brief History Of Time\n";

                for (int i = 0; i < send.Length; i++) {
                    src.Write(send[i].ToString());
                    try {
                        s = dst.ReadLine();
                    } catch (System.Exception e) {
                        if (e is TimeoutException) err = true;
                    }
                    if (i < send.Length - 1) {
                        Assert.IsTrue(err, "No timeout exception occurred when waiting for " + send[i].ToString() + " (position " + i.ToString() + ")");
                    }
                }
                Assert.AreEqual("A Brief History Of Time", s);
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_WriteReadLine_MbcsByteForByte()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_Timeout; src.ReadTimeout = c_Timeout;
                dst.WriteTimeout = c_Timeout; dst.ReadTimeout = c_Timeout;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                bool err = false;
                string s = null;

                byte[] buf = new byte[] {
                0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4A,
                0xE2, 0x82, 0xAC, 0x0A
            };

                for (int i = 0; i < buf.Length; i++) {
                    src.Write(buf, i, 1);
                    try {
                        s = dst.ReadLine();
                    } catch (System.Exception e) {
                        if (e is TimeoutException) err = true;
                    }
                    if (i < buf.Length - 1) {
                        Assert.IsTrue(err, "No timeout exception occurred when waiting for " + buf[i].ToString("X2") + " (position " + i.ToString() + ")");
                    }
                }
                Assert.AreEqual("ABCDEFGHIJ€", s);
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_ReadTo_Cached()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_Timeout; src.ReadTimeout = c_Timeout;
                dst.WriteTimeout = c_Timeout; dst.ReadTimeout = c_Timeout;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                bool err = false;
                string s;

                src.Write("foobar");
                try {
                    s = dst.ReadTo("baz");
                } catch (System.Exception e) {
                    if (e is TimeoutException) err = true;
                }
                Assert.IsTrue(err, "No timeout exception occurred when reading 'baz'");

                s = dst.ReadTo("foo");
                Assert.AreEqual("", s);

                s = dst.ReadTo("bar");
                Assert.AreEqual("", s);

                try {
                    s = dst.ReadTo("baz");
                } catch (System.Exception e) {
                    if (e is TimeoutException) err = true;
                }
                Assert.IsTrue(err, "No timeout exception occurred when reading 'baz' when empty");
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_ReadTo_Normal()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_Timeout; src.ReadTimeout = c_Timeout;
                dst.WriteTimeout = c_Timeout; dst.ReadTimeout = c_Timeout;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                string s;

                src.Write("superfoobar");
                s = dst.ReadTo("foo");
                Assert.AreEqual("super", s);

                s = dst.ReadExisting();
                Assert.AreEqual("bar", s);
            }
        }

        [TestMethod]
        [TestCategory("SerialPortStream")]
        public void SerialPortStream_ReadTo_Overflow()
        {
            using (SerialPortStream src = new SerialPortStream(c_SourcePort, 115200, 8, Parity.None, StopBits.One))
            using (SerialPortStream dst = new SerialPortStream(c_DestPort, 115200, 8, Parity.None, StopBits.One)) {
                src.WriteTimeout = c_Timeout; src.ReadTimeout = c_Timeout;
                dst.WriteTimeout = c_Timeout; dst.ReadTimeout = c_Timeout;
                src.Open(); Assert.IsTrue(src.IsOpen);
                dst.Open(); Assert.IsTrue(dst.IsOpen);

                // Send 2048 ASCII characters
                Random r = new Random();
                byte[] sdata = new byte[2048];
                for (int i = 0; i < sdata.Length; i++) {
                    sdata[i] = (byte)r.Next(65, 65 + 26);
                }

                // Wait for the data to be received
                src.Write(sdata, 0, sdata.Length);
                src.Write("EOF");
                while (dst.BytesToRead < sdata.Length) {
                    System.Threading.Thread.Sleep(100);
                }

                string result = dst.ReadTo("EOF");
                Assert.AreEqual(0, dst.BytesToRead);
                Assert.AreEqual(1024 - 3, result.Length);
                int offset = sdata.Length - result.Length;
                for (int i = 0; i < result.Length; i++) {
                    Assert.AreEqual((int)sdata[offset + i], (int)result[i]);
                }
            }
        }
    }
}
