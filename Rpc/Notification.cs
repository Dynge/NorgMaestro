using System.Text.Json.Serialization;

namespace CeorgLsp.Rpc
{
    public record Notification : Message
    {
        [JsonPropertyName("method")]
        public string Method { get; init; } = "window/showMessage";

        [JsonPropertyName("params")]
        public NotificationParams? Params { get; init; }

        public static Notification Default(string message, MessageType type = MessageType.Log)
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
        public MessageType Type { get; init; } = MessageType.Log;
    }

    public enum MessageType
    {
        /**
         * An error message.
         */
        Error = 1,

        /**
         * A warning message.
         */
        Warning = 2,

        /**
         * An information message.
         */
        Info = 3,

        /**
         * A log message.
         */
        Log = 4,

        /**
         * A debug message.
         *
         * @since 3.18.0
         * @proposed
         */
        Debug = 5,
    }
}
