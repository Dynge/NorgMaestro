using NorgMaestro.Rpc;

namespace NorgMaestro.Methods
{
    public class WorkspaceSymbolHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }
        public required LanguageServerState State { get; init; }

        public Response HandleRequest()
        {
            WorkspaceSymbolRequest workspaceRequest = WorkspaceSymbolRequest.From(Request);
            List<WorkspaceSymbol> symbols = [];
            foreach (Document doc in State.Documents.Values)
            {
                symbols.Add(
                    new()
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
                        Name = doc.Metadata.Title?.Name ?? "",
                    }
                );
            }
            return Response.OfSuccess(workspaceRequest.Id, symbols.ToArray());
        }
    }
}
