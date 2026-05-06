using System.Text.Json;
using System.Text.Json.Serialization;
using Zionet.Prompting.Exceptions;
using Zionet.Prompting.Models;

namespace Zionet.Prompting;

/// <summary>
/// Loads and validates per-prompt folder configuration from config.json.
/// </summary>
internal sealed class PromptFolderConfigLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// Loads the config for a prompt folder.
    /// </summary>
    public PromptFolderConfig Load(string folderPath, string key)
    {
        var configPath = Path.Combine(folderPath, "config.json");
        if (!File.Exists(configPath))
        {
            throw new PromptResolutionException(
                $"Prompt key '{key}' is missing required config.json under '{folderPath}'.");
        }

        PromptFolderConfigDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<PromptFolderConfigDocument>(
                File.ReadAllText(configPath),
                JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new PromptResolutionException(
                $"Prompt key '{key}' has invalid JSON in '{configPath}': {ex.Message}");
        }

        if (document is null)
        {
            throw new PromptResolutionException(
                $"Prompt key '{key}' has an empty or unreadable config.json.");
        }

        var defaultVersion = document.DefaultVersion ?? document.LegacyDefaultVersion;
        if (!defaultVersion.HasValue || defaultVersion.Value <= 0)
        {
            throw new PromptResolutionException(
                $"Prompt key '{key}' must define a positive defaultVersion in '{configPath}'.");
        }

        var labels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (document.Labels is not null)
        {
            foreach (var pair in document.Labels)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    throw new PromptResolutionException(
                        $"Prompt key '{key}' contains a blank label name in '{configPath}'.");
                }

                if (pair.Value <= 0)
                {
                    throw new PromptResolutionException(
                        $"Prompt key '{key}' label '{pair.Key}' must point to a positive version in '{configPath}'.");
                }

                labels[pair.Key.Trim()] = pair.Value;
            }
        }

        return new PromptFolderConfig
        {
            DefaultVersion = defaultVersion.Value,
            Labels = labels,
        };
    }

    private sealed class PromptFolderConfigDocument
    {
        public int? DefaultVersion { get; init; }

        [JsonPropertyName("default-version")]
        public int? LegacyDefaultVersion { get; init; }

        public Dictionary<string, int>? Labels { get; init; }
    }
}
