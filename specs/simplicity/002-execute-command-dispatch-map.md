# Simplicity Spec 002: Execute command dispatch map

## Problem

`ExecuteCommandHandler` uses a long `switch` with repeated diagnostics publishing calls in each branch. This makes command registration and flow harder to scan.

## Goal

Represent command handling as a command-to-function map so the handler reads as dispatch + shared post-step.

## Plan

1. Add a small dispatch map from command string to async handler function.
2. Replace `switch` with map lookup.
3. Keep unknown-command error behavior unchanged.
4. Publish diagnostics once after successful command dispatch.

## Expected Impact

- Smaller and clearer command handler control flow.
- Easier extension when adding future commands.
- No behavioral change to success/error responses.

## Validation

- Existing execute-command tests remain green.
- `dotnet test` must pass unchanged.
