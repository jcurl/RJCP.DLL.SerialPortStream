// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports
{
    using System;
    using NUnit.Framework;
    using Serial;

#if NET45_OR_GREATER || NETCOREAPP
    using System.Threading.Tasks;
#endif

    [TestFixture]
    [Timeout(10000)]
    public class ReadWithThrowOnError
    {
        [Test]
        public void ThrowOnReadErrorDefaultBehaviour()
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {

                Assert.That(stream.ThrowOnReadError, Is.False);
            }
        }

        [TestCase(true, TestName = "CanReadClosedThrowOnError")]
        [TestCase(false, TestName = "CanReadClosed")]
        public void CanReadClosed(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;

                Assert.That(stream.CanRead, Is.Not.EqualTo(throwOnError));
            }
        }

        [TestCase(true, TestName = "ReadBytesWhenClosedThrowOnError")]
        [TestCase(false, TestName = "ReadBytesWhenClosed")]
        public void ReadBytesWhenClosedThrowOnError(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;

                byte[] buffer = new byte[256];
                if (throwOnError) {
                    Assert.That(() => {
                        stream.Read(buffer, 0, buffer.Length);
                    }, Throws.TypeOf<InvalidOperationException>());
                } else {
                    Assert.That(stream.Read(buffer, 0, buffer.Length), Is.EqualTo(0));
                }
            }
        }

        [TestCase(true, TestName = "ReadCharsWhenClosedThrowOnError")]
        [TestCase(false, TestName = "ReadCharsWhenClosed")]
        public void ReadCharsWhenClosedThrowOnError(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;

                char[] buffer = new char[256];
                if (throwOnError) {
                    Assert.That(() => {
                        stream.Read(buffer, 0, buffer.Length);
                    }, Throws.TypeOf<InvalidOperationException>());
                } else {
                    Assert.That(stream.Read(buffer, 0, buffer.Length), Is.EqualTo(0));
                }
            }
        }

        [TestCase(true, TestName = "ReadByteWhenClosedThrowOnError")]
        [TestCase(false, TestName = "ReadByteWhenClosed")]
        public void ReadByteWhenClosedThrowOnError(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;

                if (throwOnError) {
                    Assert.That(() => {
                        _ = stream.ReadByte();
                    }, Throws.TypeOf<InvalidOperationException>());
                } else {
                    Assert.That(stream.ReadByte(), Is.EqualTo(-1));
                }
            }
        }

        [TestCase(true, TestName = "ReadCharWhenClosedThrowOnError")]
        [TestCase(false, TestName = "ReadCharWhenClosed")]
        public void ReadCharWhenClosedThrowOnError(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;

                if (throwOnError) {
                    Assert.That(() => {
                        _ = stream.ReadChar();
                    }, Throws.TypeOf<InvalidOperationException>());
                } else {
                    Assert.That(stream.ReadChar(), Is.EqualTo(-1));
                }
            }
        }

        [TestCase(true, TestName = "ReadLineWhenClosedThrowOnError")]
        [TestCase(false, TestName = "ReadLineWhenClosed")]
        public void ReadLineWhenClosedThrowOnError(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;

                if (throwOnError) {
                    Assert.That(() => {
                        _ = stream.ReadLine();
                    }, Throws.TypeOf<InvalidOperationException>());
                } else {
                    Assert.That(stream.ReadLine(), Is.Null);
                }
            }
        }

        [TestCase(true, TestName = "ReadExistingWhenClosedThrowOnError")]
        [TestCase(false, TestName = "ReadExistingWhenClosed")]
        public void ReadExistingWhenClosedThrowOnError(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;

                if (throwOnError) {
                    Assert.That(() => {
                        _ = stream.ReadExisting();
                    }, Throws.TypeOf<InvalidOperationException>());
                } else {
                    Assert.That(stream.ReadExisting(), Is.Null);
                }
            }
        }

        [TestCase(true, TestName = "ReadToWhenClosedThrowOnError")]
        [TestCase(false, TestName = "ReadToWhenClosed")]
        public void ReadToWhenClosedThrowOnError(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;

                if (throwOnError) {
                    Assert.That(() => {
                        _ = stream.ReadTo("eof");
                    }, Throws.TypeOf<InvalidOperationException>());
                } else {
                    Assert.That(stream.ReadTo("eof"), Is.Null);
                }
            }
        }

#if NET45_OR_GREATER || NETCOREAPP
        [TestCase(true, TestName = "ReadAsyncWhenClosedThrowOnError")]
        [TestCase(false, TestName = "ReadAsyncWhenClosed")]
        public async Task ReadAsyncWhenClosedThrowOnError(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;

                byte[] buffer = new byte[256];
                if (throwOnError) {
                    Assert.That(async () => {
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                    }, Throws.TypeOf<InvalidOperationException>());
                } else {
                    Assert.That(await stream.ReadAsync(buffer, 0, buffer.Length), Is.EqualTo(0));
                }
            }
        }
#endif

#if NETFRAMEWORK
        [TestCase(true, TestName = "BeginReadWhenClosedThrowOnError")]
        [TestCase(false, TestName = "BeginReadWhenClosed")]
        public void BeginReadWhenClosedThrowOnError(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;

                byte[] buffer = new byte[256];
                if (throwOnError) {
                    Assert.That(() => {
                        _ = stream.BeginRead(buffer, 0, buffer.Length, null, null);
                    }, Throws.TypeOf<InvalidOperationException>());
                } else {
                    IAsyncResult ar = stream.BeginRead(buffer, 0, buffer.Length, null, null);
                    Assert.That(stream.EndRead(ar), Is.EqualTo(0));
                }
            }
        }
