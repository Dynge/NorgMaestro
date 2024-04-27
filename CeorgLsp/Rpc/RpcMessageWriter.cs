using System.Text;
using System.Text.Json;

namespace CeorgLsp.Rpc
{
    public class RpcMessageWriter
    {
        public required Stream Stdin { get; init; }
        public required Stream Stdout { get; init; }

        private const string ContentLengthHeader = "Content-Length: ";
        private const int MaxBuffer = 128;
        private readonly Func<byte[], string> StringOfLength = body =>
            Convert.ToString(body.Length, System.Globalization.CultureInfo.InvariantCulture);

        public void EncodeAndWrite(object res)
        {
            byte[] body = JsonSerializer.SerializeToUtf8Bytes(res);
            IEnumerable<byte> rpcMessage = Encoding
                .UTF8.GetBytes(ContentLengthHeader)
                .Concat(Encoding.UTF8.GetBytes(StringOfLength(body)))
                .Concat(Encoding.UTF8.GetBytes("\r\n\r\n"))
                .Concat(body);
            Stdout.Write(rpcMessage.ToArray());
        }

        public Request? Decode()
        {
            using StreamReader streamReader = new(Stdin, Encoding.UTF8);
            char[] buffer = new char[MaxBuffer];
            _ = streamReader.ReadBlock(buffer, 0, ContentLengthHeader.Length);
            char[] headerSize = buffer[..ContentLengthHeader.Length];
            bool headerExists = headerSize.SequenceEqual(ContentLengthHeader);
            if (!headerExists)
            {
                // Console.WriteLine(
                //     string.Format(
                //         "The RPC message '{0}' did not start with '{1}'.",
                //         string.Concat(headerSize),
                //         ContentLengthHeader
                //     )
                // );
                return null;
            }
            Array.Clear(buffer);
            int size = streamReader.ReadBlock(buffer, 0, MaxBuffer);
            // Console.WriteLine("Read: " + size);
            bool contentLengthExists = int.TryParse(
                string.Concat(buffer.TakeWhile(char.IsDigit)),
                out int contentLength
            );
            if (!contentLengthExists)
            {
                // Console.WriteLine("Jamen kom nu.. der sku være tal her...");
                return null;
            }
            // Console.WriteLine("Content length is: " + contentLength);

            IEnumerable<char> sep = buffer.SkipWhile(char.IsDigit).Take(4);

            bool newLinesExists = sep.SequenceEqual("\r\n\r\n");
            if (!newLinesExists)
            {
                // Console.WriteLine(
                //     string.Format(
                //         "Jamen kom nu.. der sku være \\r\\n\\r\\n her... ({0})",
                //         string.Concat(sep)
                //     )
                // );
                return null;
            }
            int buffer_offset = contentLength.ToString().Length + 4;
            // Console.WriteLine("Buffer_size: " + buffer_offset);

            string remaining_from_buf = string.Concat(
                buffer[buffer_offset..].Where(c => c != '\0')
            );
            string content = string.Concat(remaining_from_buf, streamReader.ReadToEnd());
            Request? pson = JsonSerializer.Deserialize<Request>(content);

            // Console.WriteLine(pson?.ToString());
            // Console.WriteLine(pson?.Params?.Deserialize<InitializeRequestParams>());
            return pson;
        }
    }
}
