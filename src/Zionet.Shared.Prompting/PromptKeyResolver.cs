using Zionet.Shared.Prompting.Exceptions;
using Zionet.Shared.Prompting.Models;

namespace Zionet.Shared.Prompting;

/// <summary>
/// Resolves a slash-style prompt key (e.g. "chat/qa") to a concrete file path under the prompts root.
/// File layout: {root}/{folder1}/{folder2}/.../{name}/{vN}.promptx.
/// </summary>
public sealed class PromptKeyResolver
{
    private readonly string _rootPath;
    private readonly PromptFolderConfigLoader _configLoader;

    /// <summary>
    /// Creates a resolver rooted at the given absolute prompts directory.
    /// </summary>
    public PromptKeyResolver(string rootPath)
    {
        _rootPath = rootPath;
        _configLoader = new PromptFolderConfigLoader();
    }

    /// <summary>
    /// Resolves a key and optional version/label selectors to an absolute file path.
    /// </summary>
    /// <returns>Absolute file path, or null when no matching file exists.</returns>
    /// <exception cref="PromptResolutionException">When the key or selectors are invalid, or when the folder configuration is malformed.</exception>
    public string? Resolve(string key, int? version, string? label)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new PromptResolutionException("Prompt key must not be empty.");

        if (version.HasValue && !string.IsNullOrWhiteSpace(label))
        {
            throw new PromptResolutionException(
                $"Prompt key '{key}' cannot specify both an explicit version and a label.");
        }

        if (version.HasValue && version.Value <= 0)
        {
            throw new PromptResolutionException(
                $"Prompt key '{key}' requires a positive integer version selector.");
        }

        var normalizedKey = NormalizeKey(key);
        var folderSegments = normalizedKey.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (folderSegments.Length == 0)
        {
            throw new PromptResolutionException(
                $"Prompt key '{key}' resolves to an empty folder path.");
        }

        ValidateSegments(folderSegments, normalizedKey);

        var folder = Path.Combine(new[] { _rootPath }.Concat(folderSegments).ToArray());
        if (!Directory.Exists(folder))
        {
            return null;
        }

        var config = _configLoader.Load(folder, normalizedKey);
        var selectedVersion = version ?? ResolveConfiguredVersion(config, normalizedKey, label);
        var file = Path.Combine(folder, $"v{selectedVersion}.promptx");

        if (File.Exists(file))
        {
            return file;
        }

        if (version.HasValue)
        {
            return null;
        }

        throw new PromptResolutionException(
            $"Prompt key '{normalizedKey}' resolved to configured version 'v{selectedVersion}' but '{file}' does not exist.");
    }

    private static string NormalizeKey(string key)
    {
        return key.Trim().Replace('\\', '/');
    }

    private static void ValidateSegments(IEnumerable<string> segments, string key)
    {
        foreach (var segment in segments)
        {
            if (segment is "." or "..")
            {
                throw new PromptResolutionException(
                    $"Prompt key '{key}' contains an invalid path segment '{segment}'.");
            }
        }
    }

    private static int ResolveConfiguredVersion(PromptFolderConfig config, string key, string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return config.DefaultVersion;
        }

        var normalizedLabel = label.Trim();
        if (config.Labels.TryGetValue(normalizedLabel, out var configuredVersion))
        {
            return configuredVersion;
        }

        throw new PromptResolutionException(
            $"Prompt key '{key}' does not define label '{normalizedLabel}'.");
    }
}
