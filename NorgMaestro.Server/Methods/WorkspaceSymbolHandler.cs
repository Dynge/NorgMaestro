using System.Text.RegularExpressions;
using NorgMaestro.Rpc;

namespace NorgMaestro.Methods;

public partial class WorkspaceSymbolHandler : IMessageHandler
{
    public required RpcMessage Request { get; init; }
    public required LanguageServerState State { get; init; }

    [GeneratedRegex(@"^\[(\w+)\]")]
    private static partial Regex KindRegex();

    public Response? HandleRequest()
    {
        WorkspaceSymbolRequest workspaceRequest = WorkspaceSymbolRequest.From(Request);
        List<WorkspaceSymbol> symbols = [];
        Func<WorkspaceSymbol, bool> filter = workspaceRequest.Params.Query switch
        {
            string q when KindRegex().IsMatch(q)
                => (WorkspaceSymbol wsSymbol) =>
                {
                    var symbolMatch = KindRegex().Match(q).Groups[1].Value;
                    if (Enum.TryParse<SymbolKind>(symbolMatch, out var kind) is false)
                    {
                        return false;
                    }
                    return wsSymbol.Kind == kind;
                },
            string q
                => (WorkspaceSymbol wsSymbol) =>
                {
                    return wsSymbol.Name.Contains(q);
                },
        };
        foreach (Document doc in State.Documents.Values)
        {
            var workspaceName = doc.Metadata.Title?.Name ?? "";
            if (doc.Metadata.Description is not null)
            {
                workspaceName += $": {doc.Metadata.Description.Name}";
            }
            var symbol = new WorkspaceSymbol()
            {
                Location = new()
                {
                    Range = new()
                    {
                        Start = new() { Line = 0, Character = 0 },
                        End = new() { Line = 0, Character = 0 }
                    },
                    Uri = doc.Uri.AbsoluteUri,
                },
                Kind = SymbolKind.File,
                Name = workspaceName
            };

            if (filter(symbol) is false)
            {
                continue;
            }
            symbols.Add(symbol);
        }
        return Response.OfSuccess(workspaceRequest.Id, symbols.ToArray());
    }
}
