# Simplicity Spec 003: Shared document line reader

## Problem

Multiple handlers fetch a single line at cursor position with different patterns (`FileUtil.ReadRange`, direct file reads, state lookup). This duplicates logic and makes behavior harder to reason about.

## Goal

Use one small helper for line retrieval that prefers in-memory document content and falls back to disk.

## Plan

1. Add `DocumentLineReader` helper in methods layer.
2. Replace local line-reading code in `Definition`, `Hover`, `PrepareRename`, `Rename`, and `References` handlers.
3. Remove now-duplicated private line helper code.

## Expected Impact

- Consistent line retrieval behavior across cursor-based handlers.
- Smaller handlers with less plumbing noise.
- Better readability for link/title resolution flows.

## Validation

- Existing handler tests remain green.
- `dotnet test` must pass unchanged.
