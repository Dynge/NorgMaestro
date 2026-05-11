# Simplicity Spec 009: Parser metadata index loop

## Problem

`NorgParser.GetMetadata` uses `StringReader` over `string.Join` content and mixes sync/async line reads, which makes flow hard to follow.

## Goal

Iterate metadata with a direct index-based loop over `string[]` content.

## Plan

1. Replace `StringReader` traversal with index-based traversal.
2. Keep metadata boundary behavior (`@document.meta` to `@end`) unchanged.
3. Keep categories parsing behavior unchanged.

## Expected Impact

- Clearer parser control flow.
- No mixed read APIs.
- Same metadata parse semantics.

## Validation

- `dotnet test` must pass unchanged.
