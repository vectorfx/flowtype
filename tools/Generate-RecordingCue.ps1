$ErrorActionPreference = 'Stop'
$outDir = Join-Path $PSScriptRoot '..\assets\audio'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function Write-Wav {
    param(
        [string]$Path,
        [double]$Duration,
        [scriptblock]$SampleGenerator
    )
    $sampleRate = 44100
    $sampleCount = [int]($sampleRate * $Duration)
    $samples = New-Object 'Int16[]' $sampleCount
    for ($i = 0; $i -lt $sampleCount; $i++) {
        $t = $i / [double]$sampleRate
        $value = & $SampleGenerator $t
        $clamped = [Math]::Max(-1.0, [Math]::Min(1.0, $value))
        $samples[$i] = [Int16]($clamped * 32767)
    }
    $stream = [System.IO.MemoryStream]::new()
    $writer = [System.IO.BinaryWriter]::new($stream)
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('RIFF'))
    $dataSize = 36 + ($sampleCount * 2)
    $writer.Write([int]($dataSize - 8))
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('WAVEfmt '))
    $writer.Write([int]16)
    $writer.Write([Int16]1)
    $writer.Write([Int16]1)
    $writer.Write([int]$sampleRate)
    $writer.Write([int]($sampleRate * 2))
    $writer.Write([Int16]2)
    $writer.Write([Int16]16)
    $writer.Write([System.Text.Encoding]::ASCII.GetBytes('data'))
    $writer.Write([int]($sampleCount * 2))
    foreach ($sample in $samples) { $writer.Write($sample) }
    [System.IO.File]::WriteAllBytes($Path, $stream.ToArray())
    Write-Host "Wrote $Path"
}

$startPath = Join-Path $outDir 'recording-start.wav'
Write-Wav -Path $startPath -Duration 0.16 -SampleGenerator {
    param($t)
    $attack = 1.0 - [Math]::Exp(-$t * 140.0)
    $decay = [Math]::Exp(-$t * 12.0)
    $envelope = $attack * $decay
    $freq = 380.0 + 90.0 * [Math]::Sin($t * 18.0)
    $body = [Math]::Sin(2.0 * [Math]::PI * $freq * $t)
    $harm = 0.18 * [Math]::Sin(2.0 * [Math]::PI * ($freq * 2.03) * $t)
    ($body + $harm) * $envelope * 0.34
}

$completePath = Join-Path $outDir 'recording-complete.wav'
Write-Wav -Path $completePath -Duration 0.24 -SampleGenerator {
    param($t)
    $attack = 1.0 - [Math]::Exp(-$t * 160.0)
    $decay = [Math]::Exp(-$t * 10.5)
    $envelope = $attack * $decay
    $freq = 720.0 - 420.0 * $t + 55.0 * [Math]::Sin($t * 28.0)
    $tone = [Math]::Sin(2.0 * [Math]::PI * $freq * $t)
    $splash = 0.12 * [Math]::Sin(2.0 * [Math]::PI * ($freq * 1.7) * $t + 0.4)
    $click = if ($t -lt 0.012) { [Math]::Sin(2.0 * [Math]::PI * 1800.0 * $t) * (1.0 - ($t / 0.012)) } else { 0.0 }
    ($tone + $splash + $click) * $envelope * 0.42
}
