@echo off
if exist "%~dp0Flowtype.exe" (
  start "" "%~dp0Flowtype.exe"
) else (
  start "" powershell.exe -NoProfile -ExecutionPolicy Bypass -STA -WindowStyle Hidden -File "%~dp0Flowtype.ps1"
)
