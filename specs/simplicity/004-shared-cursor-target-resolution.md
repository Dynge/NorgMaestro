# Simplicity Spec 004: Shared cursor target resolution

## Problem

Handlers that navigate or rename notes repeat the same flow: parse link at cursor, resolve link target, then fall back to title target. This logic appears in several files and is easy to drift.

## Goal

Move cursor target resolution into `LanguageServerState` so handlers call one method for the same behavior.

## Plan

1. Add `TryResolveTargetUriAtPosition` to `LanguageServerState`.
2. Reuse this method from `Definition`, `References`, and `Rename` handlers.
3. Keep request success/error payload behavior unchanged.

## Expected Impact

- Smaller handlers with clearer intent.
- One authoritative implementation for cursor-to-target resolution.
- Easier future evolution for link/title targeting rules.

## Validation

- Existing definition/reference/rename tests remain green.
- `dotnet test` must pass unchanged.
