namespace Zionet.Shared.Prompting.Models;

/// <summary>
/// Discriminator for the prompt body shape.
/// </summary>
internal enum PromptType
{
    /// <summary>
    /// Free-form text prompt body.
    /// </summary>
    Prompt,

    /// <summary>
    /// Role-based chat body using "system:", "user:", "assistant:" blocks.
    /// </summary>
    Chat,
}
