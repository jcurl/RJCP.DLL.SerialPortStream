@ECHO OFF

echo.
echo ======================================================================
echo == Building SerialPortStream
echo ======================================================================
dotnet build -c Release

echo.
echo ======================================================================
echo == Testing SerialPortStream on .NET 4.0
echo ======================================================================
dotnet test -c Release -f net40 --logger "trx"

echo.
echo ======================================================================
echo == Testing SerialPortStream on .NET 4.5
echo ======================================================================
dotnet test -c Release -f net45 --logger "trx"

echo.
echo ======================================================================
echo == Testing SerialPortStream on .NET 6.0
echo ======================================================================
dotnet test -c Release -f net6.0 --logger "trx"

echo.
echo ======================================================================
echo == Testing SerialPortStream on .NET 8.0
echo ======================================================================
dotnet test -c Release -f net8.0 --logger "trx"

echo.
echo ======================================================================
echo == Generating Signed SerialPortStream NuGet Package
echo ======================================================================
dotnet build -c Signed_Release .\code\SerialPortStream.csproj
dotnet pack -c Signed_Release --include-symbols --include-source .\code\SerialPortStream.csproj

pause