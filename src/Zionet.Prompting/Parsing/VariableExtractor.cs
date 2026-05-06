using System.Text.RegularExpressions;

namespace Zionet.Prompting.Parsing;

/// <summary>
/// Extracts {{variable}} names referenced inside a prompt body or chat-message content.
/// </summary>
internal static class VariableExtractor
{
    private static readonly Regex VariablePattern = new(
        @"\{\{\s*([A-Za-z_][A-Za-z0-9_]*)\s*\}\}",
        RegexOptions.Compiled);

    /// <summary>
    /// Returns the distinct variable names referenced in the given text.
    /// </summary>
    public static IReadOnlySet<string> Extract(string text)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in VariablePattern.Matches(text))
        {
            result.Add(match.Groups[1].Value);
        }

        return result;
    }
}
