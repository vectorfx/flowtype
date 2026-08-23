# Changelog

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
- Emails, URLs, and filenames (kayleb.klopfer@gmail.com, github.com, flowtype.cs) are no longer split/capitalized apart
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
