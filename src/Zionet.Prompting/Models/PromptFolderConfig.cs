namespace Zionet.Prompting.Models;

/// <summary>
/// Per-prompt folder configuration that drives default-version and label resolution.
/// </summary>
internal sealed record PromptFolderConfig
{
    /// <summary>
    /// Default numeric version to use when the caller does not specify a version or label.
    /// </summary>
    public required int DefaultVersion { get; init; }

    /// <summary>
    /// Label-to-version mappings for friendly selectors such as production or test.
    /// </summary>
    public required IReadOnlyDictionary<string, int> Labels { get; init; }
}
