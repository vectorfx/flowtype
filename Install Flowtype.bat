@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -STA -File "%~dp0installer\Install-Flowtype.ps1"
if errorlevel 1 (
  echo.
  echo Flowtype could not be installed. See the message above.
  pause
)

