vim.lsp.set_log_level("debug")
if vim.fn.has("nvim-0.5.1") == 1 then
	require("vim.lsp.log").set_format_func(vim.inspect)
end

local csharp_client = {
	name = "NeorgLsp2",
	filetypes = { "norg" },
	cmd = {
		"/home/michael/git/neorg-lsp/bin/Debug/net8.0/CeorgLsp",
	},
	root_dir = "/home/michael/notes/",
	trace = "verbose",
}

local client_id = vim.lsp.start_client(csharp_client)
print(client_id)

if client_id ~= nil then
	local neorg_buffer = 26
	vim.lsp.buf_attach_client(neorg_buffer, client_id)
end
