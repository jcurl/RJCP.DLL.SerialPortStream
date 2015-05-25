@ECHO OFF

set MSBUILDDIR=%WINDIR%\Microsoft.NET\Framework\v4.0.30319
del /f /q "%CD%\distribute"

REM Note, that this project does not build for .NET 3.5. It uses features first present in .NET 4.0.

call:buildproj  4.0 v4.0    lib\net40   SerialPortStream  "code\SerialPortStream.csproj"
call:buildproj  4.0 v4.5    lib\net45   SerialPortStream  "code\SerialPortStream.csproj"

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
REM  %~1 - Tools Version. Should be 2.0, 3.5 or 4.0
REM  %~2 - Target Framework. Should be v4.0 or v4.5, etc.
REM  %~3 - Target folder.
REM  %~4 - Project Name. Used for log files only.
REM  %~5 - Project File.
:buildproj
echo.
echo ======================================================================
echo == Building %~4 for .NET %~2 (Tools v%~1)
echo ======================================================================
call %MSBUILDDIR%\MSBUILD.EXE %~5 /t:Rebuild /toolsversion:%~1 /verbosity:minimal /p:TargetFrameworkVersion=%~2 /p:DefineConstants=SIGNED_RELEASE /p:SignAssembly=true /p:Configuration=Release /p:OutputPath="%CD%\distribute\%~3" /fl /flp:verbosity=normal /nologo
copy msbuild.log "%CD%\distribute\msbuild-%~4-net-%~2-tools-%~1.log" > NUL
del msbuild.log
goto:eof
