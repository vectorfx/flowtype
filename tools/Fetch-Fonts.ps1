$ErrorActionPreference = 'Stop'
$fontDir = Join-Path $PSScriptRoot '..\assets\fonts'
New-Item -ItemType Directory -Force -Path $fontDir | Out-Null

$fonts = @(
    @{
        Name = 'RobotoMono-Regular.ttf'
        Url  = 'https://github.com/googlefonts/RobotoMono/raw/main/fonts/ttf/RobotoMono-Regular.ttf'
    },
    @{
        Name = 'RobotoMono-Bold.ttf'
        Url  = 'https://github.com/googlefonts/RobotoMono/raw/main/fonts/ttf/RobotoMono-Bold.ttf'
    }
)

foreach ($font in $fonts) {
    $target = Join-Path $fontDir $font.Name
    if (-not (Test-Path -LiteralPath $target) -or (Get-Item $target).Length -lt 50000) {
        curl.exe -fsSL -o $target $font.Url
    }
    Write-Host "Font ready: $target"
}
