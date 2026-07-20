$ErrorActionPreference = 'Stop'
$outDir = Join-Path $PSScriptRoot '..\assets\audio'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function Write-Wav {
    param(
        [string]$Path,
        [double[]]$Samples,
        [double]$TargetPeak = 0.52
    )
    $sampleCount = $Samples.Length
    $peak = 0.0
    foreach ($sample in $Samples) {
        $abs = [Math]::Abs($sample)
        if ($abs -gt $peak) { $peak = $abs }
    }
    $gain = if ($peak -gt 0.0001) { $TargetPeak / $peak } else { 1.0 }

    $pcm = New-Object 'Int16[]' $sampleCount
    for ($i = 0; $i -lt $sampleCount; $i++) {
        $clamped = [Math]::Max(-1.0, [Math]::Min(1.0, $Samples[$i] * $gain))
        $pcm[$i] = [Int16]($clamped * 32767)
    }

    $sampleRate = 44100
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
    foreach ($sample in $pcm) { $writer.Write($sample) }
    [System.IO.File]::WriteAllBytes($Path, $stream.ToArray())
    Write-Host "Wrote $Path"
}

function New-LiquidSynth {
    param([int]$Seed = 17)
    $rng = [System.Random]::new($Seed)
    $brown = 0.0
    $low = 0.0
    $mid = 0.0

    return [pscustomobject]@{
        Next = {
            $white = ($rng.NextDouble() * 2.0 - 1.0)
            $brown = $brown * 0.992 + $white * 0.018
            $low = $low * 0.965 + $brown * 0.035
            $mid = $mid * 0.88 + $brown * 0.12
            return @{
                Deep = $low
                Body = ($low * 0.72) + ($mid * 0.28)
            }
        }.GetNewClosure()
    }
}

function Get-SoftEnvelope {
    param(
        [double]$t,
        [double]$Attack,
        [double]$Decay
    )
    $attackEnv = 1.0 - [Math]::Exp(-$t * $Attack)
    $decayEnv = [Math]::Exp(-$t * $Decay)
    return $attackEnv * $decayEnv
}

function Render-LiquidCue {
    param(
        [double]$Duration,
        [array]$Bursts,
        [int]$Seed,
        [double]$TailDecay = 18.0
    )
    $sampleRate = 44100
    $sampleCount = [int]($sampleRate * $Duration)
    $samples = New-Object 'double[]' $sampleCount
    $synth = New-LiquidSynth -Seed $Seed

    for ($i = 0; $i -lt $sampleCount; $i++) {
        $t = $i / [double]$sampleRate
        $noise = & $synth.Next
        $value = 0.0

        foreach ($burst in $Bursts) {
            $local = $t - [double]$burst.At
            if ($local -lt 0.0) { continue }
            $env = Get-SoftEnvelope $local $burst.Attack $burst.Decay
            $value += $noise.Body * $env * [double]$burst.Amp
            if ($burst.DeepAmp -gt 0.0) {
                $value += $noise.Deep * $env * [double]$burst.DeepAmp
            }
        }

        $samples[$i] = $value * [Math]::Exp(-$t * $TailDecay)
    }

    $fade = [Math]::Min(180, [Math]::Max(32, [int]($sampleCount * 0.02)))
    for ($i = 0; $i -lt $fade; $i++) {
        $ramp = ($i + 1) / [double]$fade
        $samples[$i] *= $ramp
        $samples[$sampleCount - 1 - $i] *= $ramp
    }

    return ,$samples
}

$startSamples = Render-LiquidCue -Duration 0.13 -Seed 41 -TailDecay 24.0 -Bursts @(
    @{ At = 0.000; Attack = 220.0; Decay = 34.0; Amp = 0.95; DeepAmp = 0.35 }
    @{ At = 0.018; Attack = 180.0; Decay = 42.0; Amp = 0.42; DeepAmp = 0.18 }
    @{ At = 0.034; Attack = 160.0; Decay = 48.0; Amp = 0.22; DeepAmp = 0.10 }
)

$completeSamples = Render-LiquidCue -Duration 0.19 -Seed 83 -TailDecay 15.0 -Bursts @(
    @{ At = 0.000; Attack = 190.0; Decay = 28.0; Amp = 0.88; DeepAmp = 0.42 }
    @{ At = 0.022; Attack = 150.0; Decay = 32.0; Amp = 0.55; DeepAmp = 0.24 }
    @{ At = 0.048; Attack = 130.0; Decay = 36.0; Amp = 0.38; DeepAmp = 0.16 }
    @{ At = 0.078; Attack = 120.0; Decay = 40.0; Amp = 0.24; DeepAmp = 0.10 }
    @{ At = 0.112; Attack = 110.0; Decay = 44.0; Amp = 0.14; DeepAmp = 0.06 }
)

Write-Wav -Path (Join-Path $outDir 'recording-start.wav') -Samples $startSamples -TargetPeak 0.48
Write-Wav -Path (Join-Path $outDir 'recording-complete.wav') -Samples $completeSamples -TargetPeak 0.50
