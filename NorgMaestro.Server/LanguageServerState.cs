using System.Collections.ObjectModel;
using NorgMaestro.Server.Parser;
using NorgMaestro.Server.Rpc;

namespace NorgMaestro.Server;

public class LanguageServerState
{
    private readonly Dictionary<Uri, Document> _documents = [];
    private readonly Dictionary<Uri, HashSet<ReferenceLocation>> _references = [];
    private readonly Dictionary<string, Uri> _workspaces = [];
    private Uri? _workspaceRoot;

    public ReadOnlyDictionary<Uri, Document> Documents => _documents.AsReadOnly();
    public ReadOnlyDictionary<Uri, HashSet<ReferenceLocation>> References =>
        _references.AsReadOnly();

    public async Task Initialize(Uri rootUri, IEnumerable<WorkspaceFolder>? workspaceFolders = null)
    {
        _workspaceRoot = rootUri;
        _workspaces.Clear();
        foreach (WorkspaceFolder workspace in workspaceFolders ?? [])
        {
            _workspaces[workspace.Name] = workspace.Uri;
        }

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

    internal Uri ResolveLinkUri(NorgLink link)
    {
        string filePath = link.File;
        if (filePath.StartsWith("$/"))
        {
            if (_workspaceRoot is null)
            {
                return ToNorgUri(Path.Join(Directory.GetParent(link.NorgFile.AbsolutePath)!.FullName, filePath[2..]));
            }
            return ToNorgUri(Path.Join(_workspaceRoot.LocalPath, filePath[2..]));
        }

        if (filePath.StartsWith('$'))
        {
            int slashIndex = filePath.IndexOf('/');
            if (slashIndex > 1)
            {
                string workspaceName = filePath[1..slashIndex];
                string workspaceRelativePath = filePath[(slashIndex + 1)..];
                if (_workspaces.TryGetValue(workspaceName, out Uri? workspaceUri))
                {
                    return ToNorgUri(Path.Join(workspaceUri.LocalPath, workspaceRelativePath));
                }
            }
        }

        if (filePath.StartsWith("~/"))
        {
            string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return ToNorgUri(Path.Join(homePath, filePath[2..]));
        }

        if (Path.IsPathRooted(filePath))
        {
            return ToNorgUri(filePath);
        }

        string parent = Directory.GetParent(link.NorgFile.AbsolutePath)!.FullName;
        return ToNorgUri(Path.Join(parent, filePath));
    }

    private static Uri ToNorgUri(string path)
    {
        string norgPath = path.EndsWith(".norg") ? path : path + ".norg";
        return new Uri(Path.GetFullPath(norgPath));
    }

    private Dictionary<Uri, HashSet<ReferenceLocation>> GetReferences(Uri fileUri, string[] content)
    {
        Dictionary<Uri, HashSet<ReferenceLocation>> references = [];
        foreach (NorgLink link in NorgParser.ParseLinks(fileUri, content))
        {
            Uri resolvedUri = ResolveLinkUri(link);
            string line = content[(int)link.AbsoluteRange.Start.Line];
            ReferenceLocation refLocation = new()
            {
                Line = line,
                Location = new() { Uri = fileUri.AbsoluteUri, Range = link.AbsoluteRange }
            };

            references[resolvedUri] = references.TryGetValue(resolvedUri, out HashSet<ReferenceLocation>? value)
                ? ([.. value, refLocation])
                : ([refLocation]);
        }

        return references;
    }
}

public record Document
{
    public required Uri Uri { get; init; }
    public required NeorgMetadata Metadata { get; init; }
    public required string[] Content { get; init; }
}
