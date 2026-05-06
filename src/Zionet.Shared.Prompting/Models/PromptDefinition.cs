namespace Zionet.Shared.Prompting.Models;

/// <summary>
/// Internal representation of a parsed and validated .promptx file.
/// </summary>
internal sealed record PromptDefinition
{
    /// <summary>Prompt name derived from the containing folder name.</summary>
    public required string Name { get; init; }

    /// <summary>Optional human-readable description.</summary>
    public string? Description { get; init; }

    /// <summary>Version string derived from the file name (for example, "v1").</summary>
    public string? Version { get; init; }

    /// <summary>Body shape (text or chat).</summary>
    public required PromptType Type { get; init; }

    /// <summary>
    /// Raw text body (for <see cref="PromptType.Prompt"/>) or the un-split source (for <see cref="PromptType.Chat"/>).
    /// </summary>
    public required string Body { get; init; }

    /// <summary>
    /// Parsed chat messages when <see cref="Type"/> is <see cref="PromptType.Chat"/>; otherwise empty.
    /// </summary>
    public required IReadOnlyList<ChatPromptMessage> ChatMessages { get; init; }

    /// <summary>Path to the source file on disk (diagnostics).</summary>
    public required string SourcePath { get; init; }
}
