using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public class RpcMessageWriter
    {
        public required StreamReader StdinReader { get; init; }
        public required Stream Stdout { get; init; }

        private const string ContentLengthHeader = "Content-Length: ";
        private const int MaxBuffer = 5000;
        private readonly Func<byte[], string> StringOfLength = body =>
            Convert.ToString(body.Length, System.Globalization.CultureInfo.InvariantCulture);

        private readonly JsonSerializerOptions options =
            new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        public void EncodeAndWrite(object res)
        {
            byte[] body = JsonSerializer.SerializeToUtf8Bytes(res, options);
            IEnumerable<byte> rpcMessage = Encoding
                .UTF8.GetBytes(ContentLengthHeader)
                .Concat(Encoding.UTF8.GetBytes(StringOfLength(body)))
                .Concat(Encoding.UTF8.GetBytes("\r\n\r\n"))
                .Concat(body);
            Stdout.Write(rpcMessage.ToArray());
        }

        public RpcMessage? Decode()
        {
            string? header = StdinReader.ReadLine();
            bool headerExists = header?.StartsWith(ContentLengthHeader) ?? false;
            if (headerExists is false)
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
            bool contentLengthExists = int.TryParse(
                string.Concat(header!.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit)),
                out int contentLength
            );
            if (contentLengthExists is false)
            {
                // Console.WriteLine("Jamen kom nu.. der sku være tal her...");
                return null;
            }
            // Console.WriteLine("Content length is: " + contentLength);

            string? newline = StdinReader.ReadLine();
            bool newLinesExists = newline?.SequenceEqual("") ?? false;
            if (newLinesExists is false)
            {
                // Console.WriteLine(
                //     string.Format(
                //         "Jamen kom nu.. der sku være \\r\\n\\r\\n her... ({0})",
                //         string.Concat(newline)
                //     )
                // );
                return null;
            }

            char[] buffer = new char[contentLength];
            _ = StdinReader.Read(buffer, 0, contentLength);
            string content = string.Concat(buffer);

            RpcMessage? pson = JsonSerializer.Deserialize<RpcMessage>(content);
            return pson;

            // Console.WriteLine(pson?.ToString());
            // Console.WriteLine(pson?.Params?.Deserialize<InitializeRequestParams>());
        }
    }
}
