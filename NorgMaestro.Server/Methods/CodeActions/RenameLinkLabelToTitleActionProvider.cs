using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal sealed class RenameLinkLabelToTitleActionProvider : ICodeActionProvider
{
    public bool CanHandle(CodeActionContext context)
    {
        return context.LinkAtCursor is not null;
    }

    public IEnumerable<CodeAction> Build(CodeActionContext context)
    {
        if (context.LinkAtCursor is null)
        {
            yield break;
        }

        Uri targetUri = context.State.ResolveLinkUri(context.LinkAtCursor);
        if (context.State.Documents.TryGetValue(targetUri, out Document? targetDoc) is false)
        {
            yield break;
        }

        string title = targetDoc.Metadata.Title?.Name ?? Path.GetFileNameWithoutExtension(targetUri.LocalPath);
        if (title == context.LinkAtCursor.LinkText)
        {
            yield break;
        }

        yield return new()
        {
            Title = "Rename link label to target title",
            Kind = CodeActionKind.RefactorRewrite,
            Edit = new()
            {
                Changes = new Dictionary<string, TextEdit[]>
                {
                    [context.SourceUri.AbsoluteUri] =
                    [new() { Range = context.LinkAtCursor.LinkTextRange, NewText = title }],
                },
            },
        };
    }
}
