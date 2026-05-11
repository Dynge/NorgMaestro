# Performance Spec 003: References handler uses in-memory document content

## Problem

`ReferencesHandler` reads a source line from disk for every request, even when the document is already loaded in `LanguageServerState`. This adds avoidable file I/O in a hot request path.

## Goal

Resolve the current line for `ParseLink` from state-backed content first, and only fall back to disk when the document is not present in memory.

## Plan

1. Add a helper in `ReferencesHandler` that reads the line from `Document.Content` when available.
2. Keep existing `FileUtil.ReadRange` fallback for safety when state has no document.

## Expected Impact

- Lower per-request latency for `textDocument/references`.
- Less disk churn while navigating references in open workspaces.

## Validation

- Add regression test proving references still resolve when backing file is deleted after state load.
- `dotnet test` must pass unchanged.
