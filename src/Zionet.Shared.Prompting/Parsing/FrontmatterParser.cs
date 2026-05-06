using Zionet.Prompting.Exceptions;
using Zionet.Prompting.Models;
using YamlDotNet.RepresentationModel;

namespace Zionet.Prompting.Parsing;

/// <summary>
/// Parses YAML frontmatter into the strongly-typed metadata fields of a <see cref="PromptDefinition"/>.
/// </summary>
internal static class FrontmatterParser
{
    /// <summary>
    /// Parses the given YAML frontmatter into a partial PromptDefinition (Body / ChatMessages filled by caller).
    /// </summary>
    public static FrontmatterData Parse(string yaml, string sourcePath)
    {
        var yamlStream = new YamlStream();
        try
        {
            yamlStream.Load(new StringReader(yaml));
        }
        catch (Exception ex)
        {
            throw new PromptParsingException(
                $"Frontmatter YAML in '{sourcePath}' is invalid: {ex.Message}", ex);
        }

        if (yamlStream.Documents.Count == 0)
        {
            throw new PromptParsingException(
                $"Frontmatter in '{sourcePath}' is empty.");
        }

        if (yamlStream.Documents[0].RootNode is not YamlMappingNode root)
        {
            throw new PromptParsingException(
                $"Frontmatter in '{sourcePath}' must be a YAML mapping.");
        }

        return new FrontmatterData
        {
            Root = root,
            RawYaml = yaml,
            SourcePath = sourcePath,
        };
    }

    /// <summary>
    /// Builds the strongly-typed PromptDefinition fields from the YAML root.
    /// Assumes the YAML has already passed JSON Schema validation.
    /// </summary>
    public static (string? Description, PromptType Type)
        ToFields(YamlMappingNode root, string sourcePath)
    {
        var description = TryGetScalar(root, "description");
        var typeRaw = GetRequiredScalar(root, "type", sourcePath);
        var type = typeRaw switch
        {
            "prompt" => PromptType.Prompt,
            "chat" => PromptType.Chat,
            _ => throw new PromptParsingException(
                $"Prompt '{sourcePath}' has unknown type '{typeRaw}' (expected 'prompt' or 'chat')."),
        };

        return (description, type);
    }

    private static string GetRequiredScalar(YamlMappingNode root, string key, string sourcePath)
    {
        var value = TryGetScalar(root, key);
        return value ?? throw new PromptParsingException(
            $"Frontmatter in '{sourcePath}' is missing required field '{key}'.");
    }

    private static string? TryGetScalar(YamlMappingNode root, string key)
    {
        return root.Children.TryGetValue(new YamlScalarNode(key), out var node) && node is YamlScalarNode scalar
            ? scalar.Value
            : null;
    }
}

/// <summary>
/// Container for the parsed YAML root + source location, used between parsing stages.
/// </summary>
internal sealed record FrontmatterData
{
    public required YamlMappingNode Root { get; init; }
    public required string RawYaml { get; init; }
    public required string SourcePath { get; init; }
}
