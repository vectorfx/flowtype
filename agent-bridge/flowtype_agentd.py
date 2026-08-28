"""Flowtype agent daemon — the warm, accountable voice bus.

One persistent agent session stays hot, so an ask is answered in seconds instead of
paying a cold CLI start every time. Every ask and every reply is appended to a
JSONL flight recorder next to this file, so what the voice did is always answerable.

  python flowtype_agentd.py                 # warm Claude Agent SDK session
  python flowtype_agentd.py --port 5599
  python flowtype_agentd.py --cli claude    # fall back to spawning a CLI per ask
  python flowtype_agentd.py --profile full  # notes | commit | full  (action profile)

Flowtype POSTs {"text": ...} to http://127.0.0.1:<port>/ask and gets back
{"status":"ok","reply":"...","ms":1234} which it shows in the agent HUD.
"""

import argparse
import asyncio
import binascii
import json
import os
import shutil
import subprocess
import sys
import threading
import time
from datetime import datetime, timezone
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

HERE = os.path.dirname(os.path.abspath(__file__))
APP_DIR = os.path.join(os.environ.get("APPDATA", HERE), "Flowtype")
LOG_PATH = os.path.join(APP_DIR, "flight-recorder.jsonl")

# Action profiles bound the blast radius of a spoken ask. The active profile is
# reported on /status so the HUD can show which one is live.
PROFILES = {
    "notes": {
        "label": "notes-only",
        "allowed_tools": ["Read", "Glob", "Grep", "Write", "Edit"],
        "permission_mode": "acceptEdits",
    },
    "commit": {
        "label": "commit-safe",
        "allowed_tools": ["Read", "Glob", "Grep", "Write", "Edit", "Bash", "WebSearch", "WebFetch"],
        "permission_mode": "acceptEdits",
    },
    "full": {
        "label": "full-shell",
        "allowed_tools": None,
        "permission_mode": "bypassPermissions",
    },
}

