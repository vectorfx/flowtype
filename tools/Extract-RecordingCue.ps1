param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,
    [string]$OutputDir = (Join-Path $PSScriptRoot '..\assets\audio'),
    [double]$SilenceThreshold = 0.05,
    [double]$GapSeconds = 0.03,
    [double]$PadBefore = 0.02,
    [double]$PadAfter = 0.12
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $InputPath)) {
    throw "Input not found: $InputPath"
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$tempWav = Join-Path $env:TEMP ("flowtype-source-{0}.wav" -f [Guid]::NewGuid().ToString('N'))

ffmpeg -y -i $InputPath -vn -ac 1 -ar 44100 $tempWav | Out-Null

$analysisScript = @'
import json, math, struct, sys, wave
path = sys.argv[1]
threshold = float(sys.argv[2])
gap = float(sys.argv[3])
pad_before = float(sys.argv[4])
pad_after = float(sys.argv[5])
with wave.open(path, 'rb') as w:
    sr = w.getframerate()
    samples = struct.unpack('<' + 'h' * w.getnframes(), w.readframes(w.getnframes()))
duration = len(samples) / sr
peak = max(abs(s) for s in samples) or 1
cutoff = peak * threshold
window = max(1, int(sr * 0.005))
hits = []
for i in range(0, len(samples) - window, window):
    if max(abs(s) for s in samples[i:i + window]) >= cutoff:
        hits.append(i / sr)
segments = []
if hits:
    start = hits[0]
    prev = hits[0]
    for t in hits[1:]:
        if t - prev > gap:
            segments.append([max(0.0, start - pad_before), min(duration, prev + pad_after)])
            start = t
        prev = t
    segments.append([max(0.0, start - pad_before), min(duration, prev + pad_after)])
print(json.dumps(segments))
'@

$analysisPath = Join-Path $env:TEMP 'flowtype-analyze-cues.py'
Set-Content -LiteralPath $analysisPath -Value $analysisScript -Encoding UTF8
$segmentsJson = python $analysisPath $tempWav $SilenceThreshold $GapSeconds $PadBefore $PadAfter
$segments = $segmentsJson | ConvertFrom-Json

if (-not $segments -or $segments.Count -lt 1) {
    throw 'No audible cue segments were found in the recording.'
}

$names = @('recording-start.wav', 'recording-complete.wav')
for ($index = 0; $index -lt [Math]::Min(2, $segments.Count); $index++) {
    $segment = $segments[$index]
    $start = [double]$segment[0]
    $end = [double]$segment[1]
    $target = Join-Path $OutputDir $names[$index]
    ffmpeg -y -ss $start -to $end -i $tempWav -af "afade=t=in:st=0:d=0.004,afade=t=out:st=$([Math]::Max(0.0, ($end - $start - 0.015))):d=0.015" $target | Out-Null
    Write-Host "Wrote $target ($('{0:0.000}' -f $start)-$('{0:0.000}' -f $end)s)"
}

Remove-Item -LiteralPath $tempWav -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $analysisPath -Force -ErrorAction SilentlyContinue

if ($segments.Count -lt 2) {
    Write-Warning 'Only one cue segment was found; recording-complete.wav was not updated.'
}
