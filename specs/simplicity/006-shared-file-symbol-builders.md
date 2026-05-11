# Simplicity Spec 006: Shared file symbol builders

## Problem

Handlers repeat file-symbol object construction and zero-range initialization in several places (`IncomingCalls`, `OutgoingCalls`, `PrepareCallHierarchy`, `WorkspaceSymbol`).

## Goal

Provide shared builders for common file-symbol shapes so handlers express intent, not object boilerplate.

## Plan

1. Add `SymbolBuilders` helper with reusable methods for zero range, call hierarchy file item, and workspace file symbol.
2. Replace duplicated object initialization in affected handlers.
3. Keep response payload shape unchanged.

## Expected Impact

- Less duplicate boilerplate in handlers.
- More composable symbol construction points for future handlers.
- Improved readability of request handling flow.

## Validation

- Existing call hierarchy and workspace symbol tests remain green.
- `dotnet test` must pass unchanged.
