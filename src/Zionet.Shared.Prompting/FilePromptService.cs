using Zionet.Shared.Prompting.Exceptions;
using Zionet.Shared.Prompting.Models;

namespace Zionet.Shared.Prompting;

/// <summary>
/// File-based implementation of <see cref="IPromptService"/>. Returns raw, un-rendered content
/// to mirror PromptProvider's contract (the caller's PromptParser performs variable substitution).
/// </summary>
public sealed class FilePromptService : IFilePromptService
{
    private readonly PromptFileLoader _loader;

    /// <summary>
    /// Creates the service rooted at the given prompts directory and JSON schema file.
    /// </summary>
    public FilePromptService(string promptsRootPath, string schemaFilePath)
    {
        _loader = new PromptFileLoader(promptsRootPath, schemaFilePath);
    }

    /// <inheritdoc />
    public Task<TextPromptResponse?> GetPromptAsync(
        string key,
        int? version = null,
        string? label = null,
        CancellationToken ct = default)
    {
        var definition = _loader.Load(key, version, label);
        if (definition is null)
            return Task.FromResult<TextPromptResponse?>(null);

        if (definition.Type != PromptType.Prompt)
        {
            throw new PromptResolutionException(
                $"Prompt key '{key}' resolved to a chat prompt; use GetChatPromptAsync instead.");
        }

        var response = new TextPromptResponse
        {
            Content = definition.Body,
        };
        return Task.FromResult<TextPromptResponse?>(response);
    }

    /// <inheritdoc />
    public Task<ChatPromptResponse?> GetChatPromptAsync(
        string key,
        int? version = null,
        string? label = null,
        CancellationToken ct = default)
    {
        var definition = _loader.Load(key, version, label);
        if (definition is null)
            return Task.FromResult<ChatPromptResponse?>(null);

        if (definition.Type != PromptType.Chat)
        {
            throw new PromptResolutionException(
                $"Prompt key '{key}' resolved to a text prompt; use GetPromptAsync instead.");
        }

        var response = new ChatPromptResponse
        {
            ChatMessages = definition.ChatMessages.ToArray(),
        };
        return Task.FromResult<ChatPromptResponse?>(response);
    }
}
