<p align="center">
  <img src="assets/Flowtype-icon.png" alt="Flowtype" width="96">
</p>

<h1 align="center">Flowtype</h1>

<p align="center">
  <strong>Hold a key. Speak. Release. Clean text appears where you were typing.</strong>
</p>

<p align="center">
  System-wide push-to-talk for Windows — built to feel instant, stay out of your way,<br>
  and work in every app without an account, subscription, or 600 MB download.
</p>

<p align="center">
  <a href="https://github.com/vectorfx/flowtype/releases/latest"><strong>Download v1.3.16</strong></a>
  ·
  <a href="#what-makes-it-elite">Why it's different</a>
  ·
  <a href="#quick-start">Quick start</a>
  ·
  <a href="#privacy">Privacy</a>
</p>

---

## What makes it elite

Flowtype isn't a web widget glued onto your desktop. It's a native tray app engineered around one job: **get words into the field you're already in — fast, clean, and invisible.**

**Speed you can feel**
- Capsule appears on key-down, **gone the instant you release** — no lingering UI, no focus steal
- Local engine **stays warm in memory** between dictations — repeat takes don't pay a cold-start penalty
- **Rapid-fire dictation** — back-to-back phrases without dropped hotkeys or mic lockouts
- Optional **turbo path** for long rants; optional **Groq** for cloud large-model accuracy when you want it

**Works everywhere**
- **System-wide** — Slack, email, browsers, Notion, VS Code, terminal, whatever has focus
- **Click-through overlay** — live waveform meter that never blocks clicks or keyboard input
- **Smart paste** — inserts into the focused field; falls back to clipboard if focus moved
- **Context-aware cleanup** — adapts tone to the app you're in (email vs code vs chat)

**Clean output, no LLM tax**
- Built-in cleanup handles fillers, self-corrections, spoken punctuation, lists, and numbers — **offline, in milliseconds**
- Custom **dictionary & snippets** plus a tray shortcut to fix the last misheard word in one click
- Cloud polish (OpenAI, OpenRouter, Ollama) exists if you want it — **off by default**

