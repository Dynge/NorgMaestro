using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server;

public class LanguageServerState
{
    public Dictionary<Uri, Document> Documents { get; init; } = [];
    public Dictionary<Uri, HashSet<ReferenceLocation>> References { get; init; } = [];

    public void Initialize(Uri rootUri)
    {
        foreach (
            string note in Directory.GetFiles(
                rootUri.LocalPath,
                "*.norg",
                SearchOption.AllDirectories
            )
        )
        {
            _ = UpdateDocument(new Uri(Path.GetFullPath(note)));
        }
    }

    public Document? UpdateDocument(Uri fileUri)
    {
        NeorgMetadata metadata = NorgParser.GetMetadata(fileUri);
        string[] content = File.ReadAllLines(fileUri.LocalPath);
        Dictionary<Uri, HashSet<ReferenceLocation>> references = NorgParser.GetReferences(
            fileUri,
            content
        );
        Document doc =
            new()
            {
                Uri = fileUri,
                Metadata = metadata,
                Content = content,
            };

        Documents[fileUri] = doc;
        foreach (KeyValuePair<Uri, HashSet<ReferenceLocation>> refKvp in references)
        {
            References[refKvp.Key] = References.TryGetValue(
                refKvp.Key,
                out HashSet<ReferenceLocation>? value
            )
                ? ([.. value, .. refKvp.Value])
                : ([.. refKvp.Value]);
        }

        return doc;
    }
}

public record Document
{
    public required Uri Uri { get; init; }
    public required NeorgMetadata Metadata { get; init; }
    public required string[] Content { get; init; }
}
