$ErrorActionPreference = 'Stop'
$fontDir = Join-Path $PSScriptRoot '..\assets\fonts'
New-Item -ItemType Directory -Force -Path $fontDir | Out-Null
$target = Join-Path $fontDir 'JetBrainsMono-Regular.ttf'
if (-not (Test-Path -LiteralPath $target) -or (Get-Item $target).Length -lt 100000) {
    Invoke-WebRequest -Uri 'https://github.com/JetBrains/JetBrainsMono/raw/master/fonts/ttf/JetBrainsMono-Regular.ttf' `
        -OutFile $target -UseBasicParsing
}
Write-Host "Font ready at $target"
