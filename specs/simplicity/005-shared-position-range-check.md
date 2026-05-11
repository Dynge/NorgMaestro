# Simplicity Spec 005: Shared position-in-range check

## Problem

Position-range boundary logic exists in both `LanguageServerState` and `PrepareRenameHandler`. Duplicate boundary code increases maintenance risk.

## Goal

Keep one shared range check implementation and reuse it where needed.

## Plan

1. Expose a single reusable range check method on `LanguageServerState`.
2. Replace `PrepareRenameHandler` local copy with the shared method.

## Expected Impact

- Removes duplicate boundary logic.
- Improves consistency for cursor range decisions.

## Validation

- Existing prepare-rename tests remain green.
- `dotnet test` must pass unchanged.
