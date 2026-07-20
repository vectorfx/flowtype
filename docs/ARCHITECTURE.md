# Architecture

Flowtype is a single-file C# WinForms application (`src/Flowtype.cs`) compiled on the target machine with `Add-Type` — no NuGet, no SDK project file.

## Runtime layout

| Path | Purpose |
|---|---|
| `%LOCALAPPDATA%\Flowtype\` | Application binaries, whisper runtime, models |
| `%APPDATA%\Flowtype\` | Settings, DPAPI keys, recovery audio, logs |

## Pipeline

```
GlobalKeyHook / chord poller
  → WaveRecorder (16 kHz mono WAV)
  → RecordingOverlay (layered, click-through meter)
  → WhisperEngine (warm whisper-server, CLI fallback)
  → TextProcessor or optional cloud cleanup
  → ForegroundContext paste or clipboard fallback
```

## Key components

- **GlobalKeyHook:** low-level keyboard hook; Win+Ctrl chord tracked without swallowing modifiers
- **WhisperEngine:** keeps `whisper-server.exe` warm with the selected ggml model
- **TextProcessor:** offline cleanup — fillers, backtracking, lists, spoken punctuation
- **ConfigStore:** JSON settings + DPAPI-encrypted provider keys

## Single instance

Mutex `Local\FlowtypeDesktop-9F33B64C`. Second launch signals `Local\FlowtypeDesktop-Activate-9F33B64C` to open Settings.

## Build outputs

`tools/Build-Flowtype.ps1` emits `Flowtype.exe` beside the source tree. Installers copy the package to `%LOCALAPPDATA%\Flowtype` and rebuild there.
