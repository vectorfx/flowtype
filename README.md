<p align="center">
  <img src="assets/Flowtype-icon.png" alt="Flowtype" width="96">
</p>

<h1 align="center">Flowtype</h1>

<p align="center">
  <strong>Hold a key. Speak. Release. Clean text appears where you were typing.</strong>
</p>

<p align="center">
  A lightweight Windows tray app for system-wide push-to-talk dictation.<br>
  Local by default. No account. No telemetry. Keys encrypted with Windows DPAPI.
</p>

<p align="center">
  <a href="https://github.com/vectorfx/flowtype/releases/latest">Download latest release</a>
  ·
  <a href="#quick-start">Quick start</a>
  ·
  <a href="#speech-engines">Engines</a>
  ·
  <a href="#privacy">Privacy</a>
</p>

---

## Why Flowtype?

Most dictation tools want a subscription, a cloud account, or a 600 MB model sitting on your disk. Flowtype is the opposite:

| | Flowtype |
|---|---|
| **Default mode** | Local whisper.cpp (~60 MB Instant model) — works offline |
| **Install** | User folder (`%LOCALAPPDATA%`) — no admin, no registry drama |
| **While dictating** | Tiny voice capsule at the bottom of the screen — click-through, hides the instant you release |
| **After dictating** | Built-in cleanup (fillers, punctuation, lists) — no LLM required |
| **Fast path** | Optional [Groq](https://console.groq.com) free tier for cloud `whisper-large-v3-turbo` |

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
