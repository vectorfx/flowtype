"""Deferred notice — the piece that lets "tell me when the build's done" be true.

The agent runs this and walks away. It waits, then drops one line into
notices.jsonl next to this file. The daemon serves that queue at GET /notices and
Flowtype polls it, so the line surfaces in the agent HUD whenever it lands.

  python notify.py --after 25m "stand up and stretch"
  python notify.py --until "docker ps -q -f name=api" "the api container is up"
  python notify.py --until-file "C:/build/out.exe" "build finished"

Nothing here plans or executes anything on the user's behalf — it waits, then says
one sentence. Same dumb-pipe rule as the rest of the bridge.
"""

import argparse
import json
import os
import subprocess
import sys
import time
from datetime import datetime, timezone

HERE = os.path.dirname(os.path.abspath(__file__))
QUEUE = os.path.join(HERE, "notices.jsonl")
MAX_WAIT_SECONDS = 12 * 60 * 60
POLL_SECONDS = 15


def parse_duration(value):
    """'25m', '90s', '2h', '45' (seconds) -> seconds."""
    text = str(value).strip().lower()
    units = {"s": 1, "m": 60, "h": 3600, "d": 86400}
    if text and text[-1] in units:
        return int(float(text[:-1]) * units[text[-1]])
    return int(float(text))


def emit(text, source):
    record = {
        "text": text,
        "source": source,
        "ts": datetime.now(timezone.utc).isoformat(),
    }
    with open(QUEUE, "a", encoding="utf-8") as handle:
        handle.write(json.dumps(record, ensure_ascii=False) + "\n")
    print("notice queued: " + text, flush=True)


def main():
    parser = argparse.ArgumentParser(description="Queue a deferred notice for the Flowtype HUD")
    parser.add_argument("text", help="the one line to show when it fires")
    parser.add_argument("--after", help="wait this long first: 25m, 90s, 2h")
    parser.add_argument("--until", help="shell command polled until it exits 0")
    parser.add_argument("--until-file", help="path polled until it exists")
    parser.add_argument("--timeout", default="12h", help="give up after this long (default 12h)")
    args = parser.parse_args()

    deadline = time.time() + min(parse_duration(args.timeout), MAX_WAIT_SECONDS)

    if args.after:
        target = time.time() + parse_duration(args.after)
        while time.time() < target:
            time.sleep(min(POLL_SECONDS, max(1, target - time.time())))
        emit(args.text, "timer")
        return 0

    if args.until_file:
        while time.time() < deadline:
            if os.path.exists(args.until_file):
                emit(args.text, "file")
                return 0
            time.sleep(POLL_SECONDS)
        emit("gave up waiting: " + args.text, "timeout")
        return 1

    if args.until:
        # shell=True is the feature here, not an oversight: --until takes a shell
        # condition. The only caller is the local agent, which already holds a shell
        # under the active profile, so this grants nothing it did not already have.
        while time.time() < deadline:
            try:
                done = subprocess.call(args.until, shell=True,
                                       stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL) == 0
            except Exception:
                done = False
            if done:
                emit(args.text, "condition")
                return 0
            time.sleep(POLL_SECONDS)
        emit("gave up waiting: " + args.text, "timeout")
        return 1

    emit(args.text, "now")
    return 0


if __name__ == "__main__":
    sys.exit(main())
