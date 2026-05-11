using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server.Methods;

internal static class SymbolBuilders
{
    public static TextRange ZeroRange()
    {
        return new()
        {
            Start = new() { Line = 0, Character = 0 },
            End = new() { Line = 0, Character = 0 },
        };
    }

    public static CallHierarchyItem FileCallHierarchyItem(string uri, string name, TextRange? range = null)
    {
        TextRange itemRange = range ?? ZeroRange();
        return new()
        {
            Uri = uri,
            Name = name,
            Kind = SymbolKind.File,
            Range = itemRange,
            SelectionRange = itemRange,
        };
    }

    public static WorkspaceSymbol FileWorkspaceSymbol(string uri, string name)
    {
        return new()
        {
            Kind = SymbolKind.File,
            Name = name,
            Location = new()
            {
                Uri = uri,
                Range = ZeroRange(),
            },
        };
    }
}