SYSTEM_PROMPT = (
    "You are answering spoken asks relayed from Flowtype, a push-to-talk voice bus on Windows. "
    "The user spoke this out loud and is reading your reply on a small heads-up panel that wraps "
    "text and stays up while they read it. Plain text only: no markdown, no bullets, no preamble, "
    "no offers to help further.\n"
    "\n"
    "Two kinds of ask, two shapes of reply:\n"
    "1. DO something (open, move, commit, send, run). Do it, then reply with ONE short line under "
    "90 characters saying what you actually did — 'Opened Downloads (32 files)', 'Renamed 4 "
    "screenshots'. Name the object you touched so they can tell you picked the right one.\n"
    "2. TELL them something (what does it say, what's in it, when is it, who sent it, how much). "
    "The ANSWER IS THE POINT — give them the actual content, not a report that you looked. "
    "Wrong: 'Checked your inbox.' Right: 'Latest: Google, 21 Jul — you shared account data with "
    "Pixabay; nothing needs action.' Up to about 6 short lines / 500 characters when the content "
    "genuinely needs it; one line when it doesn't. Quote real names, dates, numbers and subject "
    "lines rather than describing them. Never say 'let me know if you want more detail' — they "
    "cannot reply; this one panel is the whole answer.\n"
    "\n"
    "3. ASK BACK. If the ask is ambiguous AND getting it wrong would be expensive or hard to "
    "undo — sending, spending, deleting, publishing, overwriting, touching someone else's stuff — "
    "do NOT guess. Ask ONE short question as the LAST line of your reply, starting that line "
    "with '? ' — at most one line of context before it, nothing after it. "
    "They will answer with another push-to-talk press and you will get it in this same "
    "conversation, so keep the question to one thing: '? Which Kevin — Kevin Tran or Kevin Ng?'. "
    "For anything cheap and reversible, don't ask — take the most likely reading, do it, and say "
    "which one you chose.\n"
    "\n"
    "4. TYPE IT. If they asked you to put text somewhere they are looking — 'type this in', "
    "'write it in the box', 'put that in the email I've got open', 'draft it here' — reply with "
    "the finished text ONLY, prefixed with '>> ' on the first line. No commentary, no quotes "
    "around it, no 'here you go'. Flowtype types it into the field they were in. If they asked "
    "you to SEND or SAVE it somewhere instead, that is a DO ask, not this one.\n"
    "\n"
    "THE SEAT. Some asks arrive with a [seat] line naming the window and app the person was "
    "looking at when they spoke. When they say 'this', 'that one', 'the file I've got open', or "
    "'the box that's open', that is what they mean — resolve it from the seat first and only fall "
    "back to searching or screenshotting if the seat cannot settle it.\n"
    "\n"
    "REVERSIBILITY. Before acting, classify what you are about to do:\n"
    "  UNDO — you can put it back exactly (create a file, open an app, copy something). Just do it.\n"
    "  REPAIR — reversible with effort (move files, edit a document, change a setting). Do it, and "
    "say precisely what you touched so it can be found again.\n"
    "  PERMANENT — cannot be taken back, or lands outside this machine: sending, replying, "
    "publishing, posting, paying, ordering, deleting without a recycle bin, force-pushing, "
    "deploying, wiping, anything that reaches another person. For these, confirm first with a '? ' "
    "question naming the exact target and the exact effect — '? Send to dave@acme.com now?' — "
    "UNLESS their spoken ask already named the target and the effect precisely, in which case do "
    "it. Never batch a PERMANENT action across many items without confirming the count first.\n"
    "\n"
    "MAKE IT VISIBLE. This is the difference between working and feeling broken. When an action "
    "produces something they are meant to SEE — a web page, a folder, a file, an app, a document — "
    "opening it is only half the job. They are usually looking at a different window, and a tab "
    "that opens behind their editor is indistinguishable from nothing happening at all.\n"
    "Do NOT try to raise the window yourself. Windows refuses focus changes from a background "
    "process, so AppActivate and SetForegroundWindow will appear to succeed and do nothing — and "
    "then you will report 'brought to front' when it is still buried, which is worse than not "
    "trying. Flowtype can do it, because it owns the keyboard hook.\n"
    "So: open the thing, then add a LAST line to your reply of exactly '@focus <name>' where "
    "<name> is the process or window to raise — '@focus chrome', '@focus explorer', "
    "'@focus notepad'. Flowtype strips that line and raises the window for you. Say where it "
    "appeared in your normal reply text; do not claim you focused it.\n"
    "\n"
    "DEFERRED WORK. You cannot stay running after you reply, so never say you will tell them "
    "later unless you actually arm it. To arm it, run this and then answer normally:\n"
    "  python \"{NOTIFY}\" --after 25m \"stand up\"\n"
    "  python \"{NOTIFY}\" --until \"<shell command that exits 0 when done>\" \"the build finished\"\n"
    "  python \"{NOTIFY}\" --until-file \"C:/path/to/output\" \"export is ready\"\n"
    "Start it detached so it does not block you. When it fires, the line appears on their heads-up "
    "panel. If you cannot arm it, say plainly that you cannot watch for it — do not promise."
).replace("{NOTIFY}", os.path.join(HERE, "notify.py"))


# Questions about the product itself get a written answer, not an improvised one.
# These are asked in the first hour by nearly every new user, and an agent guessing
# at its own privacy properties is exactly the wrong thing to ship.
CANNED = [
    (("what can you do", "what do you do", "what are you able", "what can i ask",
      "what are you capable"),
     "I run on your machine with a real shell, your files, and your connected accounts — "
     "mail, calendar, drive. Ask me to find or move files, run commands, check or draft mail, "
     "look things up, or take notes. Say 'type this in' and I write into the box you're in. "
     "I can't make phone calls, and I can't do anything after I answer unless I set a timer for it."),
    (("are you listening", "are you always listening", "do you hear everything",
      "is the mic always"),
     "No. The microphone only opens while you hold the chord, and it closes the moment you "
     "let go. Nothing is captured between takes."),
    (("where does my voice go", "is this going to a server", "do you send my voice",
      "where is my voice", "is my voice uploaded"),
     "Your speech is transcribed by whichever engine you picked in Settings — local Whisper "
     "stays on this machine; OpenAI or Groq send the clip to that provider. The finished text "
     "then goes to this agent, running locally on this PC, using the runtime you connected."),
    (("can my work see", "can my employer see", "does my boss see", "is this monitored"),
     "Nothing here reports to anyone. Every ask and reply is written to two local files in "
     "this Windows user profile — agent-replies.log and flight-recorder.jsonl under AppData. "
     "Anyone with access to this PC can read them; nothing is sent anywhere else."),
    (("can you see my screen", "do you see my screen", "are you watching my screen"),
     "Only if you ask me to. I know the title of the window you were in when you spoke, and I "
     "can take a screenshot and look at it when a task needs it — but I don't watch continuously."),
    (("what do you know about me", "what do you remember", "do you remember me"),
     "Nothing between restarts. I remember this conversation while the daemon is running and "
     "forget all of it when it stops. There is no profile."),
]


