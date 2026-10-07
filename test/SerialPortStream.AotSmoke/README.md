# README

This project is able to compile `RJCP.SerialPortStream` v3.0.5 and later as an
AOT (Ahead of Time), i.e. natively compiled, executable. Tested on Windows.

To build the test (to show that AOT works)

```txt
dotnet publish -c release
```

To use, check out the repository and its dependencies into your project and
build.

## Windows

Under Windows, the output, with no DLLs published, is at:
`bin\Release\net10.0\win-x64\publish`.

You'll see that there are no DLLs, just a single EXE.
