# Changelog

## 1.3.24

- Fix "final" becoming a Discord contact name (e.g. PinBal) during chat dictation
- Stop using chat app window titles (Discord, Slack, Teams, etc.) for fuzzy spell correction
- Require first-letter match for fuzzy corrections; protect common words like "final"
- Keep bare dictionary names out of the Whisper prompt so contact names do not bias transcription

---

## 1.3.23

- Add hands-free mode: double-press push-to-talk key to dictate without holding
- Expand hotkey choices (Win+Alt/Shift, Ctrl+Shift, left/right modifiers, Scroll Lock, Pause, F8–F12)
- Fix hands-free stop getting stuck when using modifier chords like Win+Ctrl
- Fix "not" becoming "t" when Whisper splits the word across syllables
- Fix settings UI overlap on the hands-free checkbox row

---

## 1.3.22

- Fix mic tester showing the same level at every boost setting (now shows mic, boosted, and Whisper input)
- Strip lone-letter Whisper glitches and mid-transcript prompt echo from longer dictation
- Tighten list detection so normal prose is not formatted as bullet lists
- Switch UI font to Space Grotesk (sharp tech sans)

---

## 1.3.21

- Fix Whisper echoing "Target window" prompt garbage into dictation output
- Fix Win+Ctrl capsule not appearing when key release beat async recording start
- Stop carrying STT prompt across entire decode (major hallucination trigger)

---

## 1.3.20

- Fix loud speech failing or garbling (double mic-gain bug + soft limiter instead of hard clipping)
- Strip Whisper repetition loops from longer dictations (duplicate sentences and trailing phrase echoes)

---

## 1.3.19

- Fix truncated recordings producing single-letter output (immediate mic start, 180 ms release tail, stable chord poller)
- Reject garbled Whisper hallucinations instead of inserting them
- Fuzzy spell correction from dictionary, snippets, and active window title

---

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
