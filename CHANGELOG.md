# Changelog

## 1.3.70

Dictation through a local streaming model:
- Find local models / Test stream turns on smart cleanup through that model, so dictation (not just the agent chord) polishes on this PC
- Tokens stream live on the capsule while the model writes, then Flowtype pastes once into the field you were in — one undo, and terminals still get Ctrl+Shift+V

Privacy and agent lock:
- The agent endpoint must be loopback (127.0.0.1 or localhost). Settings, the client, and loaded configs refuse anything else
- Example email in tests and changelog is now user@example.com

---

## 1.3.69

Terminal paste:
- Windows Terminal, PowerShell, cmd, and other consoles no longer look like a successful insert when Ctrl+V did nothing
- Those fields get Ctrl+Shift+V instead, and the take stays on your clipboard if it still cannot be confirmed — so a one-minute dictation is not wiped by restoring the previous copy
- Cursor/VS Code's integrated terminal is detected when the focused pane is actually a terminal, so the same backup applies there without changing editor paste
- The tray says Ctrl+Shift+V when that is the chord the field wants

Local streaming model:
- Optional polish can stream from a model on this PC (Ollama, LM Studio, or llama.cpp) — tokens stay local
- Settings → Local: Find local models / Test stream. If the model name is blank, Flowtype picks a small installed one (llama3.2:1b, Qwen, Phi, …)
- Cleanup engine label is **Ollama — local streaming model**

---

## 1.3.68

Liquid glass:
- On a dark page the live mark and waveform switch to light ink, so they stay visible against black terminals and dark editors
- Light pages still use the original graphite marks

---
## 1.3.67

Dictation start and end:
- The last word is no longer cut off — Flowtype now waits for the microphone's in-flight buffers instead of dropping them when you release
- Talking as you press the chord no longer eats the first word: the mic stays warm with a 400 ms preroll, and recording starts before the overlay comes up
- Quiet consonants at the start and end of a take stay in the audio instead of being trimmed as silence

---
## 1.3.66

Voice capsule:
- Grid live mark is a circle now — same equalizer columns filling from the bottom with your voice, without the square silhouette

Clipboard backup:
- If a popup, Start menu, taskbar, desktop, or another app steals the field before insert, Flowtype keeps the take on your clipboard instead of pasting into the thief
- If focus jumps away during the paste, the text stays copied too — click the field you want and press Ctrl+V

---
## 1.3.65

Spoken lists:
- Say **next point** during a take to start a new bullet, or **next number** for a numbered item — including a single command ("next point buy milk")
- Text before the first command becomes the first item, so "buy milk next point get eggs" is two bullets
- Settings → Personalization: turn the commands on or off, and change the phrases (comma-separate extras). "The next point…" stays ordinary words
- Existing "bullet point" / "next bullet" phrasing still works while the feature is on

---
## 1.3.59 (agent mode)

Agent HUD:
- Agent takes get their own terminal-style heads-up panel in the corner — run id, endpoint, live level trace while you speak, a spinner and a running clock while the agent works, then the agent's own one-line answer ("Renamed 6 screenshots by capture date · done in 2.9s")
- Nothing like the dictation capsule on purpose: different shape, different corner, monospace — you can always tell which key you are holding
- The tray icon switches to the agent mark while an ask is in the air, and back when it lands
- A failed ask says why and keeps the take on your clipboard

Agent settings:
- New Agent tab: enable/disable, pick the agent key, set the endpoint, and test the connection (it reports whether the session is warm)
- Fixes a real bug: saving settings on the previous build silently turned agent mode off, because the form rebuilt settings without the agent fields

Warm daemon:
- `agent-bridge/flowtype_agentd.py` keeps one agent session hot — measured 2.9-3.5s per ask against 10-20s for the old cold-start script — and answers with what it actually did
- Every ask, reply and failure is appended to `agent-bridge/flight-recorder.jsonl`
- Action profiles (`--profile notes|commit|full`) bound what a spoken ask is allowed to do; `--cli` still spawns any other agent CLI per ask

---

## 1.3.59

Agent chord (experimental, off by default):
- Second hold-to-talk chord (default Win + Alt) that sends the finished take to one local agent endpoint (`AgentEndpoint` in settings.json, default `http://127.0.0.1:5599/ask`) instead of pasting — Flowtype never plans, routes, or executes; it is a dumb pipe to a runtime you already trust
- Endpoint down → toast and the take stays on your clipboard; dictation is untouched either way
- Toggle from the tray menu ("Agent chord — hold Win + Alt"); the agent chord auto-moves if it would collide with the dictation chord

