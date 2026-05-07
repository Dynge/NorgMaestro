using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class OutgoingCallsHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Response? HandleRequest()
    {
        OutgoingCallsRequest outgoingRequest = OutgoingCallsRequest.From(_request);
        Uri sourceUri = new(outgoingRequest.Params.Item.Uri);
        if (_state.Documents.TryGetValue(sourceUri, out Document? doc) is false)
        {
            return Response.OfSuccess(outgoingRequest.Id, new List<OutgoingCallsResponseParams>());
        }

        List<OutgoingCallsResponseParams> outgoingCalls = [];
        foreach (NorgLink link in NorgParser.ParseLinks(doc.Uri, doc.Content))
        {
            Uri targetUri = _state.ResolveLinkUri(link);
            if (_state.Documents.TryGetValue(targetUri, out Document? targetDoc) is false)
            {
                continue;
            }

            outgoingCalls.Add(
                new()
                {
                    To = new()
                    {
                        Uri = targetDoc.Uri.AbsoluteUri,
                        Name = targetDoc.Metadata.Title?.Name ?? "Unknown title",
                        Kind = SymbolKind.File,
                        Range = targetDoc.Metadata.Title?.Range
                            ?? new()
                            {
                                Start = new() { Line = 0, Character = 0 },
                                End = new() { Line = 0, Character = 0 },
                            },
                        SelectionRange = targetDoc.Metadata.Title?.Range
                            ?? new()
                            {
                                Start = new() { Line = 0, Character = 0 },
                                End = new() { Line = 0, Character = 0 },
                            },
                    },
                    FromRanges = [link.AbsoluteRange],
                }
            );
        }

        return Response.OfSuccess(outgoingRequest.Id, outgoingCalls);
    }
}
