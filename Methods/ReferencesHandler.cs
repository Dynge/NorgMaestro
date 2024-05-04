using System.Text.RegularExpressions;
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

            uint charPos = referenceRequest.Params.Position.Character;
            Match? cursorUriMatch = NorgParser
                .NorgFileLinkRegex()
                .Matches(line)
                .FirstOrDefault(m => m.Index <= charPos && m.Index + m.Length >= charPos);

            if (cursorUriMatch is null || cursorUriMatch.Success is false)
            {
                return Response.OfSuccess(referenceRequest.Id);
            }
            Uri cursorUri =
                new(
                    Path.Join(
                        Directory
                            .GetParent(referenceRequest.Params.TextDocument.Uri.AbsolutePath)!
                            .FullName,
                        cursorUriMatch.Groups["File"].Value + ".norg"
                    )
                );

            HashSet<Location> references = State.References.GetValueOrDefault(cursorUri, []);
            if (referenceRequest.Params.Context.IncludeDeclaration)
            {
                _ = references.Add(
                    new()
                    {
                        Uri = cursorUri.AbsoluteUri,
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
