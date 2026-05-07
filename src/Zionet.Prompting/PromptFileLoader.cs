using System.Collections.Concurrent;
using Zionet.Prompting.Exceptions;
using Zionet.Prompting.Models;
using Zionet.Prompting.Parsing;
using Zionet.Prompting.Validation;

namespace Zionet.Prompting;

/// <summary>
/// Loads, parses, validates, and caches PromptDefinition instances from .prmpt.md files on disk.
/// </summary>
internal sealed class PromptFileLoader
{
    private readonly PromptKeyResolver _resolver;
    private readonly SchemaValidator _schemaValidator;
    private readonly ConcurrentDictionary<string, PromptDefinition> _cache = new();

    /// <summary>
    /// Creates a loader for the given prompts root and JSON schema file.
    /// </summary>
    public PromptFileLoader(string promptsRootPath, string schemaFilePath)
    {
        _resolver = new PromptKeyResolver(promptsRootPath);
        _schemaValidator = new SchemaValidator(schemaFilePath);
    }

    /// <summary>
    /// Loads a prompt definition by key, returning null when the file does not exist.
    /// </summary>
    public PromptDefinition? Load(string key, int? version, string? label)
    {
        var path = _resolver.Resolve(key, version, label);
        if (path is null)
            return null;

        return _cache.GetOrAdd(path, LoadFromDisk);
    }

    private PromptDefinition LoadFromDisk(string path)
    {
        var source = File.ReadAllText(path);
        var (yaml, body) = FrontmatterSplitter.Split(source, path);

        _schemaValidator.Validate(yaml, path);

        var fm = FrontmatterParser.Parse(yaml, path);
        var (description, type) = FrontmatterParser.ToFields(fm.Root, path);
        var name = ResolvePromptName(path);
        var version = ResolvePromptVersion(path);

        var chatMessages = type == PromptType.Chat
            ? ChatBodyParser.Parse(body, path)
            : Array.Empty<ChatPromptMessage>();

        var definition = new PromptDefinition
        {
            Name = name,
            Description = description,
            Version = version,
            Type = type,
            Body = body,
            ChatMessages = chatMessages,
            SourcePath = path,
        };

        SemanticValidator.Validate(definition);
        return definition;
    }

    private static string ResolvePromptName(string path)
    {
        var folderName = Directory.GetParent(path)?.Name;
        if (string.IsNullOrWhiteSpace(folderName))
        {
            throw new PromptParsingException(
                $"Prompt file '{path}' is missing a containing folder.");
        }

        return folderName;
    }

    private static string ResolvePromptVersion(string path)
    {
        var fileVersion = Path.GetFileNameWithoutExtension(path);
        if (string.IsNullOrWhiteSpace(fileVersion))
        {
            throw new PromptParsingException(
                $"Prompt file '{path}' is missing a file name version.");
        }

        return fileVersion;
    }
}
