# Simplicity Spec 007: Incoming calls flow helpers

## Problem

`IncomingCallsHandler` packs grouping, document lookup, response shaping, and ordering into one LINQ chain. Compact, but harder to step through and extend.

## Goal

Split incoming-call response building into small helper methods with explicit intent.

## Plan

1. Extract response-building logic into dedicated private helpers.
2. Keep grouping by source URI and output ordering unchanged.
3. Keep payload shape unchanged.

## Expected Impact

- Easier to read and modify incoming-call behavior.
- Clear composable units for grouping and mapping.

## Validation

- Existing incoming-calls tests remain green.
- `dotnet test` must pass unchanged.
