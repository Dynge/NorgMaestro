using System.Text.RegularExpressions;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal sealed partial class CreateNoteFromLinkTextActionProvider : ICodeActionProvider
{
    public bool CanHandle(CodeActionContext context)
    {
        Match match = EmptyTargetLinkRegex().Match(context.Line);
        if (match.Success is false)
        {
            return false;
        }

        TextRange titleRange = new()
        {
            Start = new() { Line = context.Cursor.Line, Character = (uint)match.Groups["Title"].Index },
            End = new()
            {
                Line = context.Cursor.Line,
                Character = (uint)(match.Groups["Title"].Index + match.Groups["Title"].Length),
            },
        };
        return CodeActionHelpers.IsWithinRange(context.Cursor, titleRange);
    }

    public IEnumerable<CodeAction> Build(CodeActionContext context)
    {
        Match match = EmptyTargetLinkRegex().Match(context.Line);
        if (match.Success is false)
        {
            yield break;
        }

        string title = match.Groups["Title"].Value;
        yield return new()
        {
            Title = "Create note from link text",
            Kind = CodeActionKind.QuickFix,
            Command = new()
            {
                Title = "Create note from link text",
                Command = CodeActionHandler.CreateNoteFromLinkTextCommand,
                Arguments =
                [
                    context.SourceUri.LocalPath,
                    context.Cursor.Line,
                    (uint)match.Index,
                    (uint)(match.Index + match.Length),
                    title,
                ],
            },
        };
    }

    [GeneratedRegex(@"\{\}\[(?<Title>[^\]]+)\]")]
    private static partial Regex EmptyTargetLinkRegex();
}
