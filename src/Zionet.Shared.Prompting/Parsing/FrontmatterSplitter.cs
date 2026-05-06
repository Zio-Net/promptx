using Zionet.Shared.Prompting.Exceptions;

namespace Zionet.Shared.Prompting.Parsing;

/// <summary>
/// Splits a .promptx file into raw YAML frontmatter and the body that follows it.
/// </summary>
internal static class FrontmatterSplitter
{
    private const string Delimiter = "---";

    /// <summary>
    /// Splits the given source into (yaml, body).
    /// </summary>
    /// <param name="source">Full file contents.</param>
    /// <param name="sourcePath">Path used in error messages.</param>
    /// <returns>Tuple of (frontmatter YAML, body text).</returns>
    /// <exception cref="PromptParsingException">When the file is missing the frontmatter delimiters.</exception>
    public static (string Yaml, string Body) Split(string source, string sourcePath)
    {
        var lines = source.Split('\n');

        if (lines.Length < 3 || lines[0].TrimEnd('\r') != Delimiter)
        {
            throw new PromptParsingException(
                $"Prompt file '{sourcePath}' must start with '---' on the first line.");
        }

        var closingIndex = -1;
        for (var i = 1; i < lines.Length; i++)
        {
            if (lines[i].TrimEnd('\r') == Delimiter)
            {
                closingIndex = i;
                break;
            }
        }

        if (closingIndex < 0)
        {
            throw new PromptParsingException(
                $"Prompt file '{sourcePath}' is missing the closing '---' frontmatter delimiter.");
        }

        var yaml = string.Join('\n', lines, 1, closingIndex - 1);
        var bodyLines = lines.Skip(closingIndex + 1).ToArray();
        var body = string.Join('\n', bodyLines).TrimStart('\r', '\n');

        return (yaml, body);
    }
}
