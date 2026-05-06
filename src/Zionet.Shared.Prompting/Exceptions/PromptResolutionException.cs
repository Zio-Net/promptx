namespace Zionet.Shared.Prompting.Exceptions;

/// <summary>
/// Thrown when a prompt key cannot be resolved to a single file
/// (no matching file, ambiguous "latest" resolution, missing version, ...).
/// </summary>
/// <remarks>
/// Creates a resolution exception with the given message.
/// </remarks>
public sealed class PromptResolutionException(string message) : PromptException(message)
{
}
