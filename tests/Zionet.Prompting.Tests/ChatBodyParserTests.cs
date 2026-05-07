using Zionet.Prompting.Exceptions;
using Zionet.Prompting.Parsing;

namespace Zionet.Prompting.Tests;

public sealed class ChatBodyParserTests
{
    [Fact]
    public void Parse_supports_repeating_roles_and_developer_for_multi_turn()
    {
        var body =
            """
            system:
            You are helpful.

            developer:
            Keep the reply concise.

            user:
            First question.

            assistant:
            First answer.

            user:
            Follow-up.
            """;

        var messages = ChatBodyParser.Parse(body, "test.prmpt.md");

        Assert.Equal(5, messages.Count);
        Assert.Equal("system", messages[0].Role);
        Assert.Equal("developer", messages[1].Role);
        Assert.Equal("Keep the reply concise.", messages[1].Content);
        Assert.Equal("user", messages[2].Role);
        Assert.Equal("First question.", messages[2].Content);
        Assert.Equal("assistant", messages[3].Role);
        Assert.Equal("user", messages[4].Role);
        Assert.Equal("Follow-up.", messages[4].Content);
    }

    [Fact]
    public void Parse_rejects_indented_role_marker()
    {
        var body =
            """
             system:
            You are helpful.

            user:
            Hi.
            """;

        Assert.Throws<PromptParsingException>(() =>
            ChatBodyParser.Parse(body, "test.prmpt.md"));
    }

    [Fact]
    public void Parse_throws_when_no_role_markers_present()
    {
        var body = "Just plain text with no role lines.";

        Assert.Throws<PromptParsingException>(() =>
            ChatBodyParser.Parse(body, "test.prmpt.md"));
    }

    [Fact]
    public void Parse_throws_when_content_precedes_first_role()
    {
        var body =
            """
            Stray content.

            system:
            Hello.

            user:
            Hi.
            """;

        Assert.Throws<PromptParsingException>(() =>
            ChatBodyParser.Parse(body, "test.prmpt.md"));
    }

    [Fact]
    public void Parse_is_case_sensitive_on_role_names()
    {
        var body =
            """
            System:
            Wrong case.

            user:
            Hi.
            """;

        Assert.Throws<PromptParsingException>(() =>
            ChatBodyParser.Parse(body, "test.prmpt.md"));
    }
}
