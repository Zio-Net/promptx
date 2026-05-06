namespace Zionet.Prompting;

/// <summary>
/// Response shape for a text-style prompt. Mirrors PromptProvider's TextPromptResponse.
/// </summary>
public sealed record TextPromptResponse
{
    /// <summary>
    /// Raw, un-rendered prompt body. Variable substitution is performed by the caller's PromptParser.
    /// </summary>
    public required string Content { get; init; }
}
