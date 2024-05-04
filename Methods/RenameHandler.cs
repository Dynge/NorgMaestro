using System.Text.RegularExpressions;
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
            Match? cursorUriMatch = NorgParser
                .NorgFileLinkRegex()
                .Matches(line)
                .FirstOrDefault(m => m.Index <= charPos && m.Index + m.Length >= charPos);

            if (cursorUriMatch is null || cursorUriMatch.Success is false)
            {
                return Response.OfSuccess(renameRequest.Id);
            }

            Group fileGroup = cursorUriMatch.Groups["File"];
            Group linkTextGroup = cursorUriMatch.Groups["LinkText"];

            Uri cursorUri =
                new(
                    Path.Join(
                        Directory
                            .GetParent(renameRequest.Params.TextDocument.Uri.AbsolutePath)!
                            .FullName,
                        fileGroup.Value + ".norg"
                    )
                );

            Document cursorDocument = State.Documents[cursorUri];
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
                .References[cursorUri]
                .ToLookup(loc => loc.Uri)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp =>
                        kvp.ToList()
                            .Select(loc => new TextEdit()
                            {
                                NewText = renameRequest.Params.NewName,
                                Range = loc.Range
                            })
                            .ToArray()
                );

            _ = changeInRefs.TryGetValue(cursorUri.AbsoluteUri, out TextEdit[]? value);
            List<TextEdit> textEdits = value switch
            {
                TextEdit[] => [.. value],
                _ => [],
            };
            textEdits.AddRange(changeInCursor);
            changeInRefs[cursorUri.AbsoluteUri] = [.. textEdits];

            WorkspaceEdit edit = new() { Changes = changeInRefs };

            return Response.OfSuccess(renameRequest.Id, edit);
        }
    }
}
