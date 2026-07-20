$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root 'src\Flowtype.cs'
$embedded = Join-Path $root 'src\EmbeddedAudio.g.cs'
$tests = Join-Path $PSScriptRoot 'Flowtype.Tests.cs'
$outputDir = Join-Path $PSScriptRoot 'output'
$dll = Join-Path $outputDir 'Flowtype.Tests.dll'

if (-not (Test-Path -LiteralPath $embedded)) {
    & (Join-Path $root 'tools\Embed-Audio.ps1')
}

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

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
$csc = Join-Path $framework 'csc.exe'
$appRefNames = @(
    'System.dll', 'System.Core.dll', 'System.Drawing.dll', 'System.Windows.Forms.dll',
    'System.Net.Http.dll', 'System.Web.Extensions.dll', 'System.IO.Compression.dll',
    'System.IO.Compression.FileSystem.dll', 'System.Security.dll'
)
$appRefArgs = foreach ($assemblyName in $appRefNames) {
    "/reference:$(Resolve-FrameworkAssembly $assemblyName)"
}
$appDll = Join-Path $outputDir 'Flowtype.App.dll'
& $csc /nologo /target:library /out:$appDll @appRefArgs $source $embedded
if ($LASTEXITCODE -ne 0) { throw "Failed to compile Flowtype app sources." }

$testRefArgs = @($appRefArgs) + @("/reference:$appDll")
& $csc /nologo /target:library /out:$dll @testRefArgs $tests
if ($LASTEXITCODE -ne 0) { throw "Failed to compile Flowtype tests." }

Add-Type -Path $dll
$runner = [Flowtype.Tests.TestRunner]::new()
$failures = $runner.RunAll()
if ($failures -gt 0) {
    Write-Error "$failures test(s) failed."
    exit 1
}
Write-Host "All tests passed."
