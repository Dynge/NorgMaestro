vim.lsp.set_log_level("debug")
if vim.fn.has("nvim-0.5.1") == 1 then
	require("vim.lsp.log").set_format_func(vim.inspect)
end

local client_id = vim.lsp.start_client({
	name = "NeorgLsp",
	filetypes = { "neorg" },
	cmd = {
		"/home/michael/.cache/pypoetry/virtualenvs/neorg-lsp-mE_f2NLl-py3.11/bin/python",
		"neorg_lsp/__init__.py",
	},
	root_dir = "/home/michael/git/neorg-lsp/",
	trace = "verbose",
})

if client_id ~= nil then
	local neorg_buffer = 0
	vim.lsp.buf_attach_client(neorg_buffer, client_id)
end
