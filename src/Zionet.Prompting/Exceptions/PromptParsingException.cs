namespace Zionet.Prompting.Exceptions;

/// <summary>
/// Thrown when a .prmpt.md file is structurally malformed (missing frontmatter delimiters, invalid YAML, etc.).
/// </summary>
public sealed class PromptParsingException : PromptException
{
    /// <summary>
    /// Creates a parsing exception with the given message.
    /// </summary>
    public PromptParsingException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a parsing exception with the given message and inner exception.
    /// </summary>
    public PromptParsingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
