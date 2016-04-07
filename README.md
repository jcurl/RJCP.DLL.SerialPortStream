SerialPortStream is an independent implementation of
System.IO.Ports.SerialPort and SerialStream for better reliability and
maintainability.

The SerialPortStream is a ground up implementation of a Stream that buffers
data to and from a serial port. It uses low level Win32API for managing events
and asynchronous I/O, using a programming model as in the MSDN
[PipeServer](http://msdn.microsoft.com/en-us/library/windows/desktop/aa365603.aspx)
example.

# Why another Serial Port implementation?

Microsoft already provides a reasonable implementation for accessing the
serial port. Unfortunately, documentation is sparse. When one tries to find
information about how to program the serial port one comes across instead many
blogs and forums describing the issues that they've observed.

Through the implementation of SerialPortStream, I've used [ILSpy](http://ilspy.net)
to reverse engineer how the Microsoft implementation works, discovering
many other subtle, but noteworthy implementation issues.

## System Requirements

### Tested

Software has been developed using:
* .NET 4.0 and 4.5 on Windows 7 x86 and x64.
* .NET 4.5 on Windows 8 Pro x64 and Windows 8.1 Pro x64.

I use this software for automation in another system that runs for multiple
days and it appears stable.

### Not Compatible

SerialPortStream is not designed for the Compact Framework. Nor will it work
in the Mono framework due to the heavy dependencies on the Win32 API.

### Untested, but should work:

Theoretically it should work on the following, but it hasn't been tested.
* It should work on Windows XP with .NET 4.0

### Installation

You can download the release for 1.2.0, which contains binaries compiled
against Framework version 4.0 and 4.5. You can also install the NuGet package
at http://www.nuget.org/packages/SerialPortStream/

### Goals

This project tries to achieve the following:
* An implementation similar to the MS implementation of SerialPort. It's not
  meant to be 100% compatible, but instead provide similar functionality
* Abstract the driver implementation and provide for a more reliable
  transport, by making writing serial data completely buffered. With the MS
  implementation, one can write data, but subsequently needs to check if all
  data is written or not. If it isn't written, then it needs to be retried.
  The SerialPortStream makes this easier.
* Provide for reliable and consistent behaviour. See the next section.

## Issues with MS Serial Port

The SerialPortStream tries to solve the following issues observed:
* Zach Saw describes issues regarding behaviour of the fAbortError flag in the
  Serial DCB. The SerialPortStream defines this flag.
* Closing a serial port, then reopening it generally causes problems. The
  SerialPortStream shouldn't have this issue.
* The ReadTo() implementation can subtly change the byte stream buffer, when
  one switches from characters to bytes. This problem occurs because the MS
  implementation actually converts the characters back to bytes into its
  buffer. So if you have UTF8, decoded some invalid characters and then have a
  timeout, this results in the invalid characters being converted back to
  bytes, resulting in "lost" data. I take some care when decoding bytes to
  characters to ensure a seamless and accurate transition between bytes and
  characters.
* Write() gives the data to the serial port. If the operation is asynchronous,
  the callback results in the number of bytes that were actually transferred
  to the driver. You need to check yourself if this is valid or not. In the
  synchronous case, the data is simply thrown away. The SerialPortStream
  method simply copies data to a local buffer and uses asynchronous writes in
  a different thread. It works hard in the background to send out the data you
  provided. If the data can't be sent, then you get a TimeoutException without
  any data being buffered at all. So you can implement reliable protocols and
  your code is simpler.

## Extra Features

* You can obtain the RingIndicator pin status.
* The Read() and Write() buffers are completely independent of the low level
  Windows driver.

### Reading and Writing - Buffering

Why is it interesting to perform buffering? A driver might be configured to be
4096 or 8192 bytes (which is quite typical). Testing with the PL2303 chipset,
one can't write more than about 12KB with a single write operation.

A Write buffer may be 128KB, which one writes to. The thread in the background
will write the data and issue as many write calls as is necessary to get the
job done. A Read buffer may be 5MB. The background thread will read from the
serial port when ever data arrives and buffers into the 5MB.

So long as the I/O thread in .NET an execute every 100-200ms, it can continue
to read data from the driver. Your own application doesn't need to keep to
such difficult time constraints. Such issues typically arise in Automation
type environments where a computer has many different peripherals. So long as
the process doesn't block, your main application might sleep for 10 seconds
and you've still lost no data. The MS implementation wouldn't be so simple,
you have to make sure that you perform frequent read operations else the
driver itself might overflow (resulting in lost data).
