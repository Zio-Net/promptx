using System.Text.Json;
using Microsoft.Extensions.AI;
using Stubble.Core.Builders;

namespace Zionet.Prompting;

/// <summary>
/// Handles parsing and rendering of Langfuse prompts.
/// Supports both text prompts and chat prompts (JSON array of messages).
/// </summary>
public static class PromptParser
{
    private static readonly Stubble.Core.StubbleVisitorRenderer Stubble = new StubbleBuilder().Build();

    /// <summary>
    /// Result of parsing a prompt.
    /// </summary>
    public sealed record ParsedPrompt
    {
        /// <summary>
        /// Instructions for the agent (from first system message or entire text prompt).
        /// </summary>
        public string Instructions { get; init; } = string.Empty;

        /// <summary>
        /// Base messages from chat prompt (excluding first system message which becomes Instructions).
        /// Empty for text prompts.
        /// </summary>
        public IReadOnlyList<ChatMessage> BaseMessages { get; init; } = [];

        /// <summary>
        /// Whether the prompt was a chat prompt (JSON array) or text prompt.
        /// </summary>
        public bool IsChatPrompt { get; init; }
    }

    /// <summary>
    /// Parse text prompt and apply variable substitution.
    /// </summary>
    /// <param name="promptContent">Raw text prompt content from Langfuse.</param>
    /// <param name="variables">Variables to substitute (can be null).</param>
    /// <returns>Parsed prompt with instructions.</returns>
    public static ParsedPrompt ParseText(string promptContent, IReadOnlyDictionary<string, object?>? variables)
    {
        var renderedContent = RenderTemplate(promptContent, variables);
        return new ParsedPrompt
        {
            Instructions = renderedContent,
            BaseMessages = [],
            IsChatPrompt = false
        };
    }

    /// <summary>
    /// Parse chat prompt (JSON array of messages) and apply variable substitution.
    /// </summary>
    /// <param name="chatMessagesJson">JSON array of chat messages from Langfuse.</param>
    /// <param name="variables">Variables to substitute (can be null).</param>
    /// <returns>Parsed prompt with instructions and base messages.</returns>
    /// <exception cref="InvalidOperationException">Thrown when JSON is invalid or empty.</exception>
    public static ParsedPrompt ParseChat(string chatMessagesJson, IReadOnlyDictionary<string, object?>? variables)
    {
        using var doc = JsonDocument.Parse(chatMessagesJson);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Chat prompt must be a JSON array.");

        var messages = new List<(string Role, string Content)>();

        foreach (var element in root.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException("Each chat message must be an object.");

            var role = element.TryGetProperty("role", out var roleProp) ? roleProp.GetString() : null;
            var content = element.TryGetProperty("content", out var contentProp) ? contentProp.GetString() : null;

            if (string.IsNullOrEmpty(role) || content is null)
                throw new InvalidOperationException("Each chat message must have 'role' and 'content' properties.");

            messages.Add((role, content));
        }

        if (messages.Count == 0)
            throw new InvalidOperationException("Chat prompt must contain at least one message.");

        // Find first system message for Instructions
        var instructions = string.Empty;
        var baseMessages = new List<ChatMessage>();
        var foundSystemForInstructions = false;

        foreach (var (role, content) in messages)
        {
            var renderedContent = RenderTemplate(content, variables);

            if (!foundSystemForInstructions && role.Equals("system", StringComparison.OrdinalIgnoreCase))
            {
                instructions = renderedContent;
                foundSystemForInstructions = true;
                continue;
            }

            baseMessages.Add(new ChatMessage(new ChatRole(role), renderedContent));
        }

        return new ParsedPrompt
        {
            Instructions = instructions,
            BaseMessages = baseMessages,
            IsChatPrompt = true
        };
    }

    private static string RenderTemplate(string template, IReadOnlyDictionary<string, object?>? variables)
    {
        if (variables is null || variables.Count == 0)
            return template;

        // Stubble expects IDictionary, convert if needed
        var dict = variables as IDictionary<string, object?> ?? new Dictionary<string, object?>(variables);
        return Stubble.Render(template, dict);
    }
}
