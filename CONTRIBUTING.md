# Contributing

This is a private repository for trusted collaborators only.

## Build

```powershell
./tools/Generate-RecordingCue.ps1
./tools/Build-Flowtype.ps1
./tests/Run-Tests.ps1
```

## Package

```powershell
# Full offline installer (requires local Instant model file)
./tools/Package-Release.ps1 -Variant Full

# Lite installer (model downloaded on first setup)
./tools/Package-Release.ps1 -Variant Lite
```

## Guidelines

- Never commit API keys, user paths, recordings, logs, or model binaries to Git history
- Keep changes focused; Flowtype is intentionally a single-source-file WinForms app
- Run tests before opening a pull request
- Do not make the repository public without explicit owner approval
