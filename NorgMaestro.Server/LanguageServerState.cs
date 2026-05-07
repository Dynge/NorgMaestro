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

    public async Task Initialize(Uri rootUri)
    {
        foreach (
            string note in Directory.GetFiles(
                rootUri.LocalPath,
                "*.norg",
                SearchOption.AllDirectories
            )
        )
        {
            _ = await UpdateDocument(new Uri(Path.GetFullPath(note)));
        }
    }

    public async Task<Document?> UpdateDocument(Uri fileUri)
    {
        RemoveReferencesFromDocument(fileUri);

        var metadata = await NorgParser.GetMetadata(fileUri);
        var content = await File.ReadAllLinesAsync(fileUri.LocalPath);
        var references = NorgParser.GetReferences(fileUri, content);
        var doc = new Document()
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

    public Dictionary<Uri, Diagnostic[]> GetDiagnostics()
    {
        Dictionary<Uri, List<Diagnostic>> diagnosticsByDoc = [];

        foreach ((Uri targetUri, HashSet<ReferenceLocation> locations) in _references)
        {
            if (_documents.ContainsKey(targetUri))
            {
                continue;
            }

            foreach (ReferenceLocation location in locations)
            {
                Uri sourceUri = new(location.Location.Uri);
                if (diagnosticsByDoc.TryGetValue(sourceUri, out List<Diagnostic>? diagnostics) is false)
                {
                    diagnostics = [];
                    diagnosticsByDoc[sourceUri] = diagnostics;
                }

                diagnostics.Add(
                    new()
                    {
                        Severity = DiagnosticSeverity.Warning,
                        Range = location.Location.Range,
                        Message = $"Unresolved note link: {targetUri.AbsolutePath}",
                        Source = "norgmaestro",
                    }
                );
            }
        }

        return diagnosticsByDoc.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray());
    }

    private void RemoveReferencesFromDocument(Uri fileUri)
    {
        foreach ((Uri targetUri, HashSet<ReferenceLocation> locations) in _references.ToArray())
        {
            locations.RemoveWhere(reference => reference.Location.Uri == fileUri.AbsoluteUri);
            if (locations.Count == 0)
            {
                _references.Remove(targetUri);
            }
        }
    }
}

public record Document
{
    public required Uri Uri { get; init; }
    public required NeorgMetadata Metadata { get; init; }
    public required string[] Content { get; init; }
}
