$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$source = Join-Path $root 'src\Flowtype.cs'
$output = Join-Path $root 'Flowtype.exe'

if (-not (Test-Path -LiteralPath $source)) {
    throw "Missing source file: $source"
}

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
    if ($null -eq $assembly -or [String]::IsNullOrWhiteSpace($assembly.Location)) {
        throw "Windows loaded $assemblyName but did not provide its compiler path."
    }
    $assembly.Location
}

$references = @($references | Select-Object -Unique)
Add-Type -Path $source -ReferencedAssemblies $references -OutputAssembly $output -OutputType WindowsApplication

$version = [Diagnostics.FileVersionInfo]::GetVersionInfo($output).FileVersion
Write-Host "Built Flowtype $version at $output"
