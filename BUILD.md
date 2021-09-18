# Build and Test Instructions

The project has been migrated to the .NET SDK project format. This is to allow
better integration with .NET Core projects, and to allow better unit testing.

* 1.0 Windows
  * 1.1 Building
  * 1.2 Packaging
  * 1.3 Unit Tests
    * 1.3.1 Configuring Tests
    * 1.3.2 Running Unit Tests
    * 1.3.3 Probable Failing Tests
* 2.0 Linux
  * 2.1 .NET Core Installation
  * 2.2 Building the libnserial Library
    * 2.2.1 Ubuntu Packages
  * 2.3 Building on Linux
  * 2.4 Running Unit Tests on Linux
    * 2.4.1 Preconditions
    * 2.4.2 Executing Unit Tests on Linux
    * 2.4.3 Probable Failing Tests for Linux
* 3.0 Developer
  * 3.1 .NET Framework SDK Project
  * 3.2 .NET Standard 1.5

## 1.0 Windows

### 1.1 Building

To build the software, ensure to be in the working directory where the
`SerialPortStream.sln` file is kept.

* To build DEBUG mode. This does not generate a NuGet package, to prevent
  mistakes in deployment.

  ```cmd
  PS1> dotnet build
  ```

* To build RELEASE mode. Release mode is unsigned, useful for beta testing or
  your own testing.

  ```cmd
  PS1> dotnet build -c Release
  ```

### 1.2 Packaging

To build the package for upload (see building for the sign key) to NuGet:

```cmd
PS1> dotnet build -c release .\code\SerialPortStream.csproj
PS1> dotnet pack -c release --include-source .\code\SerialPortStream.csproj
```

I generally upload the symbols version that also includes the sources.

### 1.3 Unit Tests

#### 1.3.1 Configuring Tests

The tests written are a combination of unit tests and integration tests. The
tests can be run with the driver Com0Com. The ports used for the source and
destination are defined in `test\SerialPortStreamTest\App.config`. The default
is:

```xml
  <appSettings>
    <add key="Win32SourcePort" value="CNCA0"/>
    <add key="Win32DestPort" value="CNCB0"/>
    <add key="LinuxSourcePort" value="/dev/ttyUSB0"/>
    <add key="LinuxDestPort" value="/dev/ttyUSB1"/>
  </appSettings>
```

Change the `Win32SourcePort` and `Win32DestPort` to be the actual hardware (e.g.
COM1, COM2).

#### 1.3.2 Running Unit Tests

Unit tests are supported for the configurations `Debug` and `Release`. Unit
tests marked as [Explicit]` will not run. But you can run these from within the
Visual Studio IDE.

* To test DEBUG mode

  ```cmd
  PS1> dotnet test
  ```

* To test RELEASE mode

  ```cmd
  PS1> dotnet test -c Release --logger "trx"
  ```

To skip the manual tests in the Visual Studio IDE, filter with
`-Trait:ManualTest`.

#### 1.3.3 Failing Tests on Windows

When using the Com0Com driver, the following tests will fail (but pass with real
hardware):

* `EvenParityLoopback`
* `OddParityLoopback`
* `ParityChangeLoopback`

The reason for this is that the driver doesn't take parity into account. It
sends 8-bit, but receives 7-bit parity. When using real cables with null-modem
adapters, the test cases should pass, as the hardware interprets the parity bits
correctly.

## 2.0 Linux

Building on Linux should be done with the Microsoft `dotnet` commands.
Development should be done with VSCode. References to the MONO IDE have been
removed.

### 2.1 .NET Core Installation

The instructions here were written and tested for Ubuntu 20.04. See Microsoft
documentation for other Operating Systems.

To build on Linux, install the .NET SDK. For example, on Ubuntu the instructions
are given at [Install the .NET SDK or the .NET Runtime on
Ubuntu](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu)

1. Preconditions

   ```cmd
   $ wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
   $ sudo dpkg -i packages-microsoft-prod.deb
   ```

2. Install the SDK

   ```cmd
   $ sudo apt-get update; \
    sudo apt-get install -y apt-transport-https && \
    sudo apt-get update && \
    sudo apt-get install -y dotnet-sdk-3.1
   ```

### 2.2 Building the libnserial Library

You should install the latest version of libnserial. The package for Ubuntu is
given on the release pages. For other Operating Systems, build and install from
sources (see the folder `dll\serialunix` for more information).

```cmd
# apt install libnserial_1.1.4-0ubuntu1~focal1_amd64.deb
```

#### 2.2.1 Ubuntu Packages

* 1.1.4
  * [libnserial-1.1.4 trusty 32/64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.0.0/libnserial-1.1.4-0ubuntu1.trusty1.zip)
  * [libnserial-1.1.4 xenial 32/64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.0.0/libnserial-1.1.4-0ubuntu1.xenial1.zip)
  * [libnserial-1.1.4 yakkety 32/64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.0.0/libnserial-1.1.4-0ubuntu1.yakkety1.zip)
  * [libnserial-1.1.4 zesty 32/64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.0.0/libnserial-1.1.4-0ubuntu1.zesty1.zip)
  * [libnserial-1.1.4 artful 32/64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.0.0/libnserial-1.1.4-0ubuntu1.artful1.zip)
  * [libnserial-1.1.4 bionic 32/64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.0.0/libnserial-1.1.4-0ubuntu1.bionic1.zip)
  * [libnserial-1.1.4 cosmic 32/64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.0.0/libnserial-1.1.4-0ubuntu1.cosmic1.zip)
  * [libnserial-1.1.4 disco 64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.2.0/libnserial-1.1.4-0ubuntu1.disco1.zip)
  * [libnserial-1.1.4 eoan 64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.2.0/libnserial-1.1.4-0ubuntu1.eoan1.zip)
  * [libnserial-1.1.4 focal 64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.2.0/libnserial-1.1.4-0ubuntu1.focal1.zip)
  * [libnserial-1.1.4 groovy 64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.2.2.0/libnserial-1.1.4-0ubuntu1.groovy1.zip)
* 1.1.2
  * [libnserial-1.1.2-precise 32/64-bit](https://github.com/jcurl/SerialPortStream/releases/download/release%2F2.1.2.0/libnserial_1.1.2-0ubuntu1.precise1.zip)

### 2.3 Building on Linux

The instructions to build are the same as Windows, e.g. from the directory where
the `SerialPortStream.sln` file is kept:

```cmd
$ dotnet build
Microsoft (R) Build Engine version 16.9.0+57a23d249 for .NET
Copyright (C) Microsoft Corporation. All rights reserved.

  Determining projects to restore...
  All projects are up-to-date for restore.
  SerialPortStream -> /home/jcurl/source/rjcp.dll.serialportstream/code/bin/Debug/net40/RJCP.SerialPortStream.dll
  SerialPortStream -> /home/jcurl/source/rjcp.dll.serialportstream/code/bin/Debug/net45/RJCP.SerialPortStream.dll
  SerialPortStreamTest -> /home/jcurl/source/rjcp.dll.serialportstream/test/SerialPortStreamTest/bin/Debug/net45/RJCP.SerialPortStreamTest.dll
  DatastructuresTest -> /home/jcurl/source/rjcp.dll.serialportstream/test/DatastructuresTest/bin/Debug/net40/RJCP.DatastructuresTest.dll
  SerialPortStreamTest -> /home/jcurl/source/rjcp.dll.serialportstream/test/SerialPortStreamTest/bin/Debug/net40/RJCP.SerialPortStreamTest.dll
  DatastructuresTest -> /home/jcurl/source/rjcp.dll.serialportstream/test/DatastructuresTest/bin/Debug/net45/RJCP.DatastructuresTest.dll