def canned_answer(text):
    lowered = " ".join(text.lower().split())
    if len(lowered) > 90:
        return None
    for triggers, answer in CANNED:
        if any(trigger in lowered for trigger in triggers):
            return answer
    return None


def stamp():
    return datetime.now(timezone.utc).isoformat()


def log_line(record):
    record["ts"] = stamp()
    try:
        os.makedirs(os.path.dirname(LOG_PATH), exist_ok=True)
        with open(LOG_PATH, "a", encoding="utf-8") as handle:
            handle.write(json.dumps(record, ensure_ascii=False) + "\n")
    except Exception:
        pass


def say(message):
    print(time.strftime("%H:%M:%S") + "  " + message, flush=True)


def issue_token():
    """Mint a per-boot secret and hand it to Flowtype through the user's AppData.

    Anything that can read this file already runs as the user and could run the shell
    directly, so the file is the trust boundary — not the loopback interface.
    """
    global TOKEN
    TOKEN = binascii.hexlify(os.urandom(24)).decode("ascii")
    try:
        os.makedirs(os.path.dirname(TOKEN_PATH), exist_ok=True)
        with open(TOKEN_PATH, "w", encoding="utf-8") as handle:
            handle.write(TOKEN)
        return True
    except Exception as exception:
        say("could not write the token file (" + str(exception)[:80] + ")")
        return False


def probe_environment():
    """Gather the real facts about this machine once, at boot.

    Without this the agent rediscovers where it is on every ask — the first window
    lookup of the session measured 12.6s of PowerShell spelunking for a fact that
    costs nothing to state up front. It also stops it guessing at bash or at paths
    that do not exist on Windows.
    """
    script = (
        "$ErrorActionPreference='SilentlyContinue';"
        "$b=(Get-ItemProperty 'HKCU:\\SOFTWARE\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\https\\UserChoice').ProgId;"
        "$o=(Get-CimInstance Win32_OperatingSystem).Caption;"
        "Add-Type -AssemblyName System.Windows.Forms;"
        "$s=[System.Windows.Forms.Screen]::AllScreens.Count;"
        "[pscustomobject]@{os=$o;browser=$b;screens=$s;home=$env:USERPROFILE;"
        "ps=$PSVersionTable.PSVersion.ToString()} | ConvertTo-Json -Compress"
    )
    try:
        out = subprocess.run(["powershell", "-NoProfile", "-NonInteractive", "-Command", script],
                             capture_output=True, text=True, timeout=25)
        facts = json.loads((out.stdout or "").strip())
    except Exception:
        return ""
    return format_environment(facts)


def format_environment(facts):
    """Facts the model needs. No username — that is private and unused."""
    facts = facts or {}
    lines = [
        "THIS MACHINE (measured at startup — do not go rediscovering it):",
        "  OS: " + str(facts.get("os", "Windows")),
        "  Shell: Windows PowerShell " + str(facts.get("ps", "5.1")) + ". Bash is NOT available. "
        "Use PowerShell syntax — Start-Process, Get-ChildItem, $env:VAR. No /tmp, no ~, no &&.",
        "  Home: " + str(facts.get("home", "")) + " (Desktop, Downloads, Documents live under it)",
        "  Default browser: " + str(facts.get("browser", "unknown")) +
        " — open pages with Start-Process against it, never a headless or automation browser.",
        "  Monitors: " + str(facts.get("screens", 1)) +
        " — a window you open may land on a screen or behind the app they are working in.",
    ]
    return "\n".join(lines)


