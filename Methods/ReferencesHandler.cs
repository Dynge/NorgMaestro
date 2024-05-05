using NorgMaestro.Parser;
using NorgMaestro.Rpc;

namespace NorgMaestro.Methods
{
    public class ReferencesHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }
        public required LanguageServerState State { get; init; }

        public Response HandleRequest()
        {
            ReferencesRequest referenceRequest = ReferencesRequest.From(Request);

            string line = File.ReadLines(referenceRequest.Params.TextDocument.Uri.AbsolutePath)
                .Skip((int)referenceRequest.Params.Position.Line)
                .FirstOrDefault("");

            NorgLink? link = NorgParser.ParseLink(
                referenceRequest.Params.TextDocument.Uri,
                referenceRequest.Params.Position,
                line
            );

            if (link is null)
            {
                return Response.OfSuccess(referenceRequest.Id);
            }

            HashSet<Location> references = State
                .References.GetValueOrDefault(link.GetFileLinkUri(), [])
                .Select(reference => reference.Location)
                .ToHashSet();
            if (referenceRequest.Params.Context.IncludeDeclaration)
            {
                _ = references.Add(
                    new()
                    {
                        Uri = link.GetFileLinkUri().AbsoluteUri,
                        Range = new()
                        {
                            Start = new() { Line = 0, Character = 0, },
                            End = new() { Line = 0, Character = 0, },
                        }
                    }
                );
            }

            return (references.Count > 0) switch
            {
                true => Response.OfSuccess(referenceRequest.Id, references),
                false => Response.OfSuccess(referenceRequest.Id),
            };
        }
    }
}
