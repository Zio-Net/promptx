using Zionet.Prompting.Exceptions;
using Zionet.Prompting.Models;

namespace Zionet.Prompting.Validation;

/// <summary>
/// Placeholder for PromptX checks that are not enforced by the JSON schema.
/// </summary>
internal static class SemanticValidator
{
    /// <summary>
    /// Validates a parsed prompt definition.
    /// </summary>
    public static void Validate(PromptDefinition prompt)
    {
        ArgumentNullException.ThrowIfNull(prompt);

        if (string.IsNullOrWhiteSpace(prompt.SourcePath))
        {
            throw new PromptParsingException("Prompt source path is missing.");
        }

        if (string.IsNullOrWhiteSpace(prompt.Version))
        {
            throw new PromptParsingException(
                $"Prompt file '{prompt.SourcePath}' must resolve to a version from its file name.");
        }

        if (string.IsNullOrWhiteSpace(prompt.Name))
        {
            throw new PromptParsingException(
                $"Prompt file '{prompt.SourcePath}' must resolve to a name from its containing folder.");
        }
    }
}
