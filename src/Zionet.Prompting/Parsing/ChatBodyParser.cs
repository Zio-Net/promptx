using System.Text.RegularExpressions;
using Zionet.Prompting.Exceptions;

namespace Zionet.Prompting.Parsing;

/// <summary>
/// Parses a chat-style prompt body into ordered role/content blocks.
/// Grammar: a role block starts at column 0 with one of "system:", "user:", "assistant:", or "developer:".
/// Content runs until the next role marker or EOF; content can be multi-paragraph; roles can repeat.
/// </summary>
public static class ChatBodyParser
{
    private static readonly Regex RoleMarker = new(
        @"^(system|user|assistant|developer):[ \t]*\r?$",
        RegexOptions.Multiline | RegexOptions.Compiled);

    /// <summary>
    /// Parses the body into chat messages.
    /// </summary>
    /// <param name="body">Raw chat body (post-frontmatter).</param>
    /// <param name="sourcePath">Path used in error messages.</param>
    /// <returns>Ordered chat messages.</returns>
    /// <exception cref="PromptParsingException">When no role markers are found or content precedes the first role marker.</exception>
    public static IReadOnlyList<ChatPromptMessage> Parse(string body, string sourcePath)
    {
        var matches = RoleMarker.Matches(body);

        if (matches.Count == 0)
        {
            throw new PromptParsingException(
                $"Chat prompt '{sourcePath}' must contain at least one role block (system:, user:, assistant:, or developer:) at column 0.");
        }

        var firstMarker = matches[0];
        if (firstMarker.Index > 0)
        {
            var prelude = body[..firstMarker.Index];
            if (!string.IsNullOrWhiteSpace(prelude))
            {
                throw new PromptParsingException(
                    $"Chat prompt '{sourcePath}' has content before the first role marker; everything before the first role: line must be blank.");
            }
        }

        var messages = new List<ChatPromptMessage>(matches.Count);
        for (var i = 0; i < matches.Count; i++)
        {
            var marker = matches[i];
            var role = marker.Groups[1].Value;
            var contentStart = marker.Index + marker.Length;
            var contentEnd = i + 1 < matches.Count ? matches[i + 1].Index : body.Length;

            var content = body[contentStart..contentEnd].Trim('\r', '\n', ' ', '\t');

            messages.Add(new ChatPromptMessage
            {
                Role = role,
                Content = content,
            });
        }

        return messages;
    }
}