**Pro-grade polish**
- Matte voice capsule themes (or liquid glass if you're feeling fancy)
- Optional tactile audio cues — embedded in the app, not loaded from stale disk files
- **Latency strip** on every dictation: record · transcribe · clean · total
- Mic health meter + 3-second test so you diagnose quiet mics before blaming the engine
- Failed dictation **recovery** folder when you need it

**Private by design**
- No Flowtype account. No telemetry server. No training on your voice.
- API keys sealed with **Windows DPAPI** — per user, per machine
- Successful recordings **deleted immediately**. History **off by default**.

**Lean install**
- Lands in `%LOCALAPPDATA%` — no admin, no registry circus
- ~60 MB offline path or free cloud tier — not a half-gig "quality model" tax
- Single focused codebase — one executable, one purpose, no Electron stack

<p align="center"><em>Hold. Speak. Release. Back to work.</em></p>

---

## Compared to the usual options

| | Typical dictation app | Flowtype |
|---|---|---|
| **Account** | Required | None |
| **Default path** | Cloud-only or huge local model | ~60 MB offline Instant, optional Groq |
| **While recording** | Modal, focus steal, or floating bar | Click-through capsule, hides on release |
| **Between takes** | Cold start or queue delay | Warm engine, rapid-fire ready |
| **Cleanup** | LLM round-trip or nothing | Built-in offline rules (< few ms) |
| **Install** | Admin MSI or store app | User folder, one bat file |
| **Telemetry** | Usually yes | None |

---

## Quick start

1. **Download** the latest **Lite** or **Full** ZIP from [Releases](https://github.com/vectorfx/flowtype/releases).
   - **Lite** (~15 MB) — downloads the speech model on first local use
   - **Full** (~58 MB) — Instant model bundled, offline immediately
2. **Extract** the ZIP to a normal folder (not `Downloads` directly — use `Flowtype\`).
3. Run **`Install Flowtype.bat`**.
4. Hold **`Win + Ctrl`** (default hotkey), speak, release. Text pastes into the focused field.

> **First run:** Local mode needs no API key. For Groq, paste a free key under Settings → Cloud engines.

---

## Everyday controls

| Action | Result |
|---|---|
| Hold **Win + Ctrl** | Record — voice capsule appears |
| Release either key | Transcribe → clean → paste (or copy if focus changed) |
| **Escape** while recording | Cancel |
| Left-click tray icon | Open Settings |
| Right-click tray | Settings, history, recovery, quit |
| Right-click tray → **Fix "word" in dictionary…** | Add a spelling fix after a bad transcription |

---

## Voice capsule themes

Pick **Settings → General → Voice capsule**:

| Theme | Description |
|---|---|
| **Dark** *(default)* | Matte near-black pill, pro zinc borders, grey waveform |
| **Dark purple** | Matte purple capsule, purple waveform |
| **Light** | Clean white capsule for bright desktops |
| **Mono** | High-contrast black & white |
| **Liquid glass** | Frosted live-desktop glass (experimental) |

Audio cues on start/finish are optional — toggle **Sound effects on start and finish** in Settings.

---

## Speech engines

| Engine | Best for | Notes |
|---|---|---|
| **Local** *(default)* | Privacy, offline | Instant model (~60 MB). Warm server stays loaded between dictations. |
| **Groq** | Speed + accuracy online | Free API tier at [console.groq.com](https://console.groq.com). Model: `whisper-large-v3-turbo`. |
| **OpenAI** | Your own OpenAI key | Optional; you pay OpenAI directly. |

### Groq setup (recommended cloud option)

1. Create an account at [console.groq.com](https://console.groq.com).
2. **API Keys** → **Create API Key**.
3. Flowtype **Settings → General** → Speech engine: **Groq**.
4. **Cloud engines** tab → paste key. Model: `whisper-large-v3-turbo`.
5. Keep cleanup on **Built-in rules** for speed.

Audio goes to Groq for transcription only. Cleanup stays local unless you choose a cloud cleanup engine.

---

## When is AI used?

| Path | What runs |
|---|---|
| Local speech + built-in cleanup **(default)** | ASR model only — no ChatGPT-style rewrite |
| Groq / OpenAI speech | Cloud transcription |
| OpenRouter / OpenAI / Ollama cleanup | Optional LLM polish (off by default) |

---

## Settings worth knowing

**General → Performance**

- **Fast mode** — quicker on long dictations (skips expensive word timestamps)
- **Microphone boost** — software gain for quiet mics
- **Microphone health** — live meter + **Test 3s**
- **Latency strip** — last dictation: record · transcribe · clean · total (ms)

**General → Writing**

- **Smart cleanup** — fillers, punctuation, spoken numbers
- **Adapt cleanup to active app** — e.g. code vs email tone
- **Dictionary & snippets** — custom words and expansions

---

## Privacy

- No Flowtype account or analytics server
- API keys stored with Windows DPAPI (per user, per machine)
- Successful recordings deleted immediately
- History off by default
- Failed audio kept locally only if **Recovery** is enabled (`%APPDATA%\Flowtype\Recovery`)

---

## Build from source

Requires Windows with .NET Framework 4.x (built into Windows). No Visual Studio needed.

```powershell
git clone https://github.com/vectorfx/flowtype.git
cd flowtype
./tools/Fetch-Fonts.ps1
./tools/Build-Flowtype.ps1
./tests/Run-Tests.ps1
```

Output: `Flowtype.exe` in the repo root. Audio cues and fonts are embedded at build time from `assets/audio/` and `assets/fonts/`.

---

## Project layout

```
flowtype/
├── src/Flowtype.cs          # Single-file app (~4k lines)
├── assets/                  # Icon, fonts, audio cues
├── tools/                   # Build, package, font fetch
├── installer/               # Install-Flowtype.ps1
├── tests/Run-Tests.ps1      # Offline unit tests
└── .github/workflows/       # CI + release on tag
```

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for internals.

---

## Uninstall

Quit from the tray, then run **`Uninstall Flowtype.bat`**.

Settings and keys in `%APPDATA%\Flowtype` are kept unless you delete that folder manually.

---

## Third-party

See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

- whisper.cpp (MIT)
- ggml Whisper models (MIT)
- Roboto Mono (Apache 2.0)

---

## License

MIT — see [LICENSE](LICENSE).
