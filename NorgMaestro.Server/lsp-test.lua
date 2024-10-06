vim.lsp.set_log_level("debug")
if vim.fn.has("nvim-0.5.1") == 1 then
	require("vim.lsp.log").set_format_func(vim.inspect)
end

local csharp_client = {
	name = "NorgMaestro",
	filetypes = { "norg" },
	cmd = { "NorgMaestro.Server" },
	root_dir = vim.env.HOME .. "/notes/",
	trace = "verbose",
}

local client_id = vim.lsp.start_client(csharp_client)
vim.print(client_id)

if client_id ~= nil then
	local neorg_buffer = 1
	local didattach =vim.lsp.buf_attach_client(neorg_buffer, client_id)
    vim.print(didattach)
end
