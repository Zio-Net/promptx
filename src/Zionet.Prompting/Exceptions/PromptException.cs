namespace Zionet.Prompting.Exceptions;

/// <summary>
/// Base type for all file-prompt errors. Allows callers to catch the whole family at once.
/// </summary>
public abstract class PromptException : Exception
{
    /// <summary>
    /// Creates a prompt exception with the given message.
    /// </summary>
    protected PromptException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a prompt exception with the given message and inner exception.
    /// </summary>
    protected PromptException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
