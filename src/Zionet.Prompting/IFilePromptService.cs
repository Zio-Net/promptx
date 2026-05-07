namespace Zionet.Prompting;

/// <summary>
/// File-based prompt service. Mirrors the shape of the PromptProvider package's IPromptService
/// so call sites can switch with a using-change + DI swap.
/// </summary>
public interface IFilePromptService
{
    /// <summary>
    /// Loads a text-style prompt by key.
    /// </summary>
    /// <param name="key">Slash-based prompt key (for example "text/summarize").</param>
    /// <param name="version">Optional explicit numeric version (for example 2, resolved to v2.prmpt.md).</param>
    /// <param name="label">Optional label such as production or test, resolved via the folder's config.json.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Loaded text prompt, or null when the prompt cannot be found.</returns>
    Task<TextPromptResponse?> GetPromptAsync(
        string key,
        int? version = null,
        string? label = null,
        CancellationToken ct = default);

    /// <summary>
    /// Loads a chat-style prompt by key.
    /// </summary>
    /// <param name="key">Slash-based prompt key (for example "chat/qa").</param>
    /// <param name="version">Optional explicit numeric version (for example 2, resolved to v2.prmpt.md).</param>
    /// <param name="label">Optional label such as production or test, resolved via the folder's config.json.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Loaded chat prompt, or null when the prompt cannot be found.</returns>
    Task<ChatPromptResponse?> GetChatPromptAsync(
        string key,
        int? version = null,
        string? label = null,
        CancellationToken ct = default);
}
