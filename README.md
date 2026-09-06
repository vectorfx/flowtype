<p align="center">
  <img src="assets/Flowtype-icon.png" alt="Flowtype" width="96">
</p>

<h1 align="center">Flowtype</h1>

<p align="center">
  <strong>Hold a key. Speak. Text lands where you were typing.</strong><br>
  System-wide push-to-talk dictation for Windows — offline by default, tray-resident, one EXE.
</p>

<p align="center">
  <a href="https://github.com/vectorfx/flowtype/releases/latest"><img alt="Latest release" src="https://img.shields.io/github/v/release/vectorfx/flowtype?label=release&color=111827"></a>
  <a href="LICENSE"><img alt="MIT" src="https://img.shields.io/badge/license-MIT-111827"></a>
  <a href="https://github.com/vectorfx/flowtype/actions/workflows/ci.yml"><img alt="CI" src="https://img.shields.io/github/actions/workflow/status/vectorfx/flowtype/ci.yml?branch=main&label=CI&color=111827"></a>
</p>

<p align="center">
  <a href="https://github.com/vectorfx/flowtype/releases/latest"><strong>Download</strong></a>
  ·
  <a href="#install">One-line install</a>
  ·
  <a href="#why-flowtype">Why it feels different</a>
  ·
  <a href="#privacy">Privacy</a>
</p>

---

## What it does

Flowtype lives in the tray. Hold **Win + Ctrl**, talk, release. The take is transcribed, cleaned, and pasted into whatever field had focus — Cursor, Chrome, Slack, a terminal, Notepad — without stealing that focus.

No account. No telemetry. No Electron shell. Local whisper.cpp is the default. Cloud engines are opt-in.

---

## Why Flowtype

Most “AI dictation” apps are a recorder bolted to a cloud API. Flowtype is built around the hard Windows parts:

| | |
|---|---|
| **First word stays** | Warm mic + 400&nbsp;ms pre-roll so the start of a take isn’t eaten by `waveInOpen` |
| **Paste that sticks** | Batched `SendInput`, clipboard restore, Cursor / terminal heuristics, rescue-to-clipboard when focus races |
| **Hotkey that survives** | Low-level keyboard hook + 20&nbsp;ms chord poller backup + auto-reinstall if Windows drops the hook |
| **Cleanup that doesn’t invent** | Deep built-in polish offline — fillers, punctuation, spoken lists, exact dictionary — before any LLM |
| **Voice capsule** | Click-through overlay (Dark / Glass / Ember…) that never steals focus |
| **Optional agent chord** | Hold **Win + Alt** → loopback POST to OpenCode or Claude on *your* PC. Flowtype is a dumb pipe; it does not plan or execute |

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full pipeline.

---

## Install

```powershell
irm https://raw.githubusercontent.com/vectorfx/flowtype/main/install.ps1 | iex
```

Downloads the latest **Full** release (app + offline Instant model + agent bridge), installs to `%LOCALAPPDATA%\Flowtype`, adds shortcuts, and starts.

**Manual:** grab the Full zip from [Releases](https://github.com/vectorfx/flowtype/releases/latest), extract somewhere normal (not loose in Downloads), run **`Install Flowtype.bat`**.

| Package | |
|---|---|
| **Full** (~60&nbsp;MB) | Instant speech model bundled — offline immediately. This is the one you want. |
| **Lite** (~15&nbsp;MB) | Same app; model downloads later if you use local Whisper |

---

## Quick start

1. Install (above).
2. Hold **Win + Ctrl**, speak, release — text appears in the focused field.
3. Optional: Settings → **Cloud** → paste a free [Groq](https://console.groq.com) key for faster cloud ASR (cleanup stays local unless you choose otherwise).
4. Optional: Settings → **Agent** → **Connect OpenCode** or **Connect Claude**, then hold **Win + Alt** to send a take to a local runtime you already trust.

Local mode needs no API key.

---

## Everyday controls

| Action | Result |
|---|---|
| Hold **Win + Ctrl** | Record — voice capsule appears |
| Release | Transcribe → clean → paste (or keep on clipboard if focus changed) |
| **Escape** while recording | Cancel |
| Double-press (hands-free) | Latch recording without holding — optional in Settings |
| Left-click tray | Settings |
| Right-click tray | Dictionary fix, undo last, history, recovery, quit |

---

## Specs

| | |
|---|---|
| **Platform** | Windows 10 / 11 (x64) |
| **Install** | Per-user · no admin |
| **Speech** | Local whisper.cpp (warm server) · Groq / OpenAI optional |
| **Cleanup** | Built-in rules (default) · optional OpenAI / OpenRouter / Ollama |
| **Output** | Paste into focused field · clipboard restore |
| **Privacy** | No account · no telemetry · API keys via Windows DPAPI |

---

## Speech engines

| Engine | |
|---|---|
| **Local** *(default)* | Offline · Instant English model (~60&nbsp;MB) · warm between takes |
| **Groq** | Free tier · `whisper-large-v3-turbo` · falls back to Local on hard failures |
| **OpenAI** | Bring your own key |

### Groq setup

1. [console.groq.com](https://console.groq.com) → **API Keys** → **Create API Key**
2. Settings → **Cloud engines** → paste under Groq
3. Keep cleanup on **Built-in rules** unless you want cloud polish

---

## Settings at a glance

| Tab | |
|---|---|
| **General** | Hotkey · hands-free · engine · cleanup · voice capsule · input & performance |
| **Cloud** | Groq · OpenAI · OpenRouter |
| **Local** | whisper.cpp · Ollama / LM Studio URL |
| **Personalization** | Spoken lists · dictionary · snippets |
| **Agent** | Connect OpenCode / Claude · arm the agent key (last on purpose) |

Microphone boost is **off by default** — toggle it on under Input & performance only if the mic is quiet.

**Spoken lists:** say **next point** / **next number** (customizable) to force bullets or numbered items.

---

## Voice capsule

| Theme | |
|---|---|
| **Dark** *(default)* | Matte near-black |
| **Dark purple** · **Light** · **Ember** | Alternate looks |
| **Liquid glass** | Frosted blur of the desktop behind the pill |

Live marks: **Orb**, **Hex**, **Iris**, **Grid**. Optional start/finish sound cues.

---

## Privacy

- No Flowtype account or analytics
- API keys encrypted with Windows DPAPI (per user, per machine)
- Successful recordings deleted immediately
- History off by default
- Failed audio kept locally only if **Recovery** is on (`%APPDATA%\Flowtype\Recovery`)
- Agent chord talks to **loopback only** — never binds the LAN

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

Output: `Flowtype.exe` in the repo root. Fonts and audio cues are embedded at build time.

```
flowtype/
├── src/Flowtype.cs       # Single-file app
├── agent-bridge/         # Optional loopback daemon for the agent chord
├── install.ps1           # One-line installer
├── assets/               # Icon, fonts, cues
├── tools/                # Build, package, fonts
├── installer/            # Install-Flowtype.ps1
└── tests/
```

---

## Uninstall

Quit from the tray, then run **`Uninstall Flowtype.bat`**.

`%APPDATA%\Flowtype` (settings / keys) is kept unless you delete it.

---

## Third-party

See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

- whisper.cpp (MIT)
- ggml Whisper models (MIT)
- Space Grotesk (SIL Open Font License 1.1)

---

## License

MIT — see [LICENSE](LICENSE).
