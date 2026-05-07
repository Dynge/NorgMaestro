using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal sealed class ExtractSelectionToNoteActionProvider : ICodeActionProvider
{
    public bool CanHandle(CodeActionContext context)
    {
        TextRange range = context.Request.Params.Range;
        return range.Start.Line == range.End.Line && range.Start.Character < range.End.Character;
    }

    public IEnumerable<CodeAction> Build(CodeActionContext context)
    {
        TextRange range = context.Request.Params.Range;
        yield return new()
        {
            Title = "Extract selection to new note",
            Kind = CodeActionKind.RefactorRewrite,
            Command = new()
            {
                Title = "Extract selection to new note",
                Command = CodeActionHandler.ExtractSelectionToNoteCommand,
                Arguments =
                [
                    context.SourceUri.LocalPath,
                    range.Start.Line,
                    range.Start.Character,
                    range.End.Character,
                ],
            },
        };
    }
}
