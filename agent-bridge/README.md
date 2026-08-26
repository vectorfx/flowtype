# Flowtype Agent Bridge (PoC)

Voice → action, as a **dumb pipe**. Flowtype stays the mouth; a runtime you already
trust is the hands. Flowtype never plans, never routes, never executes — it POSTs
the finished transcript to ONE **loopback** endpoint and goes back to being a
dictation tool. If nothing is listening, the chord fails closed: you get a toast
and the take stays on your clipboard.

This is per-machine. Other people run the same daemon on **their** PC, pointed at
**their** CLI. It does not bind the LAN and it will not accept a request from a
web page.

## The loop

```
hold Win+Alt, speak, release
        │
Flowtype ── POST + token ──> http://127.0.0.1:5599/ask
                                      │
                        flowtype_agentd.py  (or --cli claude/codex/opencode)
```

Transcript mode is untouched: Win+Ctrl still pastes into the focused field.

## Setup (one time, on that user's machine)

1. Build/run Flowtype 1.3.71+.
2. **Settings → Agent**: enable, leave the endpoint as `http://127.0.0.1:5599/ask`, Test connection.
3. Start the daemon (loopback + token, default profile is **notes** = files only):

   ```powershell
   pip install claude-agent-sdk        # once, for the warm Claude session
   powershell -ExecutionPolicy Bypass -File .\start-agent.ps1
   ```

   Or attach the CLI they already use:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\start-agent.ps1 -Cli claude
   powershell -ExecutionPolicy Bypass -File .\start-agent.ps1 -Cli codex
   powershell -ExecutionPolicy Bypass -File .\start-agent.ps1 -Cli opencode -Profile commit
   ```

4. Sanity check without the mic:

   ```powershell
   powershell -ExecutionPolicy Bypass -File .\test-send.ps1
   ```

Each boot mints a token into `%APPDATA%\Flowtype\agent-token`. Flowtype reads that
file; browsers cannot. Origin/Referer requests are refused. The daemon listens on
`127.0.0.1` only.

**Why the daemon and not a cloud agent:** one hot session on this PC. Measured
2.9–3.5s per ask warm, against 10–20s for a cold `claude -p`.

| Flag | Does |
|---|---|
| `--port 5599` | match the endpoint in Settings |
| `--profile notes\|commit\|full` | files only, files+shell, or unrestricted. Default is **notes** |
| `--cwd <path>` | where the agent works (default: that user's home folder) |
| `--cli claude` | spawn their CLI per ask instead of the warm SDK |

`listen.ps1` is the original cold-spawn listener. It now requires the same token.

## The flight recorder

Every ask, reply, and failure is appended to `flight-recorder.jsonl` on that machine:

```json
{"event":"ask","text":"open my downloads folder","profile":"notes-only","ts":"..."}
{"event":"reply","text":"open my downloads folder","reply":"Opened Downloads (32 items)","ms":2870,"ts":"..."}
```

Nothing is uploaded.

## The ten-ask test (the actual experiment)

Grok's bar, unchanged: **one hotkey, ten real asks over a week.**

Rules:
- Only real asks you would otherwise have done by opening an app and typing.
- Log each one in the table below (the listener also logs to `bridge.log` and
  keeps every transcript in `asks/`).
- Score honestly: did you stop opening the other apps, or not?

| # | Date | Ask (short) | Did the agent finish it? | Would you have opened an app instead? |
|---|------|-------------|--------------------------|----------------------------------------|
| 1 |      |             |                          |                                        |
| 2 |      |             |                          |                                        |
| 3 |      |             |                          |                                        |
| 4 |      |             |                          |                                        |
| 5 |      |             |                          |                                        |
| 6 |      |             |                          |                                        |
| 7 |      |             |                          |                                        |
| 8 |      |             |                          |                                        |
| 9 |      |             |                          |                                        |
| 10 |     |             |                          |                                        |

**Read-out:** still opening the apps afterward → the pipe was not the missing
piece; park it. Stopped opening them → the agent chord earns a real slot in the
signed build (visible mode, endpoint setting in the UI, docs).

## What this deliberately is NOT

- No connector picker, no routing table, no "which app should handle this" brain.
- No remote access to someone else's machine. Loopback only.
- No PowerShell, no browser control, no actions inside Flowtype.exe.
- Not a product yet. The monetization sequence is unchanged: sign the binary,
  ship the Supporter SKU first. This folder is a personal instrument spike.