Seat lock:
- A bare "press enter" take now proves the window you dictated into still has focus — refocus it or refuse — instead of pulsing Enter into whatever stole focus while Whisper was thinking
- A swallowed duplicate delivery (debounce/generation match) no longer pulses a second Enter without pasting

---

## 1.3.58

Clipboard:
- Off still uses the clipboard to insert, but your previous copy is remembered at hotkey-down and put back on the UI thread after insert — a background timer was never firing, so spoken text stayed copied
- Restore also matches clipboard text that only differs by line endings, so Chrome/Docs cannot block the put-back

---

## 1.3.57

Clipboard:
- Copy something, dictate, then Ctrl+V again — you get what you copied, not the dictation and not an empty clipboard
- The user's clipboard is pinned when recording starts (before the Docs caret probe or Ctrl+V insert) and put back after insert
- Restore no longer bails just because the paste bumped the clipboard sequence, and Cursor no longer clears the clipboard instead of giving yours back

---

## 1.3.56

Voice capsule:
- First Liquid glass hold no longer paints a black slab — the layered window is cleared before the desktop is captured, and a dead-black grab is thrown away and recaptured
- Soft gray rim stays even on every side (1.3.55)

---

## 1.3.55

Voice capsule:
- Liquid glass keeps the soft gray edge — it is just even on every side now, not a darker stroke

---

## 1.3.54

Voice capsule:
- Liquid glass rim is the same graphite line on every side — the top is no longer a white stroke that vanishes on Docs, and the shadow is even instead of sitting only under the pill

---

## 1.3.53

Voice capsule:
- Liquid glass now draws the real desktop behind the pill (aligned, blurred, slightly warped) instead of a grey wash — the page color shows through the frost
- Single even 1px rim and a short contact shadow. No inner rectangle, no thicker side borders, no top hairline

---

## 1.3.52

Voice capsule:
- Waveform bars are back on the even 3px / 2px grid so the middle gap is gone. Live levels still follow the mic

---

## 1.3.51

Voice capsule:
- Liquid glass is the 1.3.47 frosted pill again (blur + light lensing + frost veil + inset shine). The React-filter port and the flat dark stadium are gone
- Waveform follows the mic again — no frozen cosine rest floor

---

## 1.3.50

Voice capsule:
- Liquid glass on light pages is a thin dark stadium again: backdrop blur only, no scale-70 displacement, no milky white fill, no fat halo
- Live mark and idle wavelength draw sharp on top of the glass, in graphite

Context-aware join (Docs / Word / browsers / editors — not Cursor):
- Join now classifies real left/right text: SentenceStart, ClauseContinue (after `,;:—`), MidSentence, MidWord, AfterOpen
- Leading capital only at a true sentence start; Whisper/Clean capitals are downcased when the caret is continuing a clause or sentence
- Space is taken from both sides: no `word,Next`, no double spaces, no extra space before incoming `,.;:!?`
- After abbreviations like `e.g.` / `Dr.` the next take stays mid-sentence
- When Win32 cannot read the caret (Google Docs), Flowtype tries a fail-closed Shift+Left copy of nearby text, then falls back to last-insert continuity
- Cursor / VS Code still skip caret-fit (spacing-only)

---

## 1.3.49

Voice capsule:
- Liquid glass now ports the LiquidButton filter: fractal noise, displacement scale 70, then blur — plus the dark inset rim stack, clipped to a stadium (no inner rectangle)
- **Grid** live mark (sv-matrix Square 18): columns fill from the bottom with your voice; the loader cycle only runs while Whisper is writing

Updates:
- Silent launch check still auto-installs a newer Lite zip
- Download URL must be this repo’s GitHub release, and the zip cannot write outside its extract folder

---

## 1.3.48

Voice capsule:
- Liquid glass is a dark refractive pill with a single stadium clip — no inner rectangle, no top hairline
- Live mark is now a setting: **Orb**, **Hex**, or **Iris**
- Start/finish cues use the new recordings, trimmed to the hit and timed with the pop in/out

---

## 1.3.47

Voice capsule:
- Mid-size stadium (104×26) with even padding and a single 1px rim so the sides are not thicker than the top
- Spinner only while Whisper is writing; talking uses a gentle pulse on the hex dots, not a chase
- Filled, pixel-aligned waveform bars with real contrast on Dark (no more grey hairlines)
- **Ember** theme (OLED black, copper-to-cream). **Mono** is gone — existing Mono users move to Dark

