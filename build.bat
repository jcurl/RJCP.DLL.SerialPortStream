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
echo == Testing SerialPortStream on .NET Core App 3.1
echo ======================================================================
dotnet test -c Release -f netcoreapp3.1 --logger "trx"

echo.
echo ======================================================================
echo == Generating SerialPortStream NuGet Package
echo ======================================================================
dotnet pack -c Release .\code\SerialPortStream.csproj

pause