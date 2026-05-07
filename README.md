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
  - incoming links (`callHierarchy/incomingCalls`)
  - outgoing links (`callHierarchy/outgoingCalls`)

### Completion

- Link completion inside canonical link targets.
- Category completion from categories in other loaded notes.
- Trigger characters: `æ`, `ø`, `å`, `{`.

### Diagnostics

- Warning diagnostics for unresolved note links.
- Diagnostics published on initialize, save, and command execution updates.

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

## Configuration values you can supply

NorgMaestro currently uses standard LSP initialize parameters. It does **not** define custom `settings` or custom `initializationOptions` yet.

Supply these in your client initialize payload:

- `rootUri` (`string`, URI): workspace root used for indexing and `$/...` link resolution.
- `workspaceFolders` (`[{ uri, name }]`): optional multi-root folders. Folder `name` is used as workspace alias in `$name/...` links.
- `processId` (`number | null`): optional LSP process id.
- `capabilities` (`object`): normal client capability advertisement.

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

Early version (`0.1`) targeting `.NET 8`.
