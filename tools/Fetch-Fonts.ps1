$ErrorActionPreference = 'Stop'
$fontDir = Join-Path $PSScriptRoot '..\assets\fonts'
New-Item -ItemType Directory -Force -Path $fontDir | Out-Null

$fonts = @(
    @{
        Name = 'SourceCodePro-Regular.ttf'
        Url  = 'https://github.com/adobe-fonts/source-code-pro/raw/release/TTF/SourceCodePro-Regular.ttf'
    },
    @{
        Name = 'JetBrainsMono-Regular.ttf'
        Url  = 'https://github.com/JetBrains/JetBrainsMono/raw/master/fonts/ttf/JetBrainsMono-Regular.ttf'
    }
)

foreach ($font in $fonts) {
    $target = Join-Path $fontDir $font.Name
    if (-not (Test-Path -LiteralPath $target) -or (Get-Item $target).Length -lt 50000) {
        Invoke-WebRequest -Uri $font.Url -OutFile $target -UseBasicParsing
    }
    Write-Host "Font ready: $target"
}
