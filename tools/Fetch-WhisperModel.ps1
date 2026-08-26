$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$modelDir = Join-Path $root 'tools\whisper\models'
New-Item -ItemType Directory -Force -Path $modelDir | Out-Null
$target = Join-Path $modelDir 'ggml-base.en-q5_1.bin'
if ((Test-Path -LiteralPath $target) -and (Get-Item -LiteralPath $target).Length -ge 50000000) {
    Write-Host "Whisper model ready: $target"
    return
}

$urls = @(
    'https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.en-q5_1.bin?download=true',
    'https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.en-q5_1.bin',
    'https://hf.co/ggerganov/whisper.cpp/resolve/main/ggml-base.en-q5_1.bin?download=true'
)
$lastError = $null
foreach ($url in $urls) {
    try {
        Write-Host "Downloading Instant Whisper model…"
        curl.exe -fsSL -L --retry 3 -o $target $url
        if ((Test-Path -LiteralPath $target) -and (Get-Item -LiteralPath $target).Length -ge 50000000) {
            Write-Host "Whisper model ready: $target"
            return
        }
    }
    catch {
        $lastError = $_
        if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Force }
    }
}
if ($lastError) { throw $lastError }
throw 'Could not download ggml-base.en-q5_1.bin for the Full package.'
