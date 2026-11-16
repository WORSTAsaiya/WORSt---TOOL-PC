@echo off
REM ===== WORST - Account Manager : build =====
REM Needs the .NET 8 SDK (one-time): https://dotnet.microsoft.com/download/dotnet/8.0
REM Then just double-click this file. Output: publish\WORST.exe (single file, self-contained).

where dotnet >nul 2>nul
if errorlevel 1 (
  echo.
  echo  .NET 8 SDK not found.
  echo  Install it from https://dotnet.microsoft.com/download/dotnet/8.0  then run this again.
  echo.
  pause
  exit /b 1
)

dotnet publish WORST.csproj -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true ^
  -o publish

echo.
if exist publish\WORST.exe (
  echo  Done.  Run:  publish\WORST.exe
) else (
  echo  Build failed - read the messages above.
)
echo.
pause