ENVIRONMENT = ""
TOKEN = ""
TOKEN_PATH = os.path.join(os.environ.get("APPDATA", HERE), "Flowtype", "agent-token")
NOTICE_QUEUE = os.path.join(HERE, "notices.jsonl")
_notice_gate = threading.Lock()


def drain_notices():
    """Hand over any deferred notices that have fired, then clear the queue.

    notify.py appends here from its own process, so this is the handoff point between
    'the thing you asked me to watch' and the panel in front of you.
    """
    with _notice_gate:
        if not os.path.exists(NOTICE_QUEUE):
            return []
        try:
            with open(NOTICE_QUEUE, "r", encoding="utf-8") as handle:
                lines = [line.strip() for line in handle if line.strip()]
            os.remove(NOTICE_QUEUE)
        except Exception:
            return []
    notices = []
    for line in lines:
        try:
            notices.append(json.loads(line))
        except Exception:
            notices.append({"text": line})
    if notices:
        say("NOTICE x" + str(len(notices)) + " delivered to the HUD")
    return notices


def shape_reply(reply):
    """Keep the whole answer, not just its first line, and spot a question coming back.

    The old code took `.splitlines()[0]`, which silently deleted every line after the
    first — so an answer whose whole point was its content arrived as a stub. The HUD
    wraps text now, so the cap is on total size, not on line count.
    """
    text = (reply or "done").strip()

    # '@focus <name>' asks Flowtype to raise a window. It is stripped here so the marker
    # never reaches the panel, and passed back as its own field.
    focus = ""
    kept = []
    for line in text.splitlines():
        stripped = line.strip()
        if stripped.lower().startswith("@focus "):
            focus = stripped[7:].strip().strip("\"'")
            continue
        kept.append(line)
    text = "\n".join(kept).strip() or "done"

    # '>> ' means the whole reply is text destined for the user's cursor, not for the panel.
    paste = text.startswith(">>")
    if paste:
        text = text[2:].lstrip()
        return text or "done", False, True, focus
    # The question marker can arrive on its own line after a line of context, so look
    # for it anywhere rather than only at the very start.
    lines = [line.rstrip() for line in text.splitlines()]
    question = any(line.lstrip().startswith("?") and line.lstrip()[1:2] in (" ", "")
                   for line in lines if line.strip())
    if question:
        lines = [(line.lstrip()[1:].lstrip() if line.lstrip().startswith("? ") else line)
                 for line in lines]
    while lines and not lines[0]:
        lines.pop(0)
    if len(lines) > 10:
        lines = lines[:10] + ["…"]
    text = "\n".join(lines).strip()
    if len(text) > 800:
        text = text[:797].rstrip() + "…"
    return (text or "done"), question, False, focus


EMPTY_CLI_REPLY = "The agent started but did not answer. Try again."


def is_cli_noise(line):
    lower = (line or "").strip().lower()
    if not lower:
        return False
    if lower.startswith("[claude-mem]"):
        return True
    if "opencode plugin" in lower or "plugin loading" in lower:
        return True
    return False


def extract_cli_reply(text):
    """Keep the agent's answer. Drop OpenCode/Claude CLI plugin banners.

    Cold spawn used to take stdout's last line, so a banner after a real
    answer stole the HUD — and a banner-only spawn looked like Claude.
    """
    if not (text or "").strip():
        return EMPTY_CLI_REPLY
    kept = [line for line in text.splitlines() if not is_cli_noise(line)]
    out = "\n".join(kept).strip()
    return out if out else EMPTY_CLI_REPLY


def runtime_label(cli):
    if not cli:
        return "Claude"
    base = os.path.basename(cli).lower()
    if base.startswith("opencode"):
        return "OpenCode"
    if base.startswith("claude"):
        return "Claude"
    return cli


