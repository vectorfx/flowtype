# Changelog

## 1.3.18

- Fix voice capsule stuck on rapid-fire dictation (overlay session tracking + idle cleanup)
- Fix rare recording crash on loud audio peaks (`Math.Abs` overflow on 16-bit samples)

---

## 1.3.17

- Voice capsule enter/exit animation — quick fade + slide, no focus steal
- One-line PowerShell install: `irm …/install.ps1 | iex`
- README sharpened — sauce without the recipe

---

## 1.3.16

**Recommended release.** Consolidates polish since 1.3.6 into one install.

### Dictation reliability
- Rapid repeat dictation no longer drops the 3rd+ take (removed re-arm cooldown, mic retry, cleaner hotkey sync)

### Voice capsule
- Compact centred pill with matte **Dark** default theme (near-black + zinc borders)
- **Dark purple**, **Light**, **Mono**, and **Liquid glass** themes
- Liquid glass captures and blurs the desktop behind the pill

### Audio & UI
- Custom recording cues embedded in the executable (no stale disk files)
- Roboto Mono UI font
- Settings refresh: logo header, Save & close / Apply / Cancel, clearer labels
- Latency strip, mic health meter, 3-second mic test, tray dictionary shortcut

### Engines & privacy
- Local Instant model only offline (~60 MB); use Groq for higher accuracy
- Groq engine with startup prewarm and reused HTTP client
- Silent-by-default toasts; optional sound effects and insert notifications
- Built-in cleanup default; cloud polish optional

### Install & build
- Icon embedded in exe; startup error dialog on failure
- `tools/Embed-Audio.ps1`, `tools/Extract-RecordingCue.ps1`, `Fetch-Fonts.ps1` for reproducible builds

---

## 1.3.6 and earlier

See git history for incremental 1.3.0–1.3.6 release notes. Those tags remain on GitHub for reference; **use 1.3.18** for new installs.
