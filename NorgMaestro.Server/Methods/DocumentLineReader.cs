using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

internal sealed class DocumentLineReader(LanguageServerState state)
{
    private readonly LanguageServerState _state = state;

    public string GetLine(Uri sourceUri, Position position)
    {
        if (
            _state.Documents.TryGetValue(sourceUri, out Document? document)
            && position.Line < (uint)document.Content.Length
        )
        {
            return document.Content[(int)position.Line];
        }

        return FileUtil
            .ReadRange(
                sourceUri,
                new()
                {
                    Start = position,
                    End = position,
                }
            )
            .FirstOrDefault("");
    }
}
