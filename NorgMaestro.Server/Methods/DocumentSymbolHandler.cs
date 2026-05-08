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

        List<SymbolNode> symbolTree = [];
        Stack<(int Depth, SymbolNode Node)> stack = new();
        for (int i = 0; i < doc.Content.Length; i++)
        {
            Match match = HeadingRegex().Match(doc.Content[i]);
            if (match.Success is false)
            {
                continue;
            }

            int depth = match.Groups["stars"].Value.Length;
            Group nameGroup = match.Groups["name"];
            string rawTitle = nameGroup.Value;
            int leadingWhitespace = rawTitle.Length - rawTitle.TrimStart().Length;
            int trailingWhitespace = rawTitle.Length - rawTitle.TrimEnd().Length;
            string title = rawTitle.Trim();
            uint startCharacter = (uint)(nameGroup.Index + leadingWhitespace);
            uint endCharacter = (uint)(nameGroup.Index + rawTitle.Length - trailingWhitespace);
            TextRange range = new()
            {
                Start = new() { Line = (uint)i, Character = startCharacter },
                End = new() { Line = (uint)i, Character = endCharacter },
            };
            SymbolNode node =
                new(
                    new()
                    {
                        Name = title,
                        Kind = SymbolKind.StringKind,
                        Range = range,
                        SelectionRange = range,
                        Children = [],
                    }
                );

            while (stack.Count > 0 && stack.Peek().Depth >= depth)
            {
                _ = stack.Pop();
            }

            if (stack.Count == 0)
            {
                symbolTree.Add(node);
            }
            else
            {
                stack.Peek().Node.Children.Add(node);
            }

            stack.Push((depth, node));
        }

        DocumentSymbol[] symbols = symbolTree.Select(ToDocumentSymbol).ToArray();
        return Response.OfSuccess(symbolRequest.Id, symbols);
    }

    private static DocumentSymbol ToDocumentSymbol(SymbolNode node)
    {
        return node.Symbol with { Children = node.Children.Select(ToDocumentSymbol).ToArray() };
    }

    private sealed class SymbolNode(DocumentSymbol symbol)
    {
        public DocumentSymbol Symbol { get; } = symbol;
        public List<SymbolNode> Children { get; } = [];
    }
}
