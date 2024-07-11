using NorgMaestro.Rpc;

namespace NorgMaestro.Methods;

public class IncomingCallsHandler : IMessageHandler
{
    public required RpcMessage Request { get; init; }
    public required LanguageServerState State { get; init; }

    public Response HandleRequest()
    {
        IncomingCallsRequest completionRequest = IncomingCallsRequest.From(Request);

        if (
            State.References.TryGetValue(new Uri(completionRequest.Params.Item.Uri), out var refs)
            is false
        )
        {
            return Response.OfSuccess(
                completionRequest.Id,
                new List<IncomingCallsResponseParams>()
            );
        }

        List<IncomingCallsResponseParams> response = refs.Select(reference =>
            {
                State.Documents.TryGetValue(new(reference.Location.Uri), out var doc);
                return new IncomingCallsResponseParams()
                {
                    From = new()
                    {
                        Uri = reference.Location.Uri,
                        Name = doc?.Metadata.Title?.Name ?? "Unknown title",
                        Kind = SymbolKind.File,
                        Range = reference.Location.Range,
                        SelectionRange = reference.Location.Range
                    },
                    FromRanges = [reference.Location.Range],
                };
            })
            .ToList();

        return Response.OfSuccess(completionRequest.Id, response);
    }
}
