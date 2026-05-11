# Simplicity Spec 010: Parser extract categories metadata parser

## Problem

Categories metadata parsing is embedded inline in `GetMetadata`, making the method long and mixing high-level flow with low-level category block details.

## Goal

Extract category block parsing into a dedicated helper that returns parsed categories and next loop index.

## Plan

1. Add a helper to parse categories from current metadata line.
2. Return both parsed categories and consumed line index.
3. Replace inline categories block in `GetMetadata` with one helper call.

## Expected Impact

- Cleaner `GetMetadata` control flow.
- Easier to reason about categories behavior in isolation.
- No change in parse semantics.

## Validation

- `dotnet test` must pass unchanged.
