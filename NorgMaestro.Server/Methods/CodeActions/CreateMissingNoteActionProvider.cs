using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal sealed class CreateMissingNoteActionProvider : ICodeActionProvider
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
                Title = "Create missing note",
                Kind = CodeActionKind.QuickFix,
                IsPreferred = true,
                Command = new()
                {
                    Title = "Create missing note",
                    Command = CodeActionHandler.CreateNoteCommand,
                    Arguments = [targetPath],
                },
            };
        }
    }
}
