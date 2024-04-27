using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public record Notification : Message
    {
        [JsonPropertyName("method")]
        public string Method { get; init; } = "window/showMessage";

        [JsonPropertyName("params")]
        public NotificationParams? Params { get; init; }
    }

    public record NotificationParams
    {
        [JsonPropertyName("message")]
        public required string Message { get; init; }

        [JsonPropertyName("level")]
        public int Level { get; init; } = 1;
    }
}
