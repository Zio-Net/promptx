namespace Zionet.Shared.Prompting.Exceptions;

/// <summary>
/// Thrown when a prompt is structurally valid but semantically inconsistent
/// (body uses an undeclared variable, chat lacks system/user, ...).
/// </summary>
/// <remarks>
/// Creates a validation exception with the given message.
/// </remarks>
public sealed class PromptValidationException(string message) : PromptException(message)
{
}
