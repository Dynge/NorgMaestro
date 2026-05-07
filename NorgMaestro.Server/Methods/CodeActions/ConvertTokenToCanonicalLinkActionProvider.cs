using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal sealed class ConvertTokenToCanonicalLinkActionProvider : ICodeActionProvider
{
    public bool CanHandle(CodeActionContext context)
    {
        return context.LinkAtCursor is null;
    }

    public IEnumerable<CodeAction> Build(CodeActionContext context)
    {
        string token = CodeActionHelpers.ExtractTokenAtCursor(context.Line, (int)context.Cursor.Character);
        if (string.IsNullOrWhiteSpace(token) || token.Contains("{:"))
        {
            yield break;
        }

        string normalized = token.EndsWith(".norg", StringComparison.OrdinalIgnoreCase)
            ? token[..^5]
            : token;
        if (normalized.Length < 3)
        {
            yield break;
        }

        TextRange tokenRange = CodeActionHelpers.GetTokenRange(context.Line, context.Cursor);
        yield return new()
        {
            Title = "Convert token to canonical note link",
            Kind = CodeActionKind.RefactorRewrite,
            Edit = new()
            {
                Changes = new Dictionary<string, TextEdit[]>
                {
                    [context.SourceUri.AbsoluteUri] =
                    [
                        new()
                        {
                            Range = tokenRange,
                            NewText = CodeActionHelpers.CanonicalLink(normalized, normalized),
                        },
                    ],
                },
            },
        };
    }
}
