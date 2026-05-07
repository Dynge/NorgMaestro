using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal sealed class FixBrokenLinkCandidateActionProvider : ICodeActionProvider
{
    public bool CanHandle(CodeActionContext context)
    {
        return CodeActionHelpers.UnresolvedDiagnostics(context).Any();
    }

    public IEnumerable<CodeAction> Build(CodeActionContext context)
    {
        foreach ((Diagnostic diagnostic, string targetPath) in CodeActionHelpers.UnresolvedDiagnostics(context))
        {
            string brokenName = Path.GetFileNameWithoutExtension(targetPath).ToLowerInvariant();
            IEnumerable<Document> candidates = context
                .State.Documents.Values.OrderByDescending(d => ScoreCandidate(d, brokenName))
                .ThenBy(d => d.Uri.LocalPath)
                .Take(3)
                .Where(d => ScoreCandidate(d, brokenName) > 0);

            foreach (Document candidate in candidates)
            {
                string id = Path.GetFileNameWithoutExtension(candidate.Uri.LocalPath);
                string title = candidate.Metadata.Title?.Name ?? id;
                yield return new()
                {
                    Title = $"Fix broken link to '{title}'",
                    Kind = CodeActionKind.QuickFix,
                    Edit = new()
                    {
                        Changes = new Dictionary<string, TextEdit[]>
                        {
                            [context.SourceUri.AbsoluteUri] =
                            [
                                new()
                                {
                                    Range = diagnostic.Range,
                                    NewText = CodeActionHelpers.CanonicalLink(id, title),
                                },
                            ],
                        },
                    },
                };
            }
        }
    }

    private static int ScoreCandidate(Document doc, string probe)
    {
        string id = Path.GetFileNameWithoutExtension(doc.Uri.LocalPath).ToLowerInvariant();
        string title = (doc.Metadata.Title?.Name ?? string.Empty).ToLowerInvariant();
        if (id == probe || title == probe)
        {
            return 100;
        }
        if (id.Contains(probe) || title.Contains(probe))
        {
            return 75;
        }

        return id.Intersect(probe).Count();
    }
}