class WarmSession(object):
    """Keeps one Claude Agent SDK client alive on its own asyncio loop."""

    def __init__(self, profile, cwd):
        self.profile = profile
        self.cwd = cwd
        self.loop = asyncio.new_event_loop()
        self.client = None
        self.lock = threading.Lock()
        self.ready = False
        self.turns = 0
        self.in_flight = False
        self.thread = threading.Thread(target=self._run_loop, daemon=True, name="agent-loop")
        self.thread.start()

    def _run_loop(self):
        asyncio.set_event_loop(self.loop)
        self.loop.run_forever()

    def _submit(self, coro):
        return asyncio.run_coroutine_threadsafe(coro, self.loop).result()

    def start(self):
        from claude_agent_sdk import ClaudeSDKClient, ClaudeAgentOptions

        spec = PROFILES[self.profile]
        options = ClaudeAgentOptions(
            system_prompt=SYSTEM_PROMPT + ENVIRONMENT,
            permission_mode=spec["permission_mode"],
            cwd=self.cwd,
        )
        if spec["allowed_tools"]:
            options.allowed_tools = spec["allowed_tools"]

        async def _boot():
            client = ClaudeSDKClient(options=options)
            await client.connect()
            return client

        self.client = self._submit(_boot())
        self.ready = True

    def ask(self, text):
        """Send one ask into the warm session; return the agent's final line."""
        from claude_agent_sdk import AssistantMessage, TextBlock, ResultMessage

        async def _turn():
            await self.client.query(text)
            reply = ""
            async for message in self.client.receive_response():
                if isinstance(message, AssistantMessage):
                    for block in message.content:
                        if isinstance(block, TextBlock) and block.text.strip():
                            reply = block.text.strip()
                elif isinstance(message, ResultMessage):
                    result = getattr(message, "result", None)
                    if result and str(result).strip():
                        reply = str(result).strip()
            return reply

        with self.lock:
            self.turns += 1
            self.in_flight = True
            try:
                return self._submit(_turn())
            finally:
                self.in_flight = False

    def abort(self):
        """Stop whatever is running now. 'No, stop, don't send it' has to reach something."""
        if not self.client or not getattr(self, "in_flight", False):
            return False
        try:
            self._submit(self.client.interrupt())
            return True
        except Exception:
            return False

    def close(self):
        if not self.client:
            return
        try:
            self._submit(self.client.disconnect())
        except Exception:
            pass


class BootingExecutor(object):
    """Placeholder so /status works the moment the port opens, before the session is warm."""

    ready = False
    turns = 0

    def ask(self, text):
        raise RuntimeError("agent is still warming up")

    def abort(self):
        return False


class ColdSpawner(object):
    """Fallback executor: one CLI process per ask. Slow, but needs no SDK."""

    ready = True
    turns = 0

    def __init__(self, cli, cwd, model=None):
        self.cli = resolve_cli(cli)
        self.cwd = cwd
        self.model = (model or "").strip()
        self.lock = threading.Lock()

    def start(self):
        pass

    def ask(self, text):
        with self.lock:
            self.turns += 1
            cmd = cli_command(self.cli, text, self.model)
            completed = subprocess.run(
                cmd,
                cwd=self.cwd,
                capture_output=True,
                text=True,
                timeout=600,
            )
            out = (completed.stdout or "").strip()
            if not out:
                out = (completed.stderr or "").strip()
            if completed.returncode and not out:
                return "agent CLI failed (" + str(completed.returncode) + ")"
            return extract_cli_reply(out)

    def abort(self):
        return False

    def close(self):
        pass


def resolve_cli(name):
    found = shutil.which(name)
    if found:
        return found
    if os.name == "nt":
        for extra in (name + ".cmd", name + ".exe", name + ".bat"):
            found = shutil.which(extra)
            if found:
                return found
    return name


def cli_command(cli, text, model=None):
    base = os.path.basename(cli).lower()
    if base.startswith("opencode"):
        cmd = [cli, "run", "--auto"]
        if model:
            cmd.extend(["-m", model])
        cmd.append(text)
        return cmd
    return [cli, "-p", text]


