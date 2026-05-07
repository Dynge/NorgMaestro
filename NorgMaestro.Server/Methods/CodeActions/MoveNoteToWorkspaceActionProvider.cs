using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal sealed class MoveNoteToWorkspaceActionProvider : ICodeActionProvider
{
    public bool CanHandle(CodeActionContext context)
    {
        return context.State.TryGetTitleTarget(context.SourceUri, context.Cursor, out _)
            && context.State.Workspaces.Count > 0;
    }

    public IEnumerable<CodeAction> Build(CodeActionContext context)
    {
        foreach ((string name, _) in context.State.Workspaces)
        {
            yield return new()
            {
                Title = $"Move note to workspace '{name}'",
                Kind = CodeActionKind.RefactorRewrite,
                Command = new()
                {
                    Title = $"Move note to workspace '{name}'",
                    Command = CodeActionHandler.MoveNoteToWorkspaceCommand,
                    Arguments = [context.SourceUri.LocalPath, name],
                },
            };
        }
    }
}
