using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public record Notification : Message
    {
        [JsonPropertyName("method")]
        public string Method { get; init; } = "window/showMessage";

        [JsonPropertyName("params")]
        public NotificationParams? Params { get; init; }

        public static Notification Default(string message, int type)
        {
            return new()
            {
                JsonRpc = "2.0",
                Params = new() { Message = message, Type = type }
            };
        }
    }

    public record NotificationParams
    {
        [JsonPropertyName("message")]
        public required string Message { get; init; }

        [JsonPropertyName("type")]
        public int Type { get; init; } = 1;
    }
}
