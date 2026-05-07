using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods.CodeActions;

internal record CodeActionContext
{
    public required LanguageServerState State { get; init; }
    public required CodeActionRequest Request { get; init; }
    public required Uri SourceUri { get; init; }
    public required Position Cursor { get; init; }
    public required string Line { get; init; }
    public required NorgLink? LinkAtCursor { get; init; }
}
