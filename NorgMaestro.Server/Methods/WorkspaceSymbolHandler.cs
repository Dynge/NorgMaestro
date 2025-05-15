using System.Text.RegularExpressions;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public partial class WorkspaceSymbolHandler(LanguageServerState state, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    [GeneratedRegex(@"^\[(\w+)\]")]
    private static partial Regex KindRegex();

    public Task<Response?> HandleRequest()
    {
        WorkspaceSymbolRequest workspaceRequest = WorkspaceSymbolRequest.From(_request);
        List<WorkspaceSymbol> symbols = [];
        Func<WorkspaceSymbol, bool> filter = workspaceRequest.Params.Query switch
        {
            string q when KindRegex().IsMatch(q) => wsSymbol =>
            {
                var symbolMatch = KindRegex().Match(q).Groups[1].Value;
                if (Enum.TryParse<SymbolKind>(symbolMatch, out var kind) is false)
                {
                    return false;
                }
                return wsSymbol.Kind == kind;
            },
            string q => wsSymbol =>
            {
                return wsSymbol.Name.Contains(q);
            },
        };
        foreach (Document doc in _state.Documents.Values)
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
                        End = new() { Line = 0, Character = 0 },
                    },
                    Uri = doc.Uri.AbsoluteUri,
                },
                Kind = SymbolKind.File,
                Name = workspaceName,
            };

            if (filter(symbol) is false)
            {
                continue;
            }
            symbols.Add(symbol);
        }
        return Task.FromResult<Response?>(
            Response.OfSuccess(workspaceRequest.Id, symbols.ToArray())
        );
    }
}
