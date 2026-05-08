# NorgMaestro

NorgMaestro is a Language Server Protocol (LSP) server for `.norg` files, written in C#.

It is built for note-heavy workflows, especially a Zettelkasten in Norg format: you can jump between notes, track backlinks, resolve broken links, and create new linked notes without leaving your editor.

## Why for Zettelkasten

- Fast note graph navigation (`definition`, `references`, call hierarchy).
- Link-aware diagnostics for unresolved note targets.
- Refactors and quick fixes for note links and titles.
- Code actions that create and connect notes as you write.

## Features

### Core language features

- **Go to definition** for note links (`textDocument/definition`).
- **Find references** across workspace (`textDocument/references`).
- **Rename** note titles and linked labels (`textDocument/rename` + `textDocument/prepareRename`).
- **Hover preview** of target note content (`textDocument/hover`).
- **Document symbols** from Norg headings (`textDocument/documentSymbol`).
- **Workspace symbols** over note metadata (`workspace/symbol`).
- **Document links** for parsed Norg links (`textDocument/documentLink`).
- **Call hierarchy** over note graph:
  - prepare item (`textDocument/prepareCallHierarchy`, canonical `file:///` URI)
  - incoming links (`callHierarchy/incomingCalls`)
  - outgoing links (`callHierarchy/outgoingCalls`)

### Text sync lifecycle

- `textDocument/didOpen`: indexes opened document and publishes diagnostics.
  - If `textDocument.text` is present, payload text is source of truth (even when file exists on disk).
  - Missing-on-disk URIs are still tracked in memory when payload text is provided.
- `textDocument/didChange`: updates in-memory content, reparses metadata/references, publishes diagnostics.
  - Supports both full-text replacement and incremental range edits.
  - Incremental edits apply in order; invalid ranges are ignored (no crash).
- `textDocument/didClose`: removes document from in-memory state, prunes outbound references, publishes diagnostics.
- `textDocument/didSave`: refreshes from disk and publishes diagnostics.

### Completion

- Link completion inside canonical link targets.
- Category completion from categories in other loaded notes.
- Trigger characters: `æ`, `ø`, `å`, `{`.

### Diagnostics

- Warning diagnostics for unresolved note links.
- Diagnostics published on initialize, open, change, close, save, and command execution updates.

### Code actions and commands

NorgMaestro exposes quick fixes/refactors and command handlers for:

- Create missing note.
- Create missing note and open it.
- Fix broken link to best candidates.
- Rename link label to target note title.
- Convert token to canonical note link.
- Create backlink section in target note.
- Extract selection to new note.
- Move note to another workspace folder.
- Normalize link to workspace alias format (`$workspace/path`).
- Create note from link text and rewrite source link.

`workspace/executeCommand` behavior:

- Known commands return success.
- Unknown commands return JSON-RPC error `-32601` with command name in message.

Registered execute commands:

- `norgmaestro.createNote`
- `norgmaestro.createNoteAndOpen`
- `norgmaestro.createBacklinkSection`
- `norgmaestro.extractSelectionToNote`
- `norgmaestro.moveNoteToWorkspace`
- `norgmaestro.createNoteFromLinkText`

## Link path behavior

NorgMaestro resolves note links with these path forms:

- Relative path (from current note directory): `{:folder/note:}`
- Workspace-root path: `{:$/folder/note:}`
- Named workspace alias path: `{:$gtd/project/note:}`
- Home-relative path: `{:~/notes/note:}`
- Absolute filesystem path: `{:/full/path/to/note:}`

If `.norg` extension is omitted, NorgMaestro adds it automatically.

## Protocol guarantees

- List-returning handlers return arrays (never `null`) for empty results.
  - `textDocument/definition` => `[]` when no target.
  - `textDocument/references` => `[]` when no references.
- `textDocument/documentSymbol` always returns non-null `children` arrays (`[]` for leaf symbols).
- JSON-RPC success responses include `"result": null` when payload is intentionally empty.
- Notification handlers (`didOpen`, `didChange`, `didClose`, `didSave`) emit no response object.
- RPC payload serialization omits properties whose values are `null`.

## Workspace symbol behavior

- `workspace/symbol` query matching is case-insensitive.
- Kind-filter form like `[file]` is case-insensitive.
- Results are deterministic: sorted by symbol name, then URI.

## Configuration values you can supply

NorgMaestro uses standard LSP initialize parameters and supports targeted custom `initializationOptions`.

Supply these in your client initialize payload:

- `rootUri` (`string`, URI): workspace root used for indexing and `$/...` link resolution.
- `workspaceFolders` (`[{ uri, name }]`): optional multi-root folders. Folder `name` is used as workspace alias in `$name/...` links.
- `processId` (`number | null`): optional LSP process id.
- `capabilities` (`object`): normal client capability advertisement.
- `initializationOptions.diagnostics.unresolvedLinkSeverity` (`string`): optional severity for unresolved link diagnostics. Supported values: `error`, `warning`, `information` (`info`), `hint`. Default: `warning`.

### Example (Neovim lspconfig)

```lua
require('lspconfig').norgmaestro.setup {
  cmd = { 'dotnet', '/absolute/path/to/NorgMaestro.Server.dll' },
  filetypes = { 'norg' },
  root_dir = require('lspconfig.util').root_pattern('.git', '.jj'),
}
```

`workspaceFolders` are provided by the client automatically for multi-root workspaces.

## Build and run

```bash
make build
make test
dotnet run --project NorgMaestro.Server
```

## Status

Early version (`0.1`) targeting `.NET 9`.
