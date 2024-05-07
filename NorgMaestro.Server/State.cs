using NorgMaestro.Parser;
using NorgMaestro.Rpc;

namespace NorgMaestro
{
    public class LanguageServerState
    {
        public Dictionary<Uri, Document> Documents { get; init; } = [];
        public Dictionary<Uri, HashSet<ReferenceLocation>> References { get; init; } = [];
        private readonly object _updateDocLock = new();

        public void Initialize(Uri rootUri)
        {
            string[] notes = Directory.GetFiles(rootUri.LocalPath);
            foreach (string note in notes)
            {
                if (Path.GetExtension(note) is not ".norg")
                {
                    continue;
                }
                _ = UpdateDocument(new Uri(note));
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
                    Content = content
                };

            lock (_updateDocLock)
            {
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
}
