using NorgMaestro.Rpc;

namespace NorgMaestro.Methods
{
    public class IncomingCallsHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }
        public required LanguageServerState State { get; init; }

        public Response HandleRequest()
        {
            IncomingCallsRequest completionRequest = IncomingCallsRequest.From(Request);

            HashSet<Location> refs = State.References[new(completionRequest.Params.Item.Uri)];

            List<IncomingCallsResponseParams> response = refs.Select(
                    loc => new IncomingCallsResponseParams()
                    {
                        From = new()
                        {
                            Uri = loc.Uri,
                            Name = "file",
                            Kind = SymbolKind.File,
                            Range = loc.Range,
                            SelectionRange = loc.Range
                        },
                        FromRanges = [loc.Range],
                    }
                )
                .ToList();

            return Response.OfSuccess(completionRequest.Id, response);
        }
    }
}
