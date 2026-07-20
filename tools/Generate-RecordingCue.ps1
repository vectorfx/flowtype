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
Write-Wav -Path $startPath -Duration 0.14 -SampleGenerator {
    param($t)
    $attack = 1.0 - [Math]::Exp(-$t * 160.0)
    $decay = [Math]::Exp(-$t * 11.0)
    $envelope = $attack * $decay
    $freq = 360.0 + 70.0 * [Math]::Sin($t * 20.0)
    $body = [Math]::Sin(2.0 * [Math]::PI * $freq * $t)
    $harm = 0.16 * [Math]::Sin(2.0 * [Math]::PI * ($freq * 2.1) * $t)
    ($body + $harm) * $envelope * 0.55
}

$completePath = Join-Path $outDir 'recording-complete.wav'
Write-Wav -Path $completePath -Duration 0.22 -SampleGenerator {
    param($t)
    $attack = 1.0 - [Math]::Exp(-$t * 180.0)
    $decay = [Math]::Exp(-$t * 9.0)
    $envelope = $attack * $decay
    $freq = 640.0 - 380.0 * $t + 40.0 * [Math]::Sin($t * 32.0)
    $tone = [Math]::Sin(2.0 * [Math]::PI * $freq * $t)
    $splash = 0.14 * [Math]::Sin(2.0 * [Math]::PI * ($freq * 1.65) * $t + 0.5)
    $click = if ($t -lt 0.010) { [Math]::Sin(2.0 * [Math]::PI * 2200.0 * $t) * (1.0 - ($t / 0.010)) } else { 0.0 }
    ($tone + $splash + $click) * $envelope * 0.62
}