Updates:
- Launch checks GitHub every time (no 24-hour skip) and silent-installs a newer Lite zip without a dialog
- Tray **Check for updates…** still asks first; Cancel still skips that version

---

## 1.3.46

Voice capsule:
- Smaller pill (96×22) and shorter waveform so it sits tighter on the desktop
- Live mark is a hex dot-matrix (sv-matrix Hex Orbit / Glyph Cluster language) instead of concentric ping rings
- Processing spinner kept, scaled to the smaller capsule

---

## 1.3.45

Voice capsule:
- Tighter instrument chrome — double rim, hairline ticks, live/processing glyph on the left
- After you release, the peel stays up with a small AI loader while Whisper is writing
- Bars scan instead of going dead during transcription

Release tail:
- 150 ms after key-up (was 110, originally 180) plus 120 ms of pad after detected speech
- Dictation length does not need a longer tail — Whisper wants a short clean ending, not extra silence

---

## 1.3.44

Daily-driver pipeline:
- Trim leading/trailing silence on each take, then a shorter 110 ms release tail so Whisper gets less dead air
- Holding the hotkey again no longer cancels the previous transcription — takes queue and insert in order
- Tray "Undo last dictation", plus saying "scratch that" / "undo that" undoes the last paste (Ctrl+Z)
- Short takes (8 words or fewer) skip inferred list formatting so "first get milk" stays a sentence; spoken "bullet point" / "first…second…third" lists still format

Microphone health:
- The meter is now your real voice at the mic (raw peak). Aim for 15–40% while talking
- Advice no longer uses post-normalize "Whisper input" (that always looked ~70%, so 2× looked "perfect")
- If your voice is already loud, Flowtype tells you to leave boost alone — or lower it if you are at 2×

Dictionary:
- `eppi => epa` now converts Whisper variants like `eppy` to `epa`, instead of snapping `eppy` back to `eppi`
- Replacements apply even when cleanup is off

---

## 1.3.43

Whisper "thank you" silence hallucination:
- A thinking pause at the start or end of hold-to-talk no longer inserts a standalone "Thank you." / "Thanks." / "thanks for watching"
- A real "thank you", "thanks for coming", or a short closer like "Let me know. Thanks." is kept
- Long silence-only clips that decode to just "thank you" are rejected instead of pasted

---

## 1.3.42

Google Docs / browser continuation:
- Chrome and other browser editors often hide the caret and report no focused control. The next dictation in the same window is now treated as a continuation when the last insert did not end with `.?!` — so "okay so this one" + "is what I was using" becomes "okay so this one is what I was using" instead of a capital `Is` and a second space
- Unread mid-sentence inserts no longer invent join spaces (the user usually typed the space already)
- After a real sentence end, the leading space + capital is unchanged (`defended. Six`)
- Cursor / VS Code still skip caret-fit (spacing-only)

---

## 1.3.41

Spoken lists:
- "First X, second Y" no longer mangles prose that merely mentions ordinals — numbered lists require an ascending first/second/third sequence, and a would-be item that is just "And"/"Then" cancels list formatting entirely
- "First X, then Y, then Z" (comma-separated steps) now formats as a clean 1/2/3 list; "second of all"/"third of all" no longer leak "of all" into items

Single letters and numbers:
- Dictating a lone letter ("P") or number ("5", "10") now types it instead of being rejected as noise
- Letters near cue words ("the drive letter is P", "P as in Peter") survive the glitch filter; trailing lone-letter hallucinations at end of recording are now filtered again

Focus loss / lost dictations:
- If a window steals focus during dictation, text is delivered to the window you started dictating in (refocused if needed) — never blind-pasted into the thief; falls back to clipboard with a notification
- New tray item "Copy last dictation": every transcript stays recoverable, even superseded, rejected, or failed ones
- Clipboard restore now waits and checks ownership, so slow apps can't paste the OLD clipboard content; repeated identical dictations within 2.5 s are no longer silently swallowed

Text pipeline:
- "period"/"comma"/"colon" as ordinary nouns ("a long period of time", "the Oxford comma") are no longer converted to punctuation
- "no" only acts as a self-correction with punctuation on both sides ("the door, no answer" survives); bare "start over" as a verb phrase no longer wipes the sentence
- Emails, URLs, and filenames (user@example.com, github.com, flowtype.cs) are no longer split/capitalized apart
- Window-title words can no longer rewrite normal words ("tracing" stayed "Tracking" when a Tracking tab was open)

