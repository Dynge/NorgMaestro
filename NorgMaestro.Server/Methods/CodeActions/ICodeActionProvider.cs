using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal interface ICodeActionProvider
{
    public bool CanHandle(CodeActionContext context);
    public IEnumerable<CodeAction> Build(CodeActionContext context);
}
