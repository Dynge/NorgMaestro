# Performance Spec 002: Completion allocation trimming

## Problem

`CompletionHandler` allocates many short-lived `CompletionItem` objects while collecting category completions, and repeatedly builds identical fallback `TextEdit` values.

## Goal

Reduce allocations in completion requests without changing completion semantics.

## Plan

1. Collect category labels in `HashSet<string>` first, then materialize `CompletionItem` once per unique label.
2. Reuse one fallback `TextEdit` builder inside `GetLinkEditRange` instead of constructing the same object shape in three branches.

## Expected Impact

- Fewer temporary objects per completion request.
- Less GC churn during typing in large workspaces.

## Validation

- `dotnet test` must pass unchanged.
