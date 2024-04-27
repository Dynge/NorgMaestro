using System.Text.Json;
using System.Text.Json.Serialization;

namespace CeorgLsp
{
    public readonly record struct Message
    {
        [JsonPropertyName("jsonrpc")]
        public required string JsonRpc { get; init; }

        [JsonPropertyName("method")]
        public required string Method { get; init; }

        [JsonPropertyName("params")]
        public JsonElement? Params { get; init; }
    }

    public readonly record struct Notification
    {
        [JsonPropertyName("jsonrpc")]
        public required string JsonRpc { get; init; }

        [JsonPropertyName("method")]
        public required string Method { get; init; }
    }

    public readonly record struct Cap
    {
        [JsonPropertyName("capabilities")]
        public JsonElement Data { get; init; }
    }

    public class DecodeRpcMessage
    {
        private const string ContentLengthHeader = "Content-Length: ";
        private const int MaxBuffer = 128;

        public static Message? Decode(Stream stream)
        {
            using StreamReader streamReader = new(stream, System.Text.Encoding.UTF8);
            char[] buffer = new char[MaxBuffer];
            _ = streamReader.ReadBlock(buffer, 0, ContentLengthHeader.Length);
            char[] headerSize = buffer[..ContentLengthHeader.Length];
            bool headerExists = headerSize.SequenceEqual(ContentLengthHeader);
            if (!headerExists)
            {
                Console.WriteLine(
                    string.Format(
                        "The RPC message '{0}' did not start with '{1}'.",
                        string.Concat(headerSize),
                        ContentLengthHeader
                    )
                );
                return null;
            }
            Array.Clear(buffer);
            int size = streamReader.ReadBlock(buffer, 0, MaxBuffer);
            Console.WriteLine("Read: " + size);
            bool contentLengthExists = int.TryParse(
                string.Concat(buffer.TakeWhile(char.IsDigit)),
                out int contentLength
            );
            if (!contentLengthExists)
            {
                Console.WriteLine("Jamen kom nu.. der sku være tal her...");
                return null;
            }
            Console.WriteLine("Content length is: " + contentLength);

            IEnumerable<char> sep = buffer.SkipWhile(char.IsDigit).Take(4);

            bool newLinesExists = sep.SequenceEqual("\r\n\r\n");
            if (!newLinesExists)
            {
                Console.WriteLine(
                    string.Format(
                        "Jamen kom nu.. der sku være \\r\\n\\r\\n her... ({0})",
                        string.Concat(sep)
                    )
                );
                return null;
            }
            int buffer_offset = contentLength.ToString().Length + 4;
            Console.WriteLine("Buffer_size: " + buffer_offset);

            string remaining_from_buf = string.Concat(
                buffer[buffer_offset..].Where(c => c != '\0')
            );
            string content = string.Concat(remaining_from_buf, streamReader.ReadToEnd());
            Message pson = JsonSerializer.Deserialize<Message>(content);

            Console.WriteLine(pson.ToString());
            Console.WriteLine(pson.Params?.Deserialize<Cap>().Data);
            return pson;
        }
    }
}
