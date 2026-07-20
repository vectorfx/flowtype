$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms

$answer = [Windows.Forms.MessageBox]::Show(
    "First quit Flowtype from the tray icon.`r`n`r`nRemove the installed app now? Your settings and history will be kept.",
    'Uninstall Flowtype',
    [Windows.Forms.MessageBoxButtons]::YesNo,
    [Windows.Forms.MessageBoxIcon]::Question
)
if ($answer -ne [Windows.Forms.DialogResult]::Yes) { exit }

try {
    Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'Flowtype' -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath (Join-Path ([Environment]::GetFolderPath('Desktop')) 'Flowtype.lnk') -Force -ErrorAction SilentlyContinue
    $startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
    Remove-Item -LiteralPath (Join-Path $startMenu 'Flowtype.lnk') -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath (Join-Path $startMenu 'Uninstall Flowtype.lnk') -Force -ErrorAction SilentlyContinue

    $destination = Join-Path $env:LOCALAPPDATA 'Flowtype'
    $cleanup = Join-Path $env:TEMP ('remove-flowtype-{0}.ps1' -f [Guid]::NewGuid().ToString('N'))
    @(
        'Start-Sleep -Seconds 2',
        ('Remove-Item -LiteralPath ''{0}'' -Recurse -Force -ErrorAction SilentlyContinue' -f ($destination -replace "'", "''")),
        'Remove-Item -LiteralPath $MyInvocation.MyCommand.Path -Force -ErrorAction SilentlyContinue'
    ) | Set-Content -LiteralPath $cleanup -Encoding UTF8
    Start-Process powershell.exe -WindowStyle Hidden -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', ('"{0}"' -f $cleanup))

    [Windows.Forms.MessageBox]::Show(
        "Flowtype was removed. Local data remains in:`r`n%APPDATA%\Flowtype`r`n`r`nDelete that folder manually if you also want to erase settings, history, and recovery audio.",
        'Flowtype removed',
        [Windows.Forms.MessageBoxButtons]::OK,
        [Windows.Forms.MessageBoxIcon]::Information
    ) | Out-Null
}
catch {
    [Windows.Forms.MessageBox]::Show($_.Exception.Message, 'Flowtype uninstall failed', 'OK', 'Error') | Out-Null
    exit 1
}
