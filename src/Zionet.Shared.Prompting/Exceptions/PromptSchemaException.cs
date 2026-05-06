namespace Zionet.Prompting.Exceptions;

/// <summary>
/// Thrown when frontmatter metadata fails JSON Schema validation (missing required fields, unknown properties, bad enum values, ...).
/// </summary>
/// <remarks>
/// Creates a schema exception with the given errors.
/// </remarks>
public sealed class PromptSchemaException(string message, IReadOnlyList<string> errors) : PromptException(message)
{
    /// <summary>
    /// Aggregated schema-error messages, one per failure.
    /// </summary>
    public IReadOnlyList<string> Errors { get; } = errors;
}
