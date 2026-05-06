using System.Text.Json.Serialization;

namespace Zionet.Shared.Prompting;

/// <summary>
/// Response shape for a chat-style prompt. Mirrors PromptProvider's ChatPromptResponse.
/// </summary>
public sealed record ChatPromptResponse
{
    /// <summary>
    /// Ordered chat messages parsed from the prompt body.
    /// </summary>
    public required ChatPromptMessage[] ChatMessages { get; init; }
}

/// <summary>
/// Single chat message inside a chat prompt. Mirrors PromptProvider's chat-message shape.
/// </summary>
public sealed record ChatPromptMessage
{
    // JsonPropertyName pins the wire format to lowercase so callers that serialize
    // these messages (e.g. AiHelper -> PromptParser.ParseChat, which looks up "role"/"content"
    // case-sensitively) get the right shape regardless of serializer options.
    [JsonPropertyName("role")]
    public required string Role { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }
}
