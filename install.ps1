# Flowtype — one-line Windows installer
# Usage: irm https://raw.githubusercontent.com/vectorfx/flowtype/main/install.ps1 | iex

$ErrorActionPreference = 'Stop'

$tempRoot = $null
try {
    Write-Host 'Flowtype — fetching latest release...' -ForegroundColor Cyan

    $release = Invoke-RestMethod -Uri 'https://api.github.com/repos/vectorfx/flowtype/releases/latest' -Headers @{
        'User-Agent' = 'Flowtype-Installer'
    }
    $asset = $release.assets | Where-Object { $_.name -match '-Lite\.zip$' } | Select-Object -First 1
    if (-not $asset) { throw 'Lite release ZIP not found on GitHub.' }

    $tempRoot = Join-Path $env:TEMP ("Flowtype-install-" + [Guid]::NewGuid().ToString('N'))
    $zipPath = Join-Path $tempRoot 'flowtype.zip'
    $extractPath = Join-Path $tempRoot 'package'
    New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

    Write-Host "Downloading $($asset.name)..."
    Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $zipPath -UseBasicParsing

    Write-Host 'Extracting...'
    Expand-Archive -LiteralPath $zipPath -DestinationPath $extractPath -Force

    $installer = Get-ChildItem -LiteralPath $extractPath -Recurse -Filter 'Install-Flowtype.ps1' |
        Select-Object -First 1
    if (-not $installer) {
        throw 'Installer script missing from release package.'
    }

    Write-Host 'Installing to' $env:LOCALAPPDATA'\Flowtype...'
    & $installer.FullName -Silent

    Write-Host ''
    Write-Host 'Flowtype is ready.' -ForegroundColor Green
    Write-Host 'Hold Win + Ctrl, speak, release — text lands where you were typing.'
}
catch {
    Write-Host ''
    Write-Host ('Install failed: ' + $_.Exception.Message) -ForegroundColor Red
    exit 1
}
finally {
    if ($tempRoot -and (Test-Path -LiteralPath $tempRoot)) {
        Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
