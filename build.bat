@ECHO OFF

set "MSBUILDVER=Current"
reg Query "HKLM\Hardware\Description\System\CentralProcessor\0" | find /i "x86" > NUL && set OS=32BIT || set OS=64BIT
if %OS%==32BIT goto :OS32VARS
if %OS%==64BIT goto :OS64VARS

:OS32VARS
set "MSBUILDPREFIX=C:\Program Files\Microsoft Visual Studio\2019"
goto :BUILDVARS

:OS64VARS
set "MSBUILDPREFIX=C:\Program Files (x86)\Microsoft Visual Studio\2019"
goto :BUILDVARS

:BUILDVARS
if exist "%MSBUILDPREFIX%\BuildTools\MSBuild\%MSBUILDVER%\Bin\msbuild.exe" (
  set "MSBUILD=%MSBUILDPREFIX%\BuildTools\MSBuild\%MSBUILDVER%\Bin\msbuild.exe"
)
if exist "%MSBUILDPREFIX%\Community\MSBuild\%MSBUILDVER%\bin\msbuild.exe" (
  set "MSBUILD=%MSBUILDPREFIX%\Community\MSBuild\%MSBUILDVER%\bin\msbuild.exe"
)
if exist "%MSBUILDPREFIX%\Professional\MSBuild\%MSBUILDVER%\bin\msbuild.exe" (
  set "MSBUILD=%MSBUILDPREFIX%\Professional\MSBuild\%MSBUILDVER%\bin\msbuild.exe"
)
if exist "%MSBUILDPREFIX%\Enterprise\MSBuild\%MSBUILDVER%\bin\msbuild.exe" (
  set "MSBUILD=%MSBUILDPREFIX%\Enterprise\MSBuild\%MSBUILDVER%\bin\msbuild.exe"
)

:BUILD
del /f /s /q "%CD%\distribute" > nul
del /f /s /q code\bin > nul 2> nul
del /f /s /q code\obj > nul 2> nul

REM Note, that this project does not build for .NET 3.5. It uses features first present in .NET 4.0.

nuget restore SerialPortStream-net40.sln
call:buildproj net40 lib\net40 SerialPortStream "code\SerialPortStream-net40.csproj"

nuget restore SerialPortStream-net45.sln
call:buildproj net45 lib\net45 SerialPortStream "code\SerialPortStream-net45.csproj"

echo.
echo ======================================================================
echo == Building SerialPortStream for netstandard1.5
echo ======================================================================
dotnet restore SerialPortStream-netstandard15.sln
dotnet build SerialPortStream-netstandard15.sln --configuration Signed_Release --output "%CD%\distribute\lib\netstandard1.5"
del /f /s /q code\bin > nul
del /f /s /q code\obj > nul

echo.
echo ======================================================================
echo == Creating NuGet package
echo ======================================================================
nuget pack code\SerialPortStream.nuspec -symbols

echo.
pause
goto:eof

REM BuildProj
REM  Builds the project for release on NuGet.
REM
REM  Builds for a specific target framework. The resulting binary is signed with the PFX file defined in the
REM  project. By default, a project has signing disabled, so that users can simply clone the repository and
REM  build. Note, the private key is not part of the repository.
REM
REM  The constant SIGNED_RELEASE is used to indicate that this assembly is for distribution, and so "friend"
REM  assemblies are not allowed (part of the AssemblyInfo.cs).
REM 
REM Parameters
REM  %~1 - Target Framework. Used for log files only.
REM  %~2 - Target folder.
REM  %~3 - Project Name. Used for log files only.
REM  %~4 - Project File.
:buildproj
echo.
echo ======================================================================
echo == Building %~3 for %~1
echo ======================================================================
call "%MSBUILD%" %~4 /t:Rebuild /verbosity:minimal /p:SignAssembly=true /p:Configuration=Signed_Release /p:OutputPath="%CD%\distribute\%~2" /fl /flp:verbosity=normal /nologo
copy msbuild.log "%CD%\distribute\msbuild-%~3-net-%~1.log" > NUL
del msbuild.log
del /f /s /q code\bin > nul
del /f /s /q code\obj > nul
goto:eof
