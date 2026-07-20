$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root 'src\Flowtype.cs'
$output = Join-Path $root 'Flowtype.exe'
$icon = Join-Path $root 'assets\Flowtype.ico'

if (-not (Test-Path -LiteralPath $source)) {
    throw "Missing source file: $source"
}

$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
if (-not (Test-Path -LiteralPath $framework)) {
    $framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319'
}

function Resolve-FrameworkAssembly {
    param([string]$Name)
    $direct = Join-Path $framework $Name
    if (Test-Path -LiteralPath $direct) { return $direct }
    $gacRoot = Join-Path $env:WINDIR 'Microsoft.NET\assembly\GAC_MSIL'
    $folderName = [IO.Path]::GetFileNameWithoutExtension($Name)
    $matches = Get-ChildItem -Path (Join-Path $gacRoot $folderName) -Recurse -Filter $Name -ErrorAction SilentlyContinue
    if ($matches) { return $matches[0].FullName }
    throw "Could not resolve assembly: $Name"
}

$assemblyNames = @(
    'System.dll',
    'System.Core.dll',
    'System.Drawing.dll',
    'System.Windows.Forms.dll',
    'System.Net.Http.dll',
    'System.Web.Extensions.dll',
    'System.IO.Compression.dll',
    'System.IO.Compression.FileSystem.dll',
    'System.Security.dll'
)

$refArgs = foreach ($assemblyName in $assemblyNames) {
    "/reference:$(Resolve-FrameworkAssembly $assemblyName)"
}

$csc = Join-Path $framework 'csc.exe'
if (-not (Test-Path -LiteralPath $csc)) {
    throw 'Could not find csc.exe. Install .NET Framework 4.x developer tools.'
}

$iconArgs = @()
if (Test-Path -LiteralPath $icon) {
    $iconArgs = @("/win32icon:$icon")
} else {
    Write-Warning "Missing $icon - run: python assets/build_icon.py"
}

& $csc /nologo /target:winexe /out:$output @iconArgs @refArgs $source
if ($LASTEXITCODE -ne 0) {
    throw "csc.exe failed with exit code $LASTEXITCODE"
}

$version = [Diagnostics.FileVersionInfo]::GetVersionInfo($output).FileVersion
Write-Host "Built Flowtype $version at $output"
