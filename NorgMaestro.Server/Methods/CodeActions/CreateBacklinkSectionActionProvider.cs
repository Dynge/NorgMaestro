using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal sealed class CreateBacklinkSectionActionProvider : ICodeActionProvider
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
        if (context.State.Documents.ContainsKey(targetUri) is false)
        {
            yield break;
        }

        yield return new()
        {
            Title = "Create backlink section in target note",
            Kind = CodeActionKind.RefactorRewrite,
            Command = new()
            {
                Title = "Create backlink section in target note",
                Command = CodeActionHandler.CreateBacklinkSectionCommand,
                Arguments = [context.SourceUri.LocalPath, targetUri.LocalPath],
            },
        };
    }
}
