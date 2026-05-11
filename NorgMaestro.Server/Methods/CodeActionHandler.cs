using NorgMaestro.Server.Methods.CodeActions;
using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

public class CodeActionHandler(LanguageServerState state, RpcMessage request) : IMessageHandler
{
    private readonly RpcMessage _request = request;
    private readonly LanguageServerState _state = state;
    private readonly CodeActionFactory _factory = new();

    public const string CreateNoteCommand = "norgmaestro.createNote";
    public const string CreateNoteAndOpenCommand = "norgmaestro.createNoteAndOpen";
    public const string CreateBacklinkSectionCommand = "norgmaestro.createBacklinkSection";
    public const string ExtractSelectionToNoteCommand = "norgmaestro.extractSelectionToNote";
    public const string MoveNoteToWorkspaceCommand = "norgmaestro.moveNoteToWorkspace";
    public const string CreateNoteFromLinkTextCommand = "norgmaestro.createNoteFromLinkText";

    public Task<Response?> HandleRequest()
    {
        CodeActionRequest request = CodeActionRequest.From(_request);
        Uri sourceUri = request.Params.TextDocument.Uri;
        Position cursor = request.Params.Range.Start;
        string line = string.Empty;
        if (File.Exists(sourceUri.LocalPath))
        {
            line = FileUtil.ReadRange(sourceUri, new() { Start = cursor, End = cursor }).FirstOrDefault("");
        }
        NorgLink? link = NorgParser.ParseLink(sourceUri, cursor, line);

        CodeActions.CodeActionContext context = new()
        {
            State = _state,
            Request = request,
            SourceUri = sourceUri,
            Cursor = cursor,
            Line = line,
            LinkAtCursor = link,
        };

        CodeAction[] actions = _factory.Build(context);
        return Task.FromResult<Response?>(Response.OfSuccess(request.Id, actions));
    }
}
