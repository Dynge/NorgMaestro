using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal sealed class NormalizeToWorkspaceAliasActionProvider : ICodeActionProvider
{
    public bool CanHandle(CodeActionContext context)
    {
        return context.LinkAtCursor is not null && context.State.Workspaces.Count > 0;
    }

    public IEnumerable<CodeAction> Build(CodeActionContext context)
    {
        if (context.LinkAtCursor is null)
        {
            yield break;
        }

        Uri targetUri = context.State.ResolveLinkUri(context.LinkAtCursor);
        string fullPath = Path.GetFullPath(targetUri.LocalPath);
        foreach ((string workspaceName, Uri workspaceUri) in context.State.Workspaces)
        {
            string workspaceRoot = Path.GetFullPath(workspaceUri.LocalPath);
            if (fullPath.StartsWith(workspaceRoot, StringComparison.OrdinalIgnoreCase) is false)
            {
                continue;
            }

            string relative = Path.GetRelativePath(workspaceRoot, fullPath);
            if (relative.EndsWith(".norg", StringComparison.OrdinalIgnoreCase))
            {
                relative = relative[..^5];
            }

            string title = context.LinkAtCursor.LinkText;
            yield return new()
            {
                Title = $"Use workspace alias '${workspaceName}'",
                Kind = CodeActionKind.RefactorRewrite,
                Edit = new()
                {
                    Changes = new Dictionary<string, TextEdit[]>
                    {
                        [context.SourceUri.AbsoluteUri] =
                        [
                            new()
                            {
                                Range = context.LinkAtCursor.AbsoluteRange,
                                NewText = CodeActionHelpers.CanonicalLink($"${workspaceName}/{relative}", title),
                            },
                        ],
                    },
                },
            };

            yield break;
        }
    }
}
