[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$scriptPath = $MyInvocation.MyCommand.Path

if ([Threading.Thread]::CurrentThread.ApartmentState -ne 'STA') {
    Start-Process -FilePath 'powershell.exe' -WindowStyle Hidden -ArgumentList @(
        '-NoProfile', '-ExecutionPolicy', 'Bypass', '-STA', '-WindowStyle', 'Hidden',
        '-File', ('"{0}"' -f $scriptPath)
    )
    exit
}

try {
    $assemblyNames = @(
        'System',
        'System.Core',
        'System.Drawing',
        'System.Windows.Forms',
        'System.Net.Http',
        'System.Web.Extensions',
        'System.Security',
        'System.IO.Compression',
        'System.IO.Compression.FileSystem'
    )
    $references = foreach ($assemblyName in $assemblyNames) {
        Add-Type -AssemblyName $assemblyName -ErrorAction Stop
        $assembly = [AppDomain]::CurrentDomain.GetAssemblies() |
            Where-Object { $_.GetName().Name -eq $assemblyName } |
            Select-Object -First 1
        if ($null -eq $assembly -or [String]::IsNullOrWhiteSpace($assembly.Location)) {
            throw "Windows loaded $assemblyName but did not provide its compiler path."
        }
        $assembly.Location
    }
    $references = @($references | Select-Object -Unique)

    Add-Type -Path (Join-Path $PSScriptRoot 'src\Flowtype.cs') -ReferencedAssemblies $references

    [Flowtype.FlowtypeApp]::Run($PSScriptRoot)
}
catch {
    $root = Join-Path $env:APPDATA 'Flowtype'
    New-Item -ItemType Directory -Force -Path $root | Out-Null
    $message = "{0}`r`n{1}`r`n" -f (Get-Date -Format o), ($_ | Out-String)
    [IO.File]::AppendAllText((Join-Path $root 'startup-errors.log'), $message)
    Add-Type -AssemblyName System.Windows.Forms -ErrorAction SilentlyContinue
    [Windows.Forms.MessageBox]::Show(
        "Flowtype could not start. Details were written to:`r`n$root\startup-errors.log`r`n`r`n$($_.Exception.Message)",
        'Flowtype startup error',
        [Windows.Forms.MessageBoxButtons]::OK,
        [Windows.Forms.MessageBoxIcon]::Error
    ) | Out-Null
    exit 1
}
