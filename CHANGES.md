# List of Changes with Releases

## Version 3.0.0 (libnserial 1.1.4) - 18/Oct/2021

Features

* DOTNET-328: Add logging/debugging support for .NET Core
* DOTNET-404, DOTNET-405: Expose the `INativeSerial` to user code for simulating
  serial ports. add a new `SerialPortStream.Virtual` library to show how this
  works, that can be used in user code testing.
* DOTNET-410: Expose CommTimeout properties on Windows in the
  `WinSerialPortStream` class. Provide a factory `SerialPortStreamFactory` that
  automatically instantiates the correct stream object depending on the OS. If
  you don't need this, you can still instantiate the `SerialPortStream` as
  before.
* DOTNET-407: Provide Authenticode signing and GIT information in the build with
  `RJCP.MSBuildTasks`.
* DOTNET-426: Properly implement `ReadAsync` and `WriteAsync` with
  `CancellationToken`, based on RJCP.IO.Buffer.

Fixes

* DOTNET-333: The `ISerialPortStream` interface for NETSTANDARD as well as .NET
  4.5
* DOTNET-423: Fix potential data corruption on Linux as buffers aren't locked.
* DOTNET-422: `DiscardOutBuffer` should clear memory buffers in addition to the
  driver buffers.
* DOTNET-425: BytesToWrite shouldn't return more bytes than what was requested
  to write.
* DOTNET-428: Name of properties are used when raising exceptions, instead of
  "value".
* DOTNET-433: Don't request unneeded events on Windows.

Refactoring:

* DOTNET-329: Migrate to .NET SDK Project
  * DOTNET-330: with .NET 4.0, 4.5
  * DOTNET-331: with .NET Core (API .NET Standard 1.5)
  * DOTNET-334: Unit Test cases for .NET Core 3.1 (tests .NET Standard)
  * DOTNET-401: Upgrade from .NET Standard 1.5 to 2.1 and remove compatibility
    code
  * DOTNET-420: Ensure Mono assets are not deployed / referenced.
* DOTNET-185: Port test cases to NUnit 3.x
* Remove unneeded references for .NET Standard
* DOTNET-398: Clean up and remove code analysis suppressions and rework
* Clean up exception usage (parameter names, variable names, etc.)
* DOTNET-408: Refactor P/Invokes into individual classes
* DOTNET-399: Use the NuGet package RJCP.IO.BufferIO. Removes
  `InternalsVisibleTo`.
  * DOTNET-399: CircularBuffer
  * DOTNET-409: TimerExpiry
  * DOTNET-400: AsyncResult
  * DOTNET-416: MemoryReadBuffer, MemoryWriteBuffer.
* DOTNET-402: Use RJCP.Trace for `LogSource` classes (tracing for .NET 4.0 and
  .NET Core).
* DOTNET-418: Use Timeout.Infinite (-1) from framework, instead of our own
  constant.
* Remove internal `SerialBuffer.IsPinnedBuffer` as it's not used.
* Remove unneeded implementation of `SerialData.Reset(bool)`.
* Test cases should use `TaskFactory().StartNew()` to not crash test
  cases in .NET Core.
* DOTNET-404: Remove `INativeSerialDll` internal implementation.
* DOTNET-435: LibNSerial for Unix is now static. Original intent was to handle
  multiple "bitness", but this appears to be handled properly already on Linux,
  and 64-bit seems to be the standard now.
* DOTNET-403: Better testing with `SerialPortStream.Virtual` to abstract
  hardware. Separate unit tests and integration tests.
* DOTNET-429: Simplify logging for test cases slightly.

Source

* Add editor support for VSCode

## Version 2.3.1 (libnserial 1.1.4) - 19/Apr/2021

Bugfixes

