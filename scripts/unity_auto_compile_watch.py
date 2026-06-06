#!/usr/bin/env python3
import argparse
import hashlib
import json
import os
import re
import subprocess
import sys
import time
from datetime import datetime
from pathlib import Path

WATCH_EXTS = {".cs", ".asmdef", ".json", ".shader", ".cginc", ".hlsl"}
INCLUDE_NAMES = {"manifest.json", "packages-lock.json"}
EXCLUDE_PARTS = {"/Library/", "/Temp/", "/Logs/", "/Obj/", "/Builds/", "/.git/"}

ERROR_PATTERNS = [
    re.compile(r"error\\s+CS\\d{4}", re.IGNORECASE),
    re.compile(r"Scripts have compiler errors", re.IGNORECASE),
    re.compile(r"Aborting batchmode due to failure", re.IGNORECASE),
    re.compile(r"Fatal Error!", re.IGNORECASE),
    re.compile(r"another Unity instance is running", re.IGNORECASE),
]

SUCCESS_PATTERNS = [
    re.compile(r"Tundra build success", re.IGNORECASE),
    re.compile(r"successfully reloaded assembly", re.IGNORECASE),
]


def now():
    return datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%SZ")


def should_watch(path: Path) -> bool:
    sp = str(path)
    if any(part in sp for part in EXCLUDE_PARTS):
        return False
    if path.name in INCLUDE_NAMES:
        return True
    return path.suffix.lower() in WATCH_EXTS


def file_fingerprint(path: Path) -> str:
    st = path.stat()
    return f"{path}:{int(st.st_mtime_ns)}:{st.st_size}"


def compute_snapshot(project: Path) -> str:
    rows = []
    for p in project.rglob("*"):
        if not p.is_file():
            continue
        if should_watch(p):
            rows.append(file_fingerprint(p))
    rows.sort()
    return hashlib.sha256("\n".join(rows).encode("utf-8")).hexdigest()


def run_refresh(unity: str, project: Path, log_file: Path) -> int:
    cmd = [
        unity,
        "-projectPath", str(project),
        "-executeMethod", "AutoRefresher.RefreshAssets",
        "-batchmode",
        "-quit",
        "-logFile", str(log_file),
    ]
    proc = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)
    return proc.returncode


def parse_log(log_file: Path, rc: int):
    if not log_file.exists():
        return {"status": "error", "errors": ["log file missing"], "matches": []}

    text = log_file.read_text(errors="ignore")
    lines = text.splitlines()

    matches = []
    for i, line in enumerate(lines, 1):
        if any(p.search(line) for p in ERROR_PATTERNS):
            matches.append({"line": i, "text": line.strip()})

    success = any(p.search(text) for p in SUCCESS_PATTERNS)
    if matches or rc != 0:
        status = "error"
    elif success or rc == 0:
        status = "ok"
    else:
        status = "unknown"

    return {"status": status, "errors": [m["text"] for m in matches], "matches": matches}


def write_status(project: Path, result: dict, log_file: Path):
    out_dir = project / "TestResults"
    out_dir.mkdir(parents=True, exist_ok=True)

    status_file = out_dir / "auto-compile-status.json"
    history_file = out_dir / "auto-compile-history.log"

    payload = {
        "timestamp": now(),
        "status": result["status"],
        "error_count": len(result["errors"]),
        "log_file": str(log_file),
        "errors": result["errors"][:50],
    }

    status_file.write_text(json.dumps(payload, indent=2) + "\n")

    short = f"[{payload['timestamp']}] status={payload['status']} errors={payload['error_count']} log={log_file}"
    with history_file.open("a", encoding="utf-8") as f:
        f.write(short + "\n")
        for m in result["matches"][:20]:
            f.write(f"  L{m['line']}: {m['text']}\n")

    print(short)
    if result["errors"]:
        for e in result["errors"][:8]:
            print(f"  - {e}")


def main():
    parser = argparse.ArgumentParser(description="Watch Unity project and auto-run compile refresh on source changes")
    parser.add_argument("--project", default=".")
    parser.add_argument("--unity", default="/Applications/Unity/Hub/Editor/2022.3.4f1/Unity.app/Contents/MacOS/Unity")
    parser.add_argument("--interval", type=float, default=3.0)
    parser.add_argument("--once", action="store_true")
    parser.add_argument("--log", default="/tmp/unity_auto_compile.log")
    parser.add_argument("--close-unity-if-open", action="store_true", help="Kill existing Unity process before refresh (batchmode option 1 behavior)")
    args = parser.parse_args()

    project = Path(args.project).resolve()
    unity = args.unity
    log_file = Path(args.log)

    if not project.exists():
        print(f"Project path not found: {project}", file=sys.stderr)
        return 2
    if not Path(unity).exists():
        print(f"Unity executable not found: {unity}", file=sys.stderr)
        return 2

    print(f"[auto-watch] project={project}")
    print(f"[auto-watch] unity={unity}")
    print(f"[auto-watch] interval={args.interval}s")

    last = None
    while True:
        try:
            snap = compute_snapshot(project)
        except Exception as e:
            print(f"[auto-watch] snapshot error: {e}")
            time.sleep(args.interval)
            continue

        if snap != last:
            print(f"[auto-watch] change detected at {now()} -> running refresh")
            if args.close_unity_if_open:
                subprocess.run(["pkill", "-f", "Unity"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
                time.sleep(1.0)
            rc = run_refresh(unity, project, log_file)
            result = parse_log(log_file, rc)
            result["rc"] = rc
            write_status(project, result, log_file)
            last = snap

        if args.once:
            break
        time.sleep(args.interval)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