class Handler(BaseHTTPRequestHandler):
    executor = None
    profile_label = "?"
    runtime_name = ""
    protocol_version = "HTTP/1.1"

    def log_message(self, fmt, *args):
        pass  # our own logging is friendlier

    def _send(self, code, payload):
        body = json.dumps(payload).encode("utf-8")
        self.send_response(code)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Connection", "close")
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        allowed, reason = self.authorised()
        if not allowed:
            self._send(403, {"status": "error", "error": reason})
            return
        if self.path.startswith("/status"):
            self._send(200, {
                "status": "ok",
                "warm": bool(getattr(Handler.executor, "ready", False)),
                "profile": Handler.profile_label,
                "runtime": Handler.runtime_name,
                "turns": getattr(Handler.executor, "turns", 0),
            })
        elif self.path.startswith("/notices"):
            self._send(200, {"status": "ok", "notices": drain_notices()})
        else:
            self._send(404, {"status": "error", "error": "unknown path"})

    def authorised(self):
        """Only Flowtype may drive this. Loopback is NOT a trust boundary.

        Any web page the user visits can POST to 127.0.0.1 from their browser. The old
        handler took a non-JSON body verbatim as the prompt, so a page could have driven a
        full-shell agent on this machine. Three gates now: a shared token that only a local
        process able to read the token file can know; refusal of anything carrying an Origin
        or Referer (a browser always sends one, Flowtype never does); and a JSON content type,
        which forces a preflight a cross-site page cannot satisfy.
        """
        if self.headers.get("Origin") or self.headers.get("Referer"):
            return False, "browser-origin requests are refused"
        host = (self.headers.get("Host") or "").split(":")[0].strip().lower()
        if host not in ("127.0.0.1", "localhost", "::1"):
            return False, "host is not loopback"
        if (self.headers.get("X-Flowtype-Token") or "") != TOKEN:
            return False, "bad or missing token"
        return True, ""

    def do_POST(self):
        allowed, reason = self.authorised()
        if not allowed:
            say("REFUSED " + self.path + " - " + reason)
            log_line({"event": "refused", "path": self.path, "reason": reason})
            self._send(403, {"status": "error", "error": reason})
            return
        if self.path.startswith("/abort"):
            stopped = bool(Handler.executor and Handler.executor.abort())
            say("ABORT requested — " + ("interrupted" if stopped else "nothing in flight"))
            log_line({"event": "abort", "stopped": stopped})
            self._send(200, {"status": "ok", "stopped": stopped})
            return
        ctype = (self.headers.get("Content-Type") or "").lower()
        if "json" not in ctype:
            self._send(415, {"status": "error", "error": "body must be application/json"})
            return
        length = int(self.headers.get("Content-Length") or 0)
        raw = self.rfile.read(length).decode("utf-8", "replace") if length else ""
        seat = None
        try:
            parsed = json.loads(raw)
            text = (parsed.get("text") or "").strip()
            seat = parsed.get("context") or None
        except Exception:
            self._send(400, {"status": "error", "error": "body must be JSON"})
            return

        if not text:
            self._send(400, {"status": "error", "error": "empty ask"})
            return

        # The seat rides in front of the ask so "this" and "that one" resolve without
        # the agent having to go hunting for the foreground window itself.
        prompt = text
        if isinstance(seat, dict) and (seat.get("window") or seat.get("app")):
            prompt = ("[seat] they were looking at: " + str(seat.get("app") or "?")
                      + " — \"" + str(seat.get("window") or "")[:200] + "\"\n" + text)

        say("ASK  " + text[:110])
        log_line({"event": "ask", "text": text, "seat": seat, "profile": Handler.profile_label})
        started = time.time()

        # Questions about the product answer from a written script, instantly. An agent
        # improvising its own privacy properties is the one answer that must not be guessed.
        scripted = canned_answer(text)
        if scripted:
            ms = int((time.time() - started) * 1000)
            say("CANNED " + str(ms) + "ms  " + scripted[:80])
            log_line({"event": "reply", "text": text, "reply": scripted, "ms": ms, "canned": True})
            self._send(200, {"status": "ok", "reply": scripted, "ms": ms,
                             "question": False, "paste": False, "focus": ""})
            return

        if Handler.executor is None or not getattr(Handler.executor, "ready", False):
            self._send(503, {"status": "warming", "error": "agent is still warming up"})
            return

        try:
            reply = Handler.executor.ask(prompt)
            ms = int((time.time() - started) * 1000)
            reply, question, paste, focus = shape_reply(reply)
            say(("ASK? " if question else ("TYPE " if paste else "DONE ")) + str(ms) + "ms  "
                + reply[:100] + ("  [focus " + focus + "]" if focus else ""))
            log_line({"event": "reply", "text": text, "reply": reply, "ms": ms,
                      "question": question, "paste": paste, "focus": focus})
            self._send(200, {"status": "ok", "reply": reply, "ms": ms,
                             "question": question, "paste": paste, "focus": focus})
        except Exception as exception:
            ms = int((time.time() - started) * 1000)
            message = str(exception)[:300]
            say("FAIL " + message)
            log_line({"event": "error", "text": text, "error": message, "ms": ms})
            self._send(500, {"status": "error", "error": message, "ms": ms})


