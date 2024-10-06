using System.Collections.ObjectModel;
using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server;

public class LanguageServerState
{
    private readonly Dictionary<Uri, Document> _documents = [];
    private readonly Dictionary<Uri, HashSet<ReferenceLocation>> _references = [];

    public ReadOnlyDictionary<Uri, Document> Documents => _documents.AsReadOnly();
    public ReadOnlyDictionary<Uri, HashSet<ReferenceLocation>> References =>
        _references.AsReadOnly();

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

        _documents[fileUri] = doc;
        foreach (KeyValuePair<Uri, HashSet<ReferenceLocation>> refKvp in references)
        {
            _references[refKvp.Key] = _references.TryGetValue(
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
