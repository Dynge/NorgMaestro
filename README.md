# NorgMaestro

Language Server Protocol for norg files written in C#.

## To-dos

- The shutdown does not properly sent information to Neovim to trigger the LspDetach event.
  Double check the LSP specs to see if I need to send a specific message to the client upon a shutdown.
