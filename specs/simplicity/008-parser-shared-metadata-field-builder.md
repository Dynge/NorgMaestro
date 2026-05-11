# Simplicity Spec 008: Parser shared metadata field builder

## Problem

`NorgParser.GetMetadata` repeats the same match-and-build block for scalar metadata fields (`title`, `description`, `authors`, `created`, `updated`, `version`). Duplication hides parser intent.

## Goal

Use one small helper to build scalar `MetaField` values from regex matches.

## Plan

1. Add helper to parse a metadata field from one regex and one line.
2. Replace duplicated scalar-field switch branches with helper calls.
3. Keep categories handling unchanged in this unit.

## Expected Impact

- Smaller metadata parser core loop.
- Less repeated object construction code.
- Same parsing behavior and ranges.

## Validation

- `dotnet test` must pass unchanged.
