# Test Projects

This document describes the different test projects, what they're for and how to
use them.

## Unit Testing

### SerialPortStreamTest

These contain simple unit tests for testing functionality of the
`SerialPortStream` without requiring physical hardware for the test cases to
pass.

## Integration Testing

### BufferBytesTest

This is a command line tool. It is to send data over the serial port and monitor
the amount of Bytes to Write.

Use an integrated serial port, or attach a USB serial device.

It should be run from the command line. The following example sends 160kB over
COM1 at a baud rate of 115200.

```cmd
BufferBytesTest --port COM1 --baud 115200 --length 163840
```

It will print out logging when the bytes are sent, and the number of bytes still
remaining to be sent.

Expected behaviour:

* The number of bytes in the buffer are printed out every time there is a
  change. On Windows, the number of bytes is a combination of the amount of data
  in the buffer and the amount of data the serial port driver indicates there
  is.
* After the test case indicates that it is flushing, then there is no more data
  to buffer in the `SerialPortStream`, and the `BytesToWrite` should
  monotonically decrease until it reaches zero.
* The amount of data to write should never exceed the initial length, or the
  internal buffer size of 131072 bytes (128kB). If it does, it is an error
  within the `SerialPortStream`.

Observed Behaviour

* On a CP2101 device, the `BytesToWrite` might increase (it is not monotonic
  decreasing). This is because the driver shows how much is buffered, but it
  doesn't send all the data before it returns.
  * Root cause #1: Sometimes the driver reports increase the number of bytes to
    write.
  * Root cause #2: The driver's write operation returns before the data in the
    buffer is completely sent. The SerialPortStream returns the maximum of
    either the bytes buffered, or the bytes the driver reports is still to be
    sent (but it doesn't sum both which could result in the value being too
    high)
* On a PL2303 RA device, the number of bytes to write from the driver is
  generally zero, or the result of the last write operation. Assuming that the
  PL2303 RA itself has buffering in the driver, then the BytesToWrite would be
  less than the actual physical number of bytes that should still be sent.

### SerialPortStreamManualTest

Test cases that should be run manually, either because they take a long time to
run, or they need manual interaction during the test. The tests marked as
explicit cannot run in a CI as they need manual intervention.

Two serial devices are required, that are connected to each other using a
NUL-Modem configuration.

Information about each test case is documented in the test class itself.

* `DisconnectDeviceTest` - Test cases require a USB Serial as the SourcePort,
  that can be removed during each individual test.
  * These are manual tests.
  * The `App.Config` file will likely need to be modified to provide the serial
    port name.
  * Each test should be run individually.

### SerialPortStreamNativeTest

These are NUnit Tests that require a NULL-Modem configuration to work. The
require access to a serial port, and so are not allowed to run at the same time
as other tests that execute the serial port. That means, one should not create a
new project that also requires the serial port as .NET SDK testing (`dotnet
test`) will run different projects in parallel.

* `CloseWhileBlockedTest` - All test cases should pass. They take about 2s each
  on Windows, and fail on Linux. These test cases are marked Explicit, as on
  Linux the test duration takes more than a minute.
* `ParityTest` - Check the actual parity is used correctly by sending 7 bits
  with parity and receiving 8 bits without parity. On software solutions, these
  test cases fail, but do pass with real NULL-modem cables.
* `ReadCharTest` - Tests the character based functionality with two real serial
  ports.
* `ReceiveTransmitTest` - Send and receive byte data between serial ports.
* `SerialPortStreamLoggerTest` - For .NET Core, tests injection of an `ILogger`.
* `SerialPortStreamTest` - Tests properties of the serial port stream against a
  real serial port.
