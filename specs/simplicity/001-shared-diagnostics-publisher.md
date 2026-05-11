# Simplicity Spec 001: Shared diagnostics publisher for LSP handlers

## Problem

Diagnostics publishing logic is duplicated across multiple handlers (`DidOpen`, `DidChange`, `DidClose`, `DidSave`, `Initialize`, `ExecuteCommand`). The repeated loop makes handlers longer and harder to compose.

## Goal

Extract one reusable diagnostics publisher so handlers focus on request flow, not notification plumbing.

## Plan

1. Add a `DiagnosticsPublisher` helper that owns the `GetDiagnostics` loop.
2. Provide sync and async publish entry points to preserve existing call patterns.
3. Replace per-handler private `PublishDiagnostics` methods with helper calls.

## Expected Impact

- Less duplicated code in handler classes.
- Clearer request-handler intent and easier future composition.
- No behavior changes in diagnostics payloads.

## Validation

- Existing diagnostics-related tests remain green.
- `dotnet test` must pass unchanged.
