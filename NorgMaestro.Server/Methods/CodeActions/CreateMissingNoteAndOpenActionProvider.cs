using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal sealed class CreateMissingNoteAndOpenActionProvider : ICodeActionProvider
{
    public bool CanHandle(CodeActionContext context)
    {
        return CodeActionHelpers.UnresolvedDiagnostics(context).Any();
    }

    public IEnumerable<CodeAction> Build(CodeActionContext context)
    {
        foreach ((_, string targetPath) in CodeActionHelpers.UnresolvedDiagnostics(context))
        {
            yield return new()
            {
                Title = "Create missing note and open",
                Kind = CodeActionKind.QuickFix,
                Command = new()
                {
                    Title = "Create missing note and open",
                    Command = CodeActionHandler.CreateNoteAndOpenCommand,
                    Arguments = [targetPath],
                },
            };
        }
    }
}
