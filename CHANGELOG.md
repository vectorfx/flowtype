# Version history

## 1.3.3

- Fix startup crash (recorder initialized before use)
- Embed Flowtype icon in the executable and tray
- Show an error dialog if startup fails

## 1.3.2

- Removed Flow Quality (~574 MB) local model option — Instant only offline; use Groq for cloud accuracy
- Settings and upgrade path auto-migrate away from legacy large models
- Simplified Local settings tab
- Fixed latency strip record timing
- Microphone health: live level meter + 3-second test in Settings
- Tray shortcut to fix the last dictated word in the dictionary

## 1.3.1

- Turbo transcription path for faster long dictations
- Software microphone boost / AGC
- Groq cloud engine (free API tier, whisper-large-v3-turbo)
- Completion sound + inserted preview toast
- Latency diagnostics in Settings
- Non-blocking dictation while previous result processes

## 1.3.0

- Production release packaging (Full + Lite installers)
- Hotkey debounce fixes Win+Ctrl double-tap overlay glitch
- Stronger live microphone meter on recording capsule
- Subtle recording-start audio cue
- Inter font in Settings (Segoe UI fallback)
- Model quality comparison in Local settings
- CI tests and GitHub Actions workflows
- Upgrade-safe installer stops whisper workers before file copy

## 1.2.0

- Layered click-through voice capsule overlay
- Overlay hides immediately on key release
- Second instance opens Settings instead of blocking

## 1.1.9

- Initial full Windows package with bundled whisper.cpp runtime and Instant model
