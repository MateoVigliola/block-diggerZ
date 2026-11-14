#!/usr/bin/env bash
# Runs EditMode test classes in the open Editor and reports a result that can't be stale.
#
# usage: Tools/run-editmode-tests.sh <TestClassOrNamePart> [more ...]
#   Tools/run-editmode-tests.sh PieceStoreTests PiecePreviewLayoutTests
#
# Exit codes: 0 all passed, 1 failures or no matching tests, 2 bad usage or Editor unreachable,
# 3 Editor is in Play mode (the user's playtest): nothing was queued.
#
# Why: a test run started while the Editor is playing waits silently until Play mode ends. This script
# checks first and stops with a clear message instead. Each Editor call has a short timeout.
set -uo pipefail
[ $# -ge 1 ] || { echo "usage: $0 <TestClassOrNamePart> [more ...]" >&2; exit 2; }

CALL_TIMEOUT="${CALL_TIMEOUT:-60}"
U=(unity command --caller plugin --skill run-editmode-tests)
[ -n "${PROJECT:-}" ] && U+=(--project-path "$PROJECT")
export UNITY_NO_PAGER=1

play_state() {
  timeout "$CALL_TIMEOUT" "${U[@]}" editor status 2>/dev/null | sed -n 's/^playMode: *//p' | head -1
}

state=$(play_state)
if [ -z "$state" ]; then
  echo "BLOCKED: the Editor did not answer within ${CALL_TIMEOUT}s (closed, compiling or in Safe Mode)." >&2
  exit 2
fi
if [ "$state" != "stopped" ]; then
  echo "BLOCKED: the Editor is in Play mode ($state), likely a playtest. No tests were queued." >&2
  echo "Exit Play mode, then run again." >&2
  exit 3
fi

overall=0
for name in "$@"; do
  filter="$name"

  full=$(timeout "$CALL_TIMEOUT" "${U[@]}" test run -- --mode editmode --filter "$filter" --filter-type TestName 2>/dev/null)
  code=$?
  if [ $code -ne 0 ] && [ $code -ne 124 ]; then
    echo "NO MATCH $name: the Editor ran nothing for that name (typo?)." >&2
    overall=1
    continue
  fi
  if [ $code -eq 124 ]; then
    state=$(play_state)
    if [ "$state" = "playing" ] || [ "$state" = "paused" ]; then
      echo "BLOCKED: Play mode started during the run ($state). Cancelling." >&2
      timeout "$CALL_TIMEOUT" "${U[@]}" test cancel >/dev/null 2>&1
      exit 3
    fi
    echo "FAILED TO RUN $name: no answer within ${CALL_TIMEOUT}s." >&2
    timeout "$CALL_TIMEOUT" "${U[@]}" test cancel >/dev/null 2>&1
    overall=1
    continue
  fi

  rows=$(printf '%s\n' "$full" | grep -F "$name" | grep -E '[[:space:]](Passed|Failed|Skipped|Inconclusive)[[:space:]]')
  total=$(printf '%s' "$rows" | grep -c . )
  passed=$(printf '%s\n' "$rows" | grep -cE '[[:space:]]Passed[[:space:]]')
  if [ "$total" -eq 0 ]; then
    echo "NO MATCH $name: no tests with that name ran (typo?)." >&2
    overall=1
    continue
  fi

  echo "$name: $passed/$total passed"
  if [ "$passed" -ne "$total" ]; then
    printf '%s\n' "$rows" | grep -vE '[[:space:]]Passed[[:space:]]'
    overall=1
  fi
done
exit $overall
