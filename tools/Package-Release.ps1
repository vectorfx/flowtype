param(
    [ValidateSet('Lite', 'Full')]
    [string]$Variant = 'Lite'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$dist = Join-Path $root 'dist'
$version = '1.3.6'
$staging = Join-Path $dist "Flowtype-Windows-v$version-$Variant"

if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Force -Path $staging | Out-Null

$exclude = @('dist', '.git', '.github', 'tests', 'Flowtype.exe', 'packages')
Get-ChildItem -LiteralPath $root -Force | Where-Object {
    $_.Name -notin $exclude
} | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $staging -Recurse -Force
}

if ($Variant -eq 'Full') {
    $modelSource = Join-Path $root 'tools\whisper\models\ggml-base.en-q5_1.bin'
    if (-not (Test-Path -LiteralPath $modelSource)) {
        throw 'Full package requires tools\whisper\models\ggml-base.en-q5_1.bin on the build machine.'
    }
} else {
    $modelPath = Join-Path $staging 'tools\whisper\models\ggml-base.en-q5_1.bin'
    if (Test-Path -LiteralPath $modelPath) { Remove-Item -LiteralPath $modelPath -Force }
}

& (Join-Path $root 'tools\Build-Flowtype.ps1')
Copy-Item -LiteralPath (Join-Path $root 'Flowtype.exe') -Destination $staging -Force

Set-Content -LiteralPath (Join-Path $staging 'VERSION.txt') -Value "Flowtype $version ($Variant)" -Encoding UTF8
New-Item -ItemType Directory -Force -Path $dist | Out-Null
$zipPath = Join-Path $dist "Flowtype-Windows-v$version-$Variant.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -LiteralPath $staging -DestinationPath $zipPath -CompressionLevel Optimal
$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $zipPath).Hash.ToLowerInvariant()
$checksumPath = Join-Path $dist "Flowtype-Windows-v$version-$Variant.sha256"
Set-Content -LiteralPath $checksumPath -Value "$hash  $(Split-Path -Leaf $zipPath)" -Encoding ASCII
Write-Host "Created $zipPath"
Write-Host "SHA256 $hash"