#endif

        [TestCase(true, TestName = "ReadBytesTimeoutThrowOnError")]
        [TestCase(false, TestName = "ReadBytesTimeout")]
        public void ReadBytesTimeout(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;
                stream.ReadTimeout = 50;
                stream.PortName = "COM";
                stream.Open();

                byte[] buffer = new byte[256];
                if (throwOnError) {
                    Assert.That(() => {
                        _ = stream.Read(buffer, 0, buffer.Length);
                    }, Throws.TypeOf<TimeoutException>());
                } else {
                    Assert.That(stream.Read(buffer, 0, buffer.Length), Is.EqualTo(0));
                }
            }
        }

        [TestCase(true, TestName = "ReadCharsTimeoutThrowOnError")]
        [TestCase(false, TestName = "ReadCharsTimeout")]
        public void ReadCharsTimeout(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;
                stream.ReadTimeout = 50;
                stream.PortName = "COM";
                stream.Open();

                char[] buffer = new char[256];
                if (throwOnError) {
                    Assert.That(() => {
                        _ = stream.Read(buffer, 0, buffer.Length);
                    }, Throws.TypeOf<TimeoutException>());
                } else {
                    Assert.That(stream.Read(buffer, 0, buffer.Length), Is.EqualTo(0));
                }
            }
        }

        [TestCase(true, TestName = "ReadByteTimeoutThrowOnError")]
        [TestCase(false, TestName = "ReadByteTimeout")]
        public void ReadByteTimeout(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;
                stream.ReadTimeout = 50;
                stream.PortName = "COM";
                stream.Open();

                if (throwOnError) {
                    Assert.That(() => {
                        _ = stream.ReadByte();
                    }, Throws.TypeOf<TimeoutException>());
                } else {
                    Assert.That(stream.ReadByte(), Is.EqualTo(-1));
                }
            }
        }

        [TestCase(true, TestName = "ReadCharTimeoutThrowOnError")]
        [TestCase(false, TestName = "ReadCharTimeout")]
        public void ReadCharTimeout(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;
                stream.ReadTimeout = 50;
                stream.PortName = "COM";
                stream.Open();

                if (throwOnError) {
                    Assert.That(() => {
                        _ = stream.ReadChar();
                    }, Throws.TypeOf<TimeoutException>());
                } else {
                    Assert.That(stream.ReadChar(), Is.EqualTo(-1));
                }
            }
        }

        [TestCase(true, TestName = "ReadLineTimeoutThrowOnError")]
        [TestCase(false, TestName = "ReadLineTimeout")]
        public void ReadLineTimeout(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;
                stream.ReadTimeout = 50;
                stream.PortName = "COM";
                stream.Open();

                // Always throws an exception (as in v2.x)
                Assert.That(() => {
                    _ = stream.ReadLine();
                }, Throws.TypeOf<TimeoutException>());
            }
        }

        [TestCase(true, TestName = "ReadToTimeoutThrowOnError")]
        [TestCase(false, TestName = "ReadToTimeout")]
        public void ReadToTimeout(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;
                stream.ReadTimeout = 50;
                stream.PortName = "COM";
                stream.Open();

                // Always throws an exception (as in v2.x)
                Assert.That(() => {
                    _ = stream.ReadTo("eof");
                }, Throws.TypeOf<TimeoutException>());
            }
        }

        [TestCase(true, TestName = "ReadExistingTimeoutThrowOnError")]
        [TestCase(false, TestName = "ReadExistingTimeout")]
        public void ReadExistingTimeout(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;
                stream.ReadTimeout = 50;
                stream.PortName = "COM";
                stream.Open();

                Assert.That(stream.ReadExisting(), Is.Empty);
            }
        }

#if NET45_OR_GREATER || NETCOREAPP
        [TestCase(true, TestName = "ReadAsyncTimeoutThrowOnError")]
        [TestCase(false, TestName = "ReadAsyncTimeout")]
        public async Task ReadAsyncTimeout(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;
                stream.ReadTimeout = 50;
                stream.PortName = "COM";
                stream.Open();

                byte[] buffer = new byte[256];
                if (throwOnError) {
                    Assert.That(async () => {
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                    }, Throws.TypeOf<TimeoutException>());
                } else {
                    Assert.That(await stream.ReadAsync(buffer, 0, buffer.Length), Is.EqualTo(0));
                }
            }
        }
#endif

#if NETFRAMEWORK
        [TestCase(true, TestName = "BeginReadTimeoutThrowOnError")]
        [TestCase(false, TestName = "BeginReadTimeout")]
        public void BeginReadTimeout(bool throwOnError)
        {
            using (VirtualNativeSerial serial = new VirtualNativeSerial())
            using (SerialPortStream stream = new SerialPortStream(serial)) {
                stream.ThrowOnReadError = throwOnError;
                stream.ReadTimeout = 50;
                stream.PortName = "COM";
                stream.Open();

                byte[] buffer = new byte[256];
                if (throwOnError) {
                    IAsyncResult ar = stream.BeginRead(buffer, 0, buffer.Length, null, null);
                    Assert.That(() => {
                        _ = stream.EndRead(ar);
                    }, Throws.TypeOf<TimeoutException>());
                } else {
                    IAsyncResult ar = stream.BeginRead(buffer, 0, buffer.Length, null, null);
                    Assert.That(stream.EndRead(ar), Is.EqualTo(0));
                }
            }
        }
#endif
    }
}