# Serial Port Stream <!-- omit in toc -->

SerialPortStream is an independent implementation of
`System.IO.Ports.SerialPort` and `SerialStream` for better reliability and
maintainability, and now for portability to Mono on Linux systems.

The `SerialPortStream` is a ground up implementation of a `Stream` that buffers
data to and from a serial port. It uses low level Win32API (on Windows) for
managing events and asynchronous I/O, using a programming model as in the MSDN
[PipeServer](http://msdn.microsoft.com/en-us/library/windows/desktop/aa365603.aspx)
example.

On Linux, it uses a support library to interface with Posix OS calls for an
event loop.

These notes are for version 3.x, which is a refactoring of v2.x for better
maintainability. See the end of these notes for differences.

- [1. Why another Serial Port implementation](#1-why-another-serial-port-implementation)
- [2. Goals](#2-goals)
  - [2.1. Issues with MS Serial Port](#21-issues-with-ms-serial-port)
  - [2.2. Differences to the MS Serial Port](#22-differences-to-the-ms-serial-port)
- [3. System Requirements](#3-system-requirements)
  - [3.1. Testing](#31-testing)
  - [3.2. Compatibility](#32-compatibility)
    - [3.2.1. .NET Frameworks (Windows)](#321-net-frameworks-windows)
    - [3.2.2. Mono Framework (Linux Only)](#322-mono-framework-linux-only)
- [4. Installation](#4-installation)
  - [4.1. Windows](#41-windows)
  - [4.2. Linux](#42-linux)
- [5. Extra Features](#5-extra-features)
  - [5.1. Reading and Writing - Buffering](#51-reading-and-writing---buffering)
- [6. Developer Notes](#6-developer-notes)
  - [6.1. Logging](#61-logging)
  - [6.2. The LogSource abstraction](#62-the-logsource-abstraction)
  - [6.3. .NET Framework (.NET 4.0 to .NET 4.8)](#63-net-framework-net-40-to-net-48)
  - [6.4. .NET Core](#64-net-core)
    - [6.4.1. Dependency Injection](#641-dependency-injection)
    - [6.4.2. Singleton via LogSource](#642-singleton-via-logsource)
- [7. Known Issues](#7-known-issues)
  - [7.1. General Issues](#71-general-issues)
    - [7.1.1. ReadTo](#711-readto)
  - [7.2. Windows](#72-windows)
    - [7.2.1. Driver Specific Issues on Windows](#721-driver-specific-issues-on-windows)
      - [7.2.1.1. Flow Control](#7211-flow-control)
      - [7.2.1.2. BytesToWrite](#7212-bytestowrite)
        - [7.2.1.2.1. CP2101 Driver](#72121-cp2101-driver)
        - [7.2.1.2.2. PL2303 RA](#72122-pl2303-ra)
  - [7.3. Linux](#73-linux)
    - [7.3.1. Mono on non-Windows Platforms](#731-mono-on-non-windows-platforms)
    - [7.3.2. Driver Specific Issues on Linux](#732-driver-specific-issues-on-linux)
      - [7.3.2.1. Parity Errors](#7321-parity-errors)
      - [7.3.2.2. Garbage Data on Open](#7322-garbage-data-on-open)
      - [7.3.2.3. Monitoring Pins and Timing Resolution](#7323-monitoring-pins-and-timing-resolution)
      - [7.3.2.4. Close Times with Flow Control](#7324-close-times-with-flow-control)
      - [7.3.2.5. Opening Ports (and some unit test case failures)](#7325-opening-ports-and-some-unit-test-case-failures)
  - [7.4. Guidelines on Serial Protocols](#74-guidelines-on-serial-protocols)

## 1. Why another Serial Port implementation

Microsoft and Mono already provided a reasonable implementation for accessing
the serial port. Today the main goal is to provide a buffered solution that can
be used on various operating systems, the the ability to also abstract hardware.
Along the way, various issues with the original implementation in .NET Framework
are resolved in this library (see the next section).

## 2. Goals

This project tries to achieve the following:

- An implementation similar to the MS implementation of `SerialPort`. It's not
  meant to be 100% compatible, but instead provide similar functionality
- Abstract the driver implementation and provide for a more reliable transport,
  by making writing serial data completely buffered. With the MS implementation,
  one can write data, but subsequently needs to check if all data is written or
  not. If it isn't written, then it needs to be retried. The `SerialPortStream`
  makes this easier.
- Provide for reliable and consistent behaviour. See the next section.

### 2.1. Issues with MS Serial Port

The `SerialPortStream` tries to solve the following issues observed:

- Zach Saw describes issues regarding behaviour of the `fAbortError` flag in the
  Serial `DCB`. The `SerialPortStream` defines this flag.
- Closing a serial port, then reopening it generally causes problems. The
  `SerialPortStream` shouldn't have this issue. Note, there are some cases
  observed where the Operating System hangs, and this can't be avoided.
- The `ReadTo()` implementation can subtly change the byte stream buffer, when
  one switches from characters to bytes. This problem occurs because the MS
  implementation actually converts the characters back to bytes into its buffer.
  So if you have UTF8, decoded some invalid characters and then have a timeout,
  this results in the invalid characters being converted back to bytes,
  resulting in "lost" data. I take some care when decoding bytes to characters
  to ensure a seamless and accurate transition between bytes and characters.
- `Write()` gives the data to the serial port. If the operation is asynchronous,
  the call back results in the number of bytes that were actually transferred to
  the driver. You need to check yourself if this is valid or not. In the
  synchronous case, the data is simply thrown away. The `SerialPortStream`
  method simply copies data to a local buffer and uses asynchronous writes in a
  different thread. It works in the background to send out the data you
  provided. If the data can't be sent, then you get a `TimeoutException` without
  any data being buffered at all. So you can implement reliable protocols and
  your code is simpler.
- Disposing or Closing the serial port during a blocking write operation will
  not abort the write operation. This implementation will abort with an
  `System.IO.IOException` type.

### 2.2. Differences to the MS Serial Port

The goal is to provide a `Stream`, not an API compatible replacement to the
`SerialPort`.

All data is buffered internally in memory, captured using an I/O thread. The
extra buffering adds delays by reading the bytes, then performing a context
switch for the user code to read the buffer. This can slow down your software.

Buffering solves the problem however, that data is read from the serial port in
an arbitrary sized memory buffer, and not dependent on the driver, so a
likelihood of driver underruns and overruns are reduced. This was an important
aspect when writing this library.

## 3. System Requirements

### 3.1. Testing

Software has been tested and developed using:

- .NET 6.0 and 8.0 on Windows 11 Pro x64, .NET SDK 9.x
- .NET 4.8.1 on Windows 11 Pro x64
- Mono 6.x from Xamarin on Ubuntu 22.04 (64-bit)

See later in these notes for known issues and changes.

### 3.2. Compatibility

#### 3.2.1. .NET Frameworks (Windows)

The software is written originally for .NET 4.0 and should work on those
platforms. It is extended for .NET 4.5 features. A version targets .NET Core
6.0 and 8.0.

Windows XP SP3 and later should work when using .NET 4.0. It's not possible to
run the unit tests on Windows XP since the unit tests have migrated to NUnit
4.x, but was working fine prior to that with NUnit 2.x.

#### 3.2.2. Mono Framework (Linux Only)

The `SerialPortStream` should work on Linux, and it should be possible to import
the assembly into your code when running on Linux.

When using the Mono Framework, you should reference the .NET 4.0 or .NET 4.6.2
projects.

It has been tested to compile and unit test cases pass with the `dotnet` command
on Linux.

## 4. Installation

### 4.1. Windows

On Windows, just reference the assembly in your project installing the NuGet
version.

### 4.2. Linux

You first need to compile the support library `libnserial.so` for your platform.
To do that, you'll need a compiler (e.g. GCC 4.8 or later) and `cmake`. The
binaries for Linux are not part of the distribution, as it's operating system
specific.

After cloning the repository, execute the following:

```sh
git clone https://github.com/jcurl/serialportstream.git
cd serialportstream/dll/serialunix
./build.sh
```

Binaries are built and put in the `bin` folder from where you ran the build
script. You can add a reference to `LD_LIBRARY_PATH` to the library:

```sh
export LD_LIBRARY_PATH=`pwd`/bin/usr/local/lib:$LD_LIBRARY_PATH
```

and then run your Mono program from there.

Or you can build and install in your system:

```sh
cd serialportstream/dll/serialunix
mkdir mybuild
cd mybuild
cmake .. && make
sudo make install
```

## 5. Extra Features

The following features are in addition to the `System.IO.Ports.SerialPort`
implementation:

- You can obtain the `RingIndicator` pin status.
- The `Read()` and `Write()` buffers are completely independent of the low level
  Windows driver.
  - For those concerned, the buffering means that a copy must always be made on
    every `Read()` and `Write()` method.

### 5.1. Reading and Writing - Buffering

Why is it interesting to perform buffering? A driver might be configured to be
4096 or 8192 bytes (which is quite typical). Testing with older PL2303 chipset,
one can't write more than about 12KB with a single write operation. Newer
drivers seem to allow much larger buffers.

A Write buffer may be 128KB, which one writes to. The thread in the background
will write the data and issue as many write calls as is necessary to get the job
done. A Read buffer may be 5MB. The background thread will read from the serial
port when ever data arrives and buffers into the 5MB.

So long as the I/O thread in .NET can execute every 100-200ms, it can continue
to read data from the driver. Your own application doesn't need to keep to such
difficult time constraints. Such issues typically arise in Automation type
environments where a computer has many different peripherals. So long as the
process doesn't block, your main application might sleep for 10 seconds and
you've still lost no data. The MS implementation wouldn't be so simple, you have
to make sure that you perform frequent read operations else the driver itself
might overflow (resulting in lost data).

As the writes are buffered, they tend to return immediately to the application,
instead of waiting for the write to complete. Use the new `Flush()` method to
ensure that the writes are completed.

## 6. Developer Notes

### 6.1. Logging

If you come across a problem using this library, you may be asked to provide
additional debug logs. This section describes how to obtain those logs for .NET
Framework and .NET Core.

### 6.2. The LogSource abstraction

Logging for `SerialPortStream` 3.x uses my `RJCP.Diagnostics.Trace` library that
provides an implementation called `LogSource`. This is a wrapper around
`TraceSource`, and can provide where necessary a `TraceListener` for .NET Core.
For .NET Core, it provides a factory method as a singleton as an alternative to
dependency injection. The following sections provide more details.

### 6.3. .NET Framework (.NET 4.0 to .NET 4.8)

The library uses the `TraceSource` object, so you can add tracing to your
project in the normal way. You should use the switch name
`RJCPIO.Ports.SerialPortStream`. An example of an `app.config` file that you can
use to enable logging:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <system.diagnostics>
    <sources>
      <source name="RJCP.IO.Ports.SerialPortStream" switchValue="Verbose">
        <listeners>
          <clear/>
          <add name="myListener"/>
        </listeners>
      </source>
    </sources>
    <sharedListeners>
      <add name="myListener" type="System.Diagnostics.TextWriterTraceListener" initializeData="logfile.txt"/>
    </sharedListeners>
  </system.diagnostics>
</configuration>
```

Please note, for SerialPortStream 3.x and later, the name of the trace source
has changed to include the full namespace, to be compatible with my other
projects.

### 6.4. .NET Core

.NET Core has an implementation of `TraceListener` and `TraceSource`, but it
doesn't load the `app.config` on start up, nor provide a singleton for
applications to use for tracing. There are two ways to enable logging for
`SerialPortStream` on .NET Core.

#### 6.4.1. Dependency Injection

The `SerialPortStream` has a constructor where you can provide your `ILogger`
object for tracing.

Internally, the `ILogger` is wrapped around a minimal `TraceListener`
implementation to keep the code common between .NET Framework and .NET Core.

#### 6.4.2. Singleton via LogSource

When upgrading from .NET Framework to .NET Core, it can be quite difficult to
refactor software from using a singleton pattern, to dependency injection, and
may require large swaths of code to be refactored.

To avoid this, the `LogSource` classes provide a mechanism to get an `ILogger`
using a factory method you provide.

You may add in your code the following:

```csharp
using Microsoft.Extensions.Logging;
using RJCP.CodeQuality.NUnitExtensions.Trace;
using RJCP.Diagnostics.Trace;

internal static class GlobalLogger {
    static GlobalLogger() {
        ILoggerFactory factory = LoggerFactory.Create(builder => {
            builder
              .AddFilter("Microsoft", LogLevel.Warning)
              .AddFilter("System", LogLevel.Warning)
              .AddFilter("RJCP", LogLevel.Debug)
              .AddNUnitLogger();
        });
        LogSource.SetLoggerFactory(factory);
    }

    public static void Initialize() {
        /* Intentially empty. By calling this method, the static constructor
           will be automatically called */
    }
}
```

The example above is from the unit test cases for `SerialPortStream`, but can be
easily adapted for your own projects. The important part is that:

- In your production code, you assign the static factory with
  `LogSource.SetLoggerFactory()`, which is not called as part of your unit
  tests. It will likely be different to the example given above, as you'd want
  to instead, get the logging configuration (log levels) via a configuration
  file.
- Your test code would set the `LogSource.SetLoggerFactory()` similarly as in
  the example provided above (the example given above is good for NUnit 3.x with
  the `.AddNUnitLogger()` provided by my `RJCP.CodeQuality` library).
- Setting the factory is only needed for .NET Core. On .NET Framework, the
  `TraceSource` class provides the singleton functionality and will instantiate
  for you the correct `TraceSource` from the application configuration file.

The code works by requesting to the `ILoggerFactory.CreateLogger` with the name
`RJCP.IO.Ports.SerialPortStream`. This is also the motivation why the trace
source was changed to include the full namespace. The code above shows that it
will create this object and log all debug level events to the NUnit logger.

## 7. Known Issues

### 7.1. General Issues

#### 7.1.1. ReadTo

The implementation of `ReadTo` and other character based read events with the
`SerialPortStream` is slow. It tries to calculate the size (in bytes) of each
individual character, in case the user decides to read bytes in-between. The
purpose of this was to prevent possible data loss in when a read of bytes is
mixed with a read of characters. It's recommended not to use the character based
APIs.

Some test cases are failing and needs to be investigated (although it's not
clear if this is a bug in the library or in the test case).

### 7.2. Windows

The following issues are known:

- This is not an issue in the library, but when using the `Com0Com` for running
  unit tests, some specific test cases for Parity will fail. That is because
  Com0Com doesn't emulate data at a bit level. Using real serial hardware with a
  NULL modem adapter works as expected.
- .NET 4.0 to .NET 4.8.1 and .NET Core has a minor issue in
  `System.Text.Decoder` that in a special circumstance it will consume too many
  bytes. The `PeekChar()` method is therefore a slower implementation than what
  it could be. Please refer to the Xamarin bug
  [40002](https://bugzilla.xamarin.com/show_bug.cgi?id=40002). Found against
  Mono 4.2.3.4 and later tested to be present since .NET 4.0 on Windows XP also.

#### 7.2.1. Driver Specific Issues on Windows

##### 7.2.1.1. Flow Control

Using the FTDI chipset on Windows 10 x64 (FTDI 2.12.16.0 dated 09/Mar/2016) flow
control (RTS/CTS) doesn't work as expected. For writing small amounts of data
(1024 bytes) with CTS off, the FTDI driver will still send data. See the test
case `ClosedWhenFlushBlocked`, change the buffer from 8192 bytes to 1024 and the
test case now fails. This problem is not observable with com0com 3.0. You can
see the effect in logs, there is a TX-EMPTY event that occurs, which should
never be there if no data is ever sent.

##### 7.2.1.2. BytesToWrite

On Windows, the `SerialPortStream` returns the bigger of either the internal
write buffer, or the amount of data in the output queue of the driver. Drivers
don't report the number of bytes that are in the output queue before the next
write begins, and may return sooner. This leads to the effects:

###### 7.2.1.2.1. CP2101 Driver

This driver indicates more bytes are in the output queue than what it will
return from the current ongoing write operation. This can cause some jumps in
the returned value.

[CP210x Universal Windows
Driver](https://www.silabs.com/developers/usb-to-uart-bridge-vcp-drivers)
v10.1.10 1/13/2021.

For example:

```text
BytesToWrite = 40960 (driver 12288)
RJCP.IO.Ports.SerialPortStream Verbose: 0 : COM5: SerialThread: ProcessWriteEvent: 1024 bytes
BytesToWrite = 40412 (driver 40412)
RJCP.IO.Ports.SerialPortStream Verbose: 0 : COM5: SerialThread: DoWriteEvent: WriteFile(736, 312385272, 39936, ...) == False
BytesToWrite = 40387 (driver 40387)
BytesToWrite = 40386 (driver 40386)
```

The internal buffer is 40kB, the driver returned it wrote 1024 bytes, but the
queue still has 40412 bytes (which is more than the 39936 bytes it should be).

It can also fluctuate without writes without calls to the OS in between.

```text
BytesToWrite = 40418 (driver 40418)
BytesToWrite = 40393 (driver 40393)
BytesToWrite = 40392 (driver 40392)
BytesToWrite = 40391 (driver 40391)
BytesToWrite = 40390 (driver 40390)
BytesToWrite = 40389 (driver 40389)
BytesToWrite = 40428 (driver 40428)
BytesToWrite = 40427 (driver 40427)
BytesToWrite = 40426 (driver 40426)
```

###### 7.2.1.2.2. PL2303 RA

Generally this driver reports that it has zero bytes in the output queue, but
may sometimes report the number of bytes in the last `WriteFile()` call. This is
not a problem, but the number of bytes in the output queue is less than what is
still to be written, so a user may think it is complete, when it is not.

The drivers only work when flushing if the `WriteTotalTimeoutConstant` is zero.
That means doing something like:

```csharp
var serialPort = new RJCP.IO.Ports.WinSerialPortStream(com, 115200, 8, RJCP.IO.Ports.Parity.None, IO.Ports.StopBits.One);
serialPort.Settings.WriteTotalTimeoutMultiplier = 0;
serialPort.Settings.WriteTotalTimeoutConstant = 500;
serialPort.Open();
```

will cause flush to come back too soon. In version 3.0.0 - 3.0.2, the default
was 500 and was changed to zero as part of [Issue
#154](https://github.com/jcurl/RJCP.DLL.SerialPortStream/issues/154).

### 7.3. Linux

The `SerialPortStream` was tested on Ubuntu 14.04 to 22.04. Feedback welcome for
other distributions!

The main functionality on Linux is provided by a support C library that
abstracts the Posix system call `select()`.

Issues in the current implementation are:

- Custom baud rates are not supported. To know what baud rates are supported on
  your system, look at the file `config.h` after building.
- DSR and DTR handshaking is not supported. You can still set and clear the pins
  though.

Patches are welcome to implement these features!

#### 7.3.1. Mono on non-Windows Platforms

Use the currently supported versions of Mono provided by the Mono project for
your Linux distribution. For example, Ubuntu 14.04 ships with Mono 3.2.8 which
is known to not work.

- [[Mono-Dev] Mono 3.2.8 incompatibility with .NET 4.0 on Windows
  7-10](http://lists.ximian.com/pipermail/mono-devel-list/2015-December/043423.html).
  The System.Text implementation for converting bytes to UTF8 don't work. If you
  don't use the character based methods, it may work. But the software has not
  been tested against this framework.
- The `DataReceived` event doesn't fire for the EOF character (0x1A). On Windows
  it does, as this is managed by the driver itself and not emulated by the
  C-Library.
- Linux doesn't implement DSR.

#### 7.3.2. Driver Specific Issues on Linux

Tests have been done using FTDI, PL2303H, PL2303RA and 16550A (some still do
exist!).  The following has been observed:

##### 7.3.2.1. Parity Errors

Some chipsets do not report properly parity errors. The 16550A chipset works as
expected. Issues observed with FTDI, PL2303H, PL2303RA. In particular, on a
parity error, more bytes are reported as having parity errors than there are in
the stream. Tested using loopback devices with `comptest`.

```sh
$ ./nserialcomptest /dev/ttyUSB0 /dev/ttyUSB1`
  [ RUN      ] SerialParityTest.Parity7E1ReceiveError
/home/jcurl/Programming/serialportstream/dll/serialunix/libnserial/comptest/SerialParityTest.cpp:221: Failure
Value of: comparison
  Actual: false
Expected: true
Unexpected byte received with Even Parity
[  FAILED  ] SerialParityTest.Parity7E1ReceiveError (585 ms)
[ RUN      ] SerialParityTest.Parity7O1ReceiveError
/home/jcurl/Programming/serialportstream/dll/serialunix/libnserial/comptest/SerialParityTest.cpp:373: Failure
Value of: comparison
  Actual: false
Expected: true
Unexpected byte received with Even Parity
[  FAILED  ] SerialParityTest.Parity7O1ReceiveError (584 ms)
[ RUN      ] SerialParityTest.Parity7O1ReceiveErrorWithReplace
/home/jcurl/Programming/serialportstream/dll/serialunix/libnserial/comptest/SerialParityTest.cpp:427: Failure
Value of: comparison
  Actual: false
Expected: true
Unexpected byte received with Even Parity
[  FAILED  ] SerialParityTest.Parity7O1ReceiveErrorWithReplace (572 ms)
```

##### 7.3.2.2. Garbage Data on Open

On Linux Kernel with Ubuntu 14.04 and Ubuntu 16.04, we observe that some USB-SER
drivers provide extra data depending on what a previous process was doing. It
shows itself as garbage zero's appearing at the beginning of a stream when
reading, and may be visible in your application also. There's a test case
`comptest/kernelbug` that shows this behaviour on a Lenovo T61p. Affected is
PL2303H and FTDI chipsets. Chipsets that don't show this behaviour are 16550A
and PL2303RA chipsets. Invocate the test program twice and you'll see the error.
This is reported to
[Ubuntu](https://bugs.launchpad.net/ubuntu/+source/linux/+bug/1542862)

```sh
$ kernelbug /dev/ttyUSB0 /dev/ttyUSB1
Offset: 4
Flushing...
Writing Complete...
Reading complete...
Comparison MATCH                    <---- PASS
Flushing...
Reading complete...
Complete...

$ kernelbug /dev/ttyUSB0 /dev/ttyUSB1
Offset: 108
Flushing...
Flush 2 bytes
Writing Complete...
Reading complete...
ERROR: Comparison mismatch          <---- ERROR
Flushing...
Flush 510 bytes
Reading complete...
Complete...
```

##### 7.3.2.3. Monitoring Pins and Timing Resolution

Monitoring of pins CTS, DSR, RI and DCD is not 100% reliable for some chipsets
and workarounds are in place. In particular, the chips PL2303H, PL2303RA do not
support the `ioctl(TIOCGICOUNT)`, so on a pin toggle, we cannot reliably detect
if they have changed if the pulse is too short. For 16550A and FTDI chips, this
`ioctl()` does work and so we can always detect a change. To check if your
driver supports the `ioctl(TIOCGICOUNT)` call, run the small test program
`comptest/icount`.

```sh
$ ./icount /dev/ttyS0
Your driver supports TIOCGICOUNT
ocounter.cts=0
ocounter.dsr=0
ocounter.rng=3
ocounter.dcd=0
```

or in the case it's not supported:

```sh
$ ./icount /dev/ttyUSB0
Your driver doesn't support TIOCGICOUNT
  Error: 25 (Inappropriate ioctl for device)
```

##### 7.3.2.4. Close Times with Flow Control

Some times closing the serial port may take a long time (observed from 5s to
21s) if it is write blocked due to hardware flow control. In particular, the
C-library function `serial_close()` appears to take an excessive time when
calling `close(handle->fd)` on Ubuntu 16.04. This issue appears related to the
Linux driver and not the MONO framework.

The .NET Test Cases that show this behaviour are (blocking on write):

- `ClosedWhenBlocked`
- `CloseWhenFlushBlocked`
- `DisposeWhenBlocked`
- `DisposeWhenFlushBlocked`

This issue is not reproducible with the 16550A UART when it is write blocked. In
this case, the times for closing are usually not more than 20ms.

##### 7.3.2.5. Opening Ports (and some unit test case failures)

When testing continuously to open a port, send data, and then receive on another
port using a NULL-modem cable, minor issues can occur that result in test case
failures. These issues would also be visible in real-world programs and are
driver dependent.

The test scenario is on Linux (Ubuntu 20.04) with various USB serial port
devices. The test case `ReadToWithMbcs` from SerialPortStreamNativeTest is
modified to run 2000 times with `[Repeat(2000)]`. The command to run the test
after building is then:

```cmd
dotnet test RJCP.SerialPortStreamNativeTest.dll --filter Name=ReadToWithMbcs
```

(please note, not only this test case is affected, but it is easy to reproduce.)

- *FTDI*: After opening the serial port, in about 1% of the cases, data is not
  sent (observed using .NET Mono 4.0 and 4.5). It is confirmed that the system
  call `write()` was called, and all data was given to the kernel via the
  library `libnserial`. However, on the other serial port, data is never
  received. Waiting 15ms after opening would resolve the problem - this
  workaround will not be part of the `SerialPortStream` as it appears to be very
  specific to this driver and similar behaviour is not observed on other
  drivers.
- *PL2303H*: Sometimes on connecting the serial port, a spurios 0xFF is sent.
  This causes the test case to fail, as data that was not sent is received and
  affects the output.
- *PL2303RA*: The test cases appear to run about 10x slower than any other
  driver, but no errors were observed.
- *CP2101*: Seems to work flawlessly.

### 7.4. Guidelines on Serial Protocols

Given the issues listed in this section, one can come up with the following
recommendations for protocol design over the serial port:

- Assume that at Layer 2 (the serial port bus), data can be inserted, modified,
  or deleted.
- Define data as frames. There should be a marker byte indicating the start of a
  frame, a length to know how much data should be received, and a checksum (at
  least a CRC16) that can be used to check the integrity of the frame.
- Define the protocol to be able to resend data in case of lost data if needed,
  or can continue if data is lost.

I recommend to not use hardware or software flow control, but define in the
serial protocol frames, like a link control protocol (LCP) that can manage this.
Do not use parity, and instead opt to use checksum bytes within a frame.

- Hardware flow control can lead to deadlocks in software. No flow control just
  means data can be lost, and can be replaced using a LCP. Software flow control
  can also cause complications in the protocol, which can be more generically
  handled using a LCP.
- Parity can insert arbitrary bytes and corrupted data, especially with USB
  serial devices. Use frame checksums (FCS) instead.

Allow bundling of frames one after the other, and decode separately. Lots of
small frames that need to be acknowledged can lead to delays between frames, and
longer transmission times for an already "slow" bus speed. The
`SerialPortStream` is buffered, so performance is impacted by lots of context
switches between sending data, and waiting for a response, as there is a buffer
thread used in-between.
