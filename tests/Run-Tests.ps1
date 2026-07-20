$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root 'src\Flowtype.cs'
$tests = Join-Path $PSScriptRoot 'Flowtype.Tests.cs'
$outputDir = Join-Path $PSScriptRoot 'output'
$dll = Join-Path $outputDir 'Flowtype.Tests.dll'

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

$assemblyNames = @(
    'System',
    'System.Core',
    'System.Drawing',
    'System.Windows.Forms',
    'System.Net.Http',
    'System.Web.Extensions',
    'System.IO.Compression',
    'System.IO.Compression.FileSystem',
    'System.Security'
)

$references = foreach ($assemblyName in $assemblyNames) {
    Add-Type -AssemblyName $assemblyName -ErrorAction Stop
    $assembly = [AppDomain]::CurrentDomain.GetAssemblies() |
        Where-Object { $_.GetName().Name -eq $assemblyName } |
        Select-Object -First 1
    $assembly.Location
}
$references = @($references | Select-Object -Unique)

Add-Type -TypeDefinition (Get-Content -Raw -LiteralPath $source) -ReferencedAssemblies $references -OutputAssembly (Join-Path $outputDir 'Flowtype.App.dll') -OutputType Library
Add-Type -TypeDefinition (Get-Content -Raw -LiteralPath $tests) -ReferencedAssemblies ($references + (Join-Path $outputDir 'Flowtype.App.dll')) -OutputAssembly $dll -OutputType Library

Add-Type -Path $dll
$runner = [Flowtype.Tests.TestRunner]::new()
$failures = $runner.RunAll()
if ($failures -gt 0) {
    Write-Error "$failures test(s) failed."
    exit 1
}
Write-Host "All tests passed."