def main():
    parser = argparse.ArgumentParser(description="Flowtype warm agent daemon")
    parser.add_argument("--port", type=int, default=5599)
    parser.add_argument("--profile", choices=list(PROFILES.keys()), default="notes",
                        help="notes = files only; commit = files+shell; full = unrestricted (default used to be full — now notes)")
    parser.add_argument("--cwd", default=os.path.expanduser("~"))
    parser.add_argument("--cli", default=None,
                        help="skip the warm SDK session and spawn this CLI per ask (claude, codex, opencode, …)")
    parser.add_argument("--model", default=None,
                        help="optional provider/model for --cli opencode, e.g. ollama/qwen2.5-coder:14b")
    args = parser.parse_args()

    global ENVIRONMENT
    spec = PROFILES[args.profile]
    Handler.executor = BootingExecutor()
    Handler.profile_label = spec["label"]
    Handler.runtime_name = runtime_label(args.cli)
    if issue_token():
        say("token issued -> " + TOKEN_PATH)

    # Bind the port before warming. Settings → Test connection should succeed as soon as
    # this line prints, not after a 10–30s Claude session boot.
    server = ThreadingHTTPServer(("127.0.0.1", args.port), Handler)
    listener = threading.Thread(target=server.serve_forever, name="agent-http")
    listener.daemon = True
    listener.start()
    say("listening on http://127.0.0.1:" + str(args.port) + "/ask")

    say("reading the machine ...")
    facts = probe_environment()
    if facts:
        ENVIRONMENT = "\n\n" + facts
        say("environment: " + facts.splitlines()[1].strip())
    else:
        say("environment probe failed - the agent will have to discover the machine itself")

    if args.cli:
        executor = ColdSpawner(args.cli, args.cwd, args.model)
        mode = "cold spawn: " + args.cli + ((" / " + args.model) if args.model else "")
    else:
        try:
            executor = WarmSession(args.profile, args.cwd)
            say("warming agent session (" + spec["label"] + ") ...")
            executor.start()
            mode = "warm Claude Agent SDK"
        except Exception as exception:
            say("warm session unavailable (" + str(exception)[:120] + ") - falling back to cold spawn")
            executor = ColdSpawner("claude", args.cwd)
            mode = "cold spawn: claude (fallback)"

    Handler.executor = executor
    say("mode: " + mode + "  profile: " + spec["label"] + "  cwd: " + args.cwd)
    say("flight recorder: " + LOG_PATH)
    say("hold the agent chord in Flowtype (default Win+Alt) and speak. Ctrl+C to stop.")
    log_line({"event": "daemon-start", "mode": mode, "profile": spec["label"], "port": args.port})
    try:
        while listener.is_alive():
            listener.join(0.5)
    except KeyboardInterrupt:
        say("stopping")
    finally:
        executor.close()
        log_line({"event": "daemon-stop"})


if __name__ == "__main__":
    main()