```

### 2.4 Running Unit Tests on Linux

#### 2.4.1 Preconditions

You will need to have two USB devices attached, that map to:

* `/dev/ttyUSB0`; and
* `/dev/ttyUSB1`

that are connected via a null-modem physical connector. Many of the test cases
are integration tests and rely on two serial ports to work properly.

The devices used are defined in the `SerialPortStreamTest\App.config` file, the
same as for Windows, but the variables `LinuxSourcePort` and `LinuxDestPort`.

Ensure that the user running the tests have permissions to access the serial
ports. e.g. on Ubuntu this is not the case by default:

```cmd
$ ls -l /dev/ttyU*
crw-rw---- 1 root dialout 188, 0 Apr 24 18:32 /dev/ttyUSB0
crw-rw---- 1 root dialout 188, 1 Apr 24 18:32 /dev/ttyUSB1

$ sudo usermod -aG dialout <user>
```

Replace `<user>` with the user name.

After assigning permissions, you may need to log out and log back in.

#### 2.4.2 Executing Unit Tests on Linux

Executing the tests is the same as on Windows.

```cmd
$ dotnet test
```

#### 2.4.3 Failing Tests for Linux

There are three failing tests on Linux. This functionality is missing on Linux,
but provided on Windows. Linux doesn't provide this functionality natively.

* `WaitForRxCharEventOnEofChar`
* `WaitForCtsChangedEvent`
* `WaitForDsrChangedEvent`

The following tests occasionally fail

* `ReadToWithMbcs`
* `ReadToResetWithMbcs2`
* `WriteLineReadLineTimeout1`

## 3.0 Developer

### 3.1 .NET Framework SDK Project

This project uses the newest Microsoft SDK Project format, which is simpler and
easier to write. It has been modified from the original to require explicit
inclusion of files (this is for safety reasons to ensure that unexpected files
do not get included).

The library targets .NET 4.0, .NET 4.5, .NET Standard 1.5; where as the unit
tests are using NUnit 3.x and target .NET 4.0, .NET 4.5 and .NET Core App 3.1
(LTS).

When adding files, you'll need to look and modify the `.csproj` files directly,
Visual Studio 2019 will likely not be able to put the files in the correct
`<ItemGroup/>`.

### 3.2 .NET Standard 1.5

This project also targets .NET Standard 1.5. There are some features that are
available in .NET 4.x that are not available in .NET Standard 1.5, and so there
are replacement libraries in the `System` folder, which are not otherwise
needed.

#### 3.2.1 Logging in Unit Tests

The Unit Tests for .NET Standard 1.5 is run by compiling for .NET Core App 3.1.
In .NET Framework, logging is done using the `TraceSource` which doesn't work
well in .NET Core, and so the unit test cases come with its own implementation.
See the files under the `SerialPortStreamTest\Trace` folder.
