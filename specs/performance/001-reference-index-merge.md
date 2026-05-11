# Performance Spec 001: Reference index merge without set cloning

## Problem

`LanguageServerState.UpdateDocument` and `NorgParser.GetReferences` rebuild hash sets with spread copies while indexing links. On large workspaces this causes avoidable allocations and extra hashing.

## Goal

Keep behavior same while reducing allocations and repeated dictionary lookups during reference indexing.

## Plan

1. In `LanguageServerState.UpdateDocument`, replace spread-copy merge with `HashSet.UnionWith` on existing sets.
2. In `NorgParser.GetReferences`, compute link URI once per link and mutate existing set with `Add`.

## Expected Impact

- Fewer temporary `HashSet<ReferenceLocation>` allocations.
- Lower GC pressure during initialize/didOpen/didChange indexing.
- Faster reference map merges as note count grows.

## Validation

- `dotnet test` must pass unchanged.