Stability and speed:
- Escape-cancel no longer runs heavy file work inside the keyboard hook (could kill the hotkey until restart)
- Caret probing now uses timeouts — a hung target app can no longer freeze Flowtype
- Groq dictations reuse the warmed connection (saves a TLS handshake per dictation); regex cache sized to the pipeline
- Build now compiles with /codepage:65001 so em dashes and typographic characters compile correctly

---

## 1.3.40

- Fix Google Docs / browser editors gluing text after periods (`defended.six` → `defended. Six`)
- Stop treating unread-caret short phrases as mid-sentence fragments — keep normal punctuation unless prior-insert continuity says otherwise

---

## 1.3.39

- Roll back CaretFit in Cursor/VS Code — restore normal punctuation and capitalization (spacing-only insert)
- CaretFit mid-sentence fit unchanged for apps that expose caret context (Notepad, Google Docs, etc.)

---

## 1.3.38

- Fix mid-sentence spacing: join space before *and* after the insert when neighbors need it
- Stop treating EM_GETSEL 0,0 as caret-at-end (that caused `macro  if it doesn'tshifter`)
- When caret can't be read, prefer trailing join space and short-fragment mid fit — never invent a leading space

---

## 1.3.37

- Mid-sentence dictation fit: lowercase fragments, skip forced periods, join with exactly one space using caret neighbors
- Preserve `I`/`I'm` and acronyms (`API`, `OK`) when fitting
- Selection replace does not add a join space; unread caret stays sentence-mode unless prior insert continuity says mid-fragment

---

## 1.3.36

- Remove live preview from the voice capsule and restore the original overlay
- Keep in-app auto-update from 1.3.35

---

## 1.3.35

- Live preview while speaking — partial transcript appears in the voice capsule during dictation (toggle in Settings)
- In-app auto-update — checks GitHub releases on startup, tray menu "Check for updates…", downloads and installs silently

---

## 1.3.34

- Fix liquid glass yellow/warm color bleed — desaturate and frost the backdrop to cool silver glass
- When "leave on clipboard" is off, always clear dictation from the clipboard (including Cursor)

---

## 1.3.33

- Fix Cursor insert broken by 1.3.32 Unicode typing — restore clipboard + Ctrl+V
- In Cursor/VS Code, leave dictation on the clipboard after paste (restoring/clearing was causing extra inserts)

---

## 1.3.32

- Attempted Cursor-specific Unicode typing to avoid duplicate paste (reverted — did not reach Electron composer)

---

## 1.3.31

- Fix triple/duplicate inserts: one deliver per recording, stronger transcript dedup, single Ctrl+V paste
- Fix missing space after periods when continuing dictation (e.g. `can.But` → `can. But`)
- Improve leading-space detection when appending to existing field text (caret-at-end fallback)

---

## 1.3.30

- Fix double-paste: debounce identical inserts and strip exact duplicate transcript blocks
- Add a leading space when dictating into a field that already has text before the caret

---

## 1.3.29

- Fix Groq dictation failing on every use after 1.3.28 (HttpClient timeout cannot change after warm-up)

---

## 1.3.28

- Paste into whichever field is focused when transcription finishes (switch monitors/apps mid-dictation)
- Scale local/Groq transcription timeouts with clip length so 10-minute recordings complete
- Notify when the 10-minute recording limit is reached

---

## 1.3.27

- Fix cut-off latency stats line in Settings (mic status and stats label were overlapping)
- Rename latency "total" to "pipeline" so it matches what the number actually measures

---

## 1.3.26

- Fix random/stale pastes: re-check focus before Ctrl+V and cancel in-flight inserts when a new recording starts
- Fix voice dead after alt-tab: restore generation-bump interrupt, reset hotkey state on foreground change
- Fix mic tester Whisper input reading (was using wrong peak metric; gain changes looked identical)
- Restore Discord/chat fuzzy-correction guard accidentally removed in 1.3.25
- Tighten inferred list detection again (min 3 items; drop comma-only prose path; stricter pause lists)

---

## 1.3.25

- Always insert dictation into the focused field (hands-free and hold-to-talk)
- Stop hijacking the clipboard by default; restore your previous clipboard after insert
- Fix stale paste race where a new dictation could re-insert the previous message
- Block starting a new recording while the last one is still being written
- Clipboard retention is now an optional setting, off by default and tucked under Optional

---

## 1.3.24

- Fix "final" becoming a Discord contact name (e.g. PinBal) during chat dictation
- Tighten fuzzy spell correction: first-letter match and protected common words like "final"
- Keep fuzzy correction from chat app window titles (Discord, Slack, etc.)

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
- **Dark purple**, **Light**, **Ember**, and **Liquid glass** themes
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
