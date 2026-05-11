# Performance Spec 004: Outgoing calls read from reference index

## Problem

`OutgoingCallsHandler` reparses the source document links for each `callHierarchy/outgoingCalls` request. Repeated parsing adds CPU overhead during navigation.

## Goal

Build outgoing call results from `LanguageServerState.References`, which is already maintained during document updates.

## Plan

1. Replace per-request `NorgParser.ParseLinks` call with iteration over `_state.References`.
2. Match entries by `reference.Location.Uri == sourceUri.AbsoluteUri`.
3. Keep response shape unchanged (`FromRanges` with link range; unresolved targets skipped).

## Expected Impact

- Lower CPU per outgoing call request.
- Reuse already-indexed link graph instead of repeated parse work.

## Validation

- Existing outgoing calls tests remain green.
- `dotnet test` must pass unchanged.
