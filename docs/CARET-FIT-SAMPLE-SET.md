# Caret-fit sample set (v1.3.50)

Blind review: 200 modes. Triaged against Flowtype before implement. Join classifier + Docs probe added in 1.3.50.

## Score
- Already handled: ~35 (clipboard restore, paste debounce, terminal strip, messaging period strip, generation guard, quality reject)
- Real + missed (addressed this ship): casing/punct/space fit, I/I'm, acronyms, selection no-join, unread no-invent, continuity mid, fit-after-clean, note fitted punct, clause-continue, mid-word, incoming glue punct, abbreviation period, Docs selection probe
- Real but later: UIA Electron caret, late-paste abort on focus move, cancel-on-type, Vim mode, toast when caret unread
- Wrong / N/A: ~20

## Ship gates — verified green in RunCaretFitTests
1. Mid-sentence `quick fix` → lowercase, no period, one space
2. Preserve `I` / `I'm` / `I'll`
3. Preserve `API` / `OK` (no `aPI`)
4. Left already space → no second space
5. Selection replace → no join space
6. Sentence start / empty → keep sentence polish
7. Fit runs after Clean
8. Caret unread → do not invent lowercase
9. Unread + continuity → mid fit
10. Terminal still without forced `Cd.`
11. After comma → lowercase + space-as-needed
12. Incoming `,.;:!?` does not add a join space
13. Mid-word caret does not invent spaces
14. `e.g.` is not treated as a sentence start

## Deferred (do not block)
- Full UI Automation for Electron caret
- Abort paste if focus moved since record
- Cancel-on-type while cloud cleanup pending
- Warning toast when caret unread