* [Issue #116](https://github.com/jcurl/SerialPortStream/issues/116): Fix
  `ReadAsync()` and `WriteAsync()` on .NET Standard 1.5 (.NET Core and .NET 5.0 and
  later).

## Version 2.3.0 (libnserial 1.1.4) - Do not use - 13/Apr/2021

Features

* [Issue #114](https://github.com/jcurl/SerialPortStream/issues/114): Provide
  `ReadAsync()` and `WriteAsync()` implementation
  * Note, the `ReadAsync` `CancellationToken` is ignored
  * This implementation is broken on .NET Core (.NET Standard 1.5). Works on
    .NET 4.5

Bugfixes

* [Issue #110](https://github.com/jcurl/SerialPortStream/issues/110): Fix ReadFile P/Invoke

## Version 2.2.2 (libnserial 1.1.4) - 5/Jul/2020

Bugfixes

* DOTNET-194: Prevent exceptions when converting bytes to chars when using
  ISO-8859-15.

## Version 2.2.1 (libnserial 1.1.4) - 8/May/2020

Bugfixes

* DOTNET-180: Allow compilation of libnserial on Ubuntu 16.04.5.
* [Issue #104](https://github.com/jcurl/SerialPortStream/issues/104): Fix buffer
  handling. `Write()` would sometimes corrupt data.
* [Issue #90](https://github.com/jcurl/SerialPortStream/issues/90): `IsOpen()`
  might return `NullReferenceException` as it's not thread safe with `Close()`.

Features:

* DOTNET-184: Update to NUnit 2.7.1.
* DOTNET-186: Update codebase to use new features introduced with C# 7.0
  (VS2019).
* [Issue #82](https://github.com/jcurl/SerialPortStream/issues/82): Provide
  release note documentation

## Version 2.2.0 (libnserial 1.1.4) - 14/Nov/2018

Bugfixes

* [Issue #62](https://github.com/jcurl/SerialPortStream/issues/62): Allow
  RTS/DTR to be set before serial port is open on UNIX.
* [Issue #64](https://github.com/jcurl/SerialPortStream/issues/64): Windows -
  Check registry if port is not CHAR/UNKNOWN.
* DOTNET-171: libnserial: Fix port detection on Linux.
* DOTNET-172: libnserial: Show the handle value when logging on Linux.
* [Issue #69](https://github.com/jcurl/SerialPortStream/issues/69): `IsOpen`
  should return false when the serial port device is removed.
* DOTNET-175: Windows usage of `ClearCommError` was incorrect.
* DOTNET-176: Don't lose data on a `Write()`.

Features

* [Pull #77](https://github.com/jcurl/SerialPortStream/pull/77): Add interface
  for the `SerialPortStream`.

## Version 2.1.4 (libnserial 1.1.3) - 1/Jun/2018

Bugfixes

* DOTNET-162: Reduce number of objects created when writing, reading, flushing.
* DOTNET-163: Reduce CPU and Memory if tracing is disabled.
* DOTNET-164: Reduce number of `WaitHandle`s created in an I/O loop.
* DOTNET-165: Don't use `Enum.HasFlag` as it's slow, boxes and uses the GC.

Features

* [Issue #55](https://github.com/jcurl/SerialPortStream/issues/55): Sign the
  .NET Standard 1.5 library.

## Version 2.1.3 (libnserial 1.1.2) - 26/Apr/2018

Bugfixes

* [Pull #40](https://github.com/jcurl/SerialPortStream/pull/40): Correct paths
  for CMake in `README.md`.
* DOTNET-154: The `IsDisposable` flag should not be publicly settable.
* [Issue #50](https://github.com/jcurl/SerialPortStream/issues/50): Avoid
  deadlock in event handling.

Features

* [Issue #20](https://github.com/jcurl/SerialPortStream/issues/20): Upgrade
  project to work with .NET Core 1.0.4.

## Version 2.1.2 (libnserial 1.1.2) - 26/May/2017

Bugfixes

* [Issue #22](https://github.com/jcurl/SerialPortStream/issues/22): Abort pin
  monitoring on Linux for devices that don't support it.
* DOTNET-89: Properly clean up pin monitoring thread on Linux.
* DOTNET-86, 87: Fix race condition when checking/aborting modem events.
* DOTNET-93: Make usage of `pthread_setcancel{state|type}` portable.
* DOTNET-94: Properly handle errors from pthreads.
* DOTNET-95: Correct race condition when aborting a modem event.
* DOTNET-91: Treat `EINTR` as a non-fatal interrupt.
* [Issue #24](https://github.com/jcurl/SerialPortStream/issues/24): Thread names
  on Linux too long.
* DOTNET-100: Use a `SafeHandle` for `libnserial`.
* [Issue #25](https://github.com/jcurl/SerialPortStream/issues/25): Support
  devices that don't work with overlapped WaitCommEvent.
* DOTNET-106: Allow building for OSes that don't support TIOCNXCL and TIOCEXCL.

Features

* [Issue #26](https://github.com/jcurl/SerialPortStream/issues/26): Provide an
  implementation for DiscardInBuffer and DiscardOutBuffer.

## Version 2.1.1 (libnserial 1.1.1) - 8/Apr/2017

Bugfixes

* DOTNET-82: Close a serial port properly on Linux with Native Lib.

## Version 2.1.0 (libnserial 1.1.0) - 25/Mar/2017

Bugfixes

* DOTNET-34: Use a smarter algorithm to get the available ports on Linux.

Features

* [Issue #10](https://github.com/jcurl/SerialPortStream/issues/10): Support .NET
  Standard 1.5 (.NET Core 1.0 and .NET FX 4.6.x).
* DOTNET-75: Add proper exception support for Linux.
* DOTNET-74: Refactor projects to separate .NET 4.0 and .NET 4.5 for
  multitargetting.

## Version 2.0.3 (libnserial 1.0.1) - 22/Jan/2017

Bugfixes

* DOTNET-30: Don't call `SetCommBreak` or `ClearCommBreak` on opening the
  `SerialPortStream` (or when changing any other property either).
* DOTNET-45: Dispose `ManualResetEvent` (Linux).
* DOTNET-41: Remove double initialisation of property `m_TxContinueOnXOff` in
  `GetPortSettings()`.
* DOTNET-41: Make `IsDisposed` internally a volatile.
* DOTNET-41: Properly implement singleton method for tracing.

Features

* DOTNET-33: Support CMake `find-package` by providing `nserialConfig.cmake`.
* DOTNET-58: Force exclusive access of the serial port for non-root processes
  (Linux).
* DOTNET-62: Enable possibility to log open/close issues.

## Version 2.0.2 (libnserial 1.0.0) - 13/Oct/2016

Bugfixes

* `ToString()` shouldn't raise an exception in case the device causes problems.

## Version 2.0.1.1 (libnserial 1.0.0) - 7/Sep/2016

Bugfixes

* Reenable tracing in the final Nuget package.

## Version 2.0.1 (libnserial 1.0.0) - 7/Sep/2016

Bugfixes

* `OverlappedResult` should wait when completing.

## Version 2.0.0 - 25/May/2016

This is a rewrite from v1.x

Bugfixes

* Don't raise an exception in `GetPortDescriptions()` if no COM ports are
  registered.
* Fix race condition in events, make events protected virtual for overrides.
* Fix exception on Windows for serial drivers that don't support breaks.
* Fix reading characters for `System.Text.Decoder` bug found on Windows also.
* Fix `Write()` when closed to raise the correct exception, which is
  `InvalidOperationException`.
* When closing/disposing during an active write, we now raise an exception.
* Use a more explicit solution for aborting a `Write()` by aborting it
  explicitly on `Close()`.
* Fix RTS and DTR control on opening the port.
* Flush now properly aborts when closed/disposed while blocked.
* Change `InvalidOperationException` during a blocking write to `IOException`.
* `Read()` now returns when `Close()`d or `Dispose()`d.
* Raise `IOException` when the device is removed immediately on `Write`.
* Stop the Win32 `SerialPortStream` in case of an error.

Features

* Major refactoring for `WinNative` against a more formal architecture.
* Provide DLL used for running under Unix operating systems.
