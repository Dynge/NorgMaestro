using System.Text.RegularExpressions;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public partial class DocumentSymbolHandler(LanguageServerState state, RpcMessage request)
    : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    [GeneratedRegex(@"^(?<stars>\*+)\s+(?<name>.+)$")]
    private static partial Regex HeadingRegex();

    public Response? HandleRequest()
    {
        DocumentSymbolRequest symbolRequest = DocumentSymbolRequest.From(_request);
        if (_state.Documents.TryGetValue(symbolRequest.Params.TextDocument.Uri, out Document? doc) is false)
        {
            return Response.OfSuccess(symbolRequest.Id, Array.Empty<DocumentSymbol>());
        }

        List<DocumentSymbol> symbols = [];
        for (int i = 0; i < doc.Content.Length; i++)
        {
            Match match = HeadingRegex().Match(doc.Content[i]);
            if (match.Success is false)
            {
                continue;
            }

            string title = match.Groups["name"].Value.Trim();
            uint startCharacter = (uint)match.Groups["name"].Index;
            uint endCharacter = startCharacter + (uint)title.Length;
            TextRange range = new()
            {
                Start = new() { Line = (uint)i, Character = startCharacter },
                End = new() { Line = (uint)i, Character = endCharacter },
            };
            symbols.Add(new() { Name = title, Kind = SymbolKind.StringKind, Range = range, SelectionRange = range });
        }

        return Response.OfSuccess(symbolRequest.Id, symbols);
    }
}
