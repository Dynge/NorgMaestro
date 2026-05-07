using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class RenameHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;

    public Task<Response?> HandleRequest()
    {
        RenameRequest renameRequest = RenameRequest.From(_request);

        string line = File.ReadLinesAsync(renameRequest.Params.TextDocument.Uri.AbsolutePath)
            .ToBlockingEnumerable()
            .Skip((int)renameRequest.Params.Position.Line)
            .FirstOrDefault("");

        uint charPos = renameRequest.Params.Position.Character;
        NorgLink? link = NorgParser.ParseLink(
            renameRequest.Params.TextDocument.Uri,
            renameRequest.Params.Position,
            line
        );

        Uri targetUri;
        if (link is not null)
        {
            targetUri = _state.ResolveLinkUri(link);
        }
        else if (_state.TryGetTitleTarget(renameRequest.Params.TextDocument.Uri, renameRequest.Params.Position, out Uri titleTarget))
        {
            targetUri = titleTarget;
        }
        else
        {
            return Task.FromResult<Response?>(Response.OfSuccess(renameRequest.Id));
        }

        Document cursorDocument = _state.Documents[targetUri];
        List<TextEdit> changeInCursor = cursorDocument.Metadata.Title switch
        {
            MetaField titleField =>
            [
                new() { NewText = renameRequest.Params.NewName, Range = titleField.Range },
            ],
            _ => [],
        };

        Dictionary<string, TextEdit[]> changeInRefs = _state
            .References.GetValueOrDefault(targetUri, [])
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
                                    targetUri,
                                    reference.Location.Range.Start,
                                    reference.Line
                                )!
                                .LinkTextRange,
                        })
                        .ToArray()
            );

        _ = changeInRefs.TryGetValue(targetUri.AbsoluteUri, out TextEdit[]? value);
        List<TextEdit> textEdits = value switch
        {
            TextEdit[] => [.. value],
            _ => [],
        };
        textEdits.AddRange(changeInCursor);
        changeInRefs[targetUri.AbsoluteUri] = [.. textEdits];

        WorkspaceEdit edit = new() { Changes = changeInRefs };

        _state.UpdateTitleInState(targetUri, renameRequest.Params.NewName);

        return Task.FromResult<Response?>(Response.OfSuccess(renameRequest.Id, edit));
    }
}
