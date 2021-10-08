# Serial Port Stream - Virtual Native Implementation

SerialPortStream v3.0 provides a constructor that can accept an object
implementing the interface `INativeSerial`. For Windows, the normal
implementation is `WinNativeSerial`, and for Linux (and Mac) it is
`UnixNativeSerial`.

For the purposes of testing the serial port stream and protocols, one can use
this package and inject the `VirtualNativeSerial` which doesn't talk to low
level devices. Instead, user application code can use the `VirtualNativeSerial`
to simulate a virtual serial port connection.

It's main purpose it to support unit testing and integration testing, that code
can implement an class to implement a protocol, and talk to the application
under test via the `VirtualNativeSerial`.

By not using global resources (it's a virtual implementation), unit tests can
happily run in parallel (unlike when performing integration tests and relying on
physical hardware, or drivers that must register the device names and are global
for all applications, preventing parallel execution of test cases).

## SerialPortStream.Virtual is its own NuGet Package

For complete separation of concerns, this is a separate package to the
SerialPortStream. Import it in your packages only as you need it.

## Implementation and Usage

It is expected that your code has a factory for creating an object of type
`SerialPortStream`. This factory is important that production code would get an
instance that can talk to real serial ports, where test cases would get an
object that uses the `VirtualNativeSerial` instead.

### Not a Full Serial Implementation

When implementing your protocol, you should design and abstract the reading and
writing of data (at a byte level) from the protocol itself. This can make it
easier later to convert your implementation to a stand-alone protocol simulator
that can use real serial ports (or emulated drivers such as Com0Com).

The `VirtualNativeSerial` performs reads and writes atomically, where real
serial ports operate only at individual bytes. When implementing your protocol,
you should not assume you've received all data.

### Example on a Factory Pattern

```csharp
public interface ISerialPortFactory {
    SerialPortStream Create(string port);
}

public class SerialPortFactory : ISerialPortFactory {
    private static ISerialPortFactory _SerialPortFactory;

    public static ISerialPortFactory Instance {
        get {
            if (_SerialPortFactory == null) {
                _SerialPortFactory = new SerialPortFactory();
            }
            return _SerialPortFactory;
        }
        set {
            _SerialPortFactory = value;
        }
    }

    public SerialPortStream Create(string port) {
        return new SerialPortStream(port);
    }
}
```

The above code could be in your production software.

Then in your test case code you can assign it with a factory for your test
cases:

```csharp
public class VirtualSerialPortFactory : ISerialPortFactory {
    public Dictionary<string, VirtualNativeSerial> EndPoints = new Dictionary<string, VirtualNativeSerial>();

    public SerialPortStream Create(string port) {
        VirtualNativeSerial serial = new VirtualNativeSerial();;
        if (EndPoints.Contains(port)) {
            EndPoints[port] = serial;
        } else {
            EndPoints.Add(port, serial);
        }
        return new SerialPortStream(serial);
    }
}
```

And you can assign the factory, and use this to get the `VirtualNativeSerial`

```csharp
SerialPortFactory.Instance = new VirtualSerialPortFactory()
```

Safe usage to interact with the SerialPortStream should be done with the
`VirtualNativeSerial.VirtualBuffer` property.

### Getting Data from the User that was Written

You can read the data the user has requested to send to the serial port via
`VirtualBuffer.ReadSentData()`. It will copy the data that the user write with
API like `SerialPortStream.Write()` into a buffer. You can use this buffer to
know what the user wrote to implement a protocol.

The following properties are useful:

* `serial.VirtualBuffer.SentDataLength` contains the number of bytes that in the
  buffer which your test driver can read.
* `serial.VirtualBuffer.ReadSendData(byte[], int, int)` moves the data from the
  internal buffers to your own buffers, which you can use to implement your
  protocol.
* `serial.VirtualBuffer.WriteEvent` to be notified when the user has written
  data in the buffers. Within this event, you can set flags (or WaitHandles) to
  know that data has arrived when implementing your read/write thread as a
  `Task`.

### Responding and Sending the User Data

You can push data to the user that they can read it via the
`VirtualBuffer.WriteReceivedData()`. It will append the buffers given (as bytes)
to the byte buffer that the user can read with a call to
`SerialPortStream.Read()`.

The following properties are useful:

* `serial.VirtualBuffer.ReceivedDataFree` to know how much space is available to
  write in the buffer.
* `serial.VirtualBuffer.WriteReceivedData(byte[], int, int)` copies the data
  from the buffer to the serial port stream buffers that the user can read the
  data. The user can be notified of this data via the
  `SerialPortStream.DataReceived` event (slow), or they might have a continuous
  loop that just calls `SerialPortStream.Read()` which blocks until data
  arrives.
* `serial.VirtualBuffer.ReadEvent` to be notified when the user has read (and so
  data is removed) from the serial port stream buffer. This can let your
  protocol implementation know that more data can be written.

### Implementation Details

Assume that all methods and properties within the `VirtualNativeSerial` are
private except for those exposed by the property `VirtualBuffer`.

Implementation may change over time. Only the `VirtualBuffer` (i.e. the
interface `IVirtualSerialBuffer`) will try to be stable.
