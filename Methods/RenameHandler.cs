using NorgMaestro.Parser;
using NorgMaestro.Rpc;

namespace NorgMaestro.Methods
{
    public class RenameHandler : IMessageHandler
    {
        public required RpcMessage Request { get; init; }
        public required LanguageServerState State { get; init; }

        public Response HandleRequest()
        {
            RenameRequest renameRequest = RenameRequest.From(Request);

            string line = File.ReadLines(renameRequest.Params.TextDocument.Uri.AbsolutePath)
                .Skip((int)renameRequest.Params.Position.Line)
                .FirstOrDefault("");

            uint charPos = renameRequest.Params.Position.Character;
            NorgLink? link = NorgParser.ParseLink(
                renameRequest.Params.TextDocument.Uri,
                renameRequest.Params.Position,
                line
            );

            if (link is null)
            {
                return Response.OfSuccess(renameRequest.Id);
            }

            Document cursorDocument = State.Documents[link.GetFileLinkUri()];
            TextEdit[] changeInCursor = cursorDocument.Metadata.Title switch
            {
                MetaField titleField
                    =>
                    [
                        new() { NewText = renameRequest.Params.NewName, Range = titleField.Range, }
                    ],
                _ => [],
            };

            Dictionary<string, TextEdit[]> changeInRefs = State
                .References[link.GetFileLinkUri()]
                .ToLookup(reference => reference.Location.Uri)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp =>
                        kvp.ToList()
                            .Select(reference => new TextEdit()
                            {
                                NewText = renameRequest.Params.NewName,
                                Range = NorgParser
                                    .ParseLink(
                                        link.GetFileLinkUri(),
                                        reference.Location.Range.Start,
                                        reference.Line
                                    )!
                                    .LinkTextRange
                            })
                            .ToArray()
                );

            _ = changeInRefs.TryGetValue(link.GetFileLinkUri().AbsoluteUri, out TextEdit[]? value);
            List<TextEdit> textEdits = value switch
            {
                TextEdit[] => [.. value],
                _ => [],
            };
            textEdits.AddRange(changeInCursor);
            changeInRefs[link.GetFileLinkUri().AbsoluteUri] = [.. textEdits];

            WorkspaceEdit edit = new() { Changes = changeInRefs };

            return Response.OfSuccess(renameRequest.Id, edit);
        }
    }
}
