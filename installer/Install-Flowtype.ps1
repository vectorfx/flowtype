$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms

try {
    $source = Split-Path -Parent $PSScriptRoot
    $destination = Join-Path $env:LOCALAPPDATA 'Flowtype'
    $startMenu = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs'
    $desktop = [Environment]::GetFolderPath('Desktop')

    if (-not (Test-Path -LiteralPath (Join-Path $source 'src\Flowtype.cs'))) {
        throw 'Extract the entire Flowtype ZIP to a normal folder before running Install Flowtype.bat.'
    }

    # Stop running instances and whisper workers so locked DLLs can be replaced.
    Get-Process -Name 'Flowtype' -ErrorAction SilentlyContinue |
        Stop-Process -Force -ErrorAction SilentlyContinue
    Get-Process -Name 'whisper-server','whisper-cli' -ErrorAction SilentlyContinue |
        Stop-Process -Force -ErrorAction SilentlyContinue
    $legacyHosts = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object {
        $_.ProcessId -ne $PID -and
        $_.Name -match '^(powershell|pwsh)(\.exe)?$' -and
        $_.CommandLine -match '(?i)[\\/]Flowtype\.ps1'
    }
    foreach ($legacyHost in $legacyHosts) {
        Stop-Process -Id $legacyHost.ProcessId -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Milliseconds 900

    New-Item -ItemType Directory -Force -Path $destination | Out-Null
    Get-ChildItem -LiteralPath $source -Force | Where-Object {
        $_.Name -notin @('flowtype-desktop.zip')
    } | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $destination -Recurse -Force
    }

    Get-ChildItem -LiteralPath $destination -Recurse -File | Unblock-File -ErrorAction SilentlyContinue

    $outputExe = Join-Path $destination 'Flowtype.exe'
    & (Join-Path $destination 'tools\Build-Flowtype.ps1')
    if (-not (Test-Path -LiteralPath $outputExe)) {
        throw 'Flowtype.exe is missing.'
    }
    $installedVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($outputExe).FileVersion
    if ($installedVersion -notmatch '^1\.3\.2\.') {
        throw "Expected Flowtype 1.3.2.x but found $installedVersion. Extract the new ZIP into an empty folder and retry."
    }

    $shell = New-Object -ComObject WScript.Shell
    $target = $outputExe
    $arguments = ''

    foreach ($shortcutPath in @(
        (Join-Path $desktop 'Flowtype.lnk'),
        (Join-Path $startMenu 'Flowtype.lnk')
    )) {
        $shortcut = $shell.CreateShortcut($shortcutPath)
        $shortcut.TargetPath = $target
        $shortcut.Arguments = $arguments
        $shortcut.WorkingDirectory = $destination
        $shortcut.IconLocation = "$outputExe,0"
        $shortcut.Description = 'System-wide push-to-talk dictation'
        $shortcut.Save()
    }

    $uninstallShortcut = $shell.CreateShortcut((Join-Path $startMenu 'Uninstall Flowtype.lnk'))
    $uninstallShortcut.TargetPath = (Join-Path $destination 'Uninstall Flowtype.bat')
    $uninstallShortcut.WorkingDirectory = $destination
    $uninstallShortcut.IconLocation = "$env:SystemRoot\System32\imageres.dll,84"
    $uninstallShortcut.Save()

    $runKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
    New-ItemProperty -Path $runKey -Name 'Flowtype' -PropertyType String -Value ('"{0}"' -f $target) -Force | Out-Null

    $newProcess = Start-Process -FilePath $target -PassThru
    Start-Sleep -Milliseconds 1200
    $newProcess.Refresh()
    if ($newProcess.HasExited) {
        throw 'The new Flowtype process exited during startup. The old copy may still be running; restart Windows once, then run this installer again.'
    }

    [Windows.Forms.MessageBox]::Show(
        "Flowtype 1.3.2 is installed and the new executable is running.`r`n`r`nHold Win + Ctrl together to dictate anywhere.",
        'Flowtype 1.3.2 installed',
        [Windows.Forms.MessageBoxButtons]::OK,
        [Windows.Forms.MessageBoxIcon]::Information
    ) | Out-Null
}
catch {
    Add-Type -AssemblyName System.Windows.Forms -ErrorAction SilentlyContinue
    [Windows.Forms.MessageBox]::Show($_.Exception.Message, 'Flowtype install failed', 'OK', 'Error') | Out-Null
    exit 1
}
