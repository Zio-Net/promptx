using Zionet.Prompting.Exceptions;

namespace Zionet.Prompting.Tests;

public sealed class FilePromptServiceTests
{
    [Fact]
    public async Task GetPromptAsync_returns_raw_body_for_text_promptAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/summarize", 1, ("production", 1));
        fx.WritePrompt("text/summarize/v1.prmpt.md",
            """
            ---
            type: prompt
            ---

            Summarize: {{text}}
            """);
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        var result = await service.GetPromptAsync("text/summarize");

        Assert.NotNull(result);
        Assert.Contains("{{text}}", result!.Content);
        Assert.StartsWith("Summarize:", result.Content);
    }

    [Fact]
    public async Task GetChatPromptAsync_parses_role_blocks_in_orderAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("chat/qa", 1, ("production", 1));
        fx.WritePrompt("chat/qa/v1.prmpt.md",
            """
            ---
            type: chat
            ---

            system:
            You are helpful.

            developer:
            Keep the answer concise.

            user:
            {{question}}
            """);
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        var result = await service.GetChatPromptAsync("chat/qa");

        Assert.NotNull(result);
        Assert.Equal(3, result!.ChatMessages.Length);
        Assert.Equal("system", result.ChatMessages[0].Role);
        Assert.Equal("You are helpful.", result.ChatMessages[0].Content);
        Assert.Equal("developer", result.ChatMessages[1].Role);
        Assert.Equal("Keep the answer concise.", result.ChatMessages[1].Content);
        Assert.Equal("user", result.ChatMessages[2].Role);
        Assert.Equal("{{question}}", result.ChatMessages[2].Content);
    }

    [Fact]
    public async Task GetPromptAsync_returns_null_when_key_does_not_existAsync()
    {
        using var fx = new PromptingTestFixture();
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        var result = await service.GetPromptAsync("text/does-not-exist");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPromptAsync_uses_default_version_from_config_when_selector_omittedAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/summarize", 3, ("production", 3));
        fx.WritePrompt("text/summarize/v1.prmpt.md",
            """
            ---
            type: prompt
            ---

            Old version body.
            """);
        fx.WritePrompt("text/summarize/v3.prmpt.md",
            """
            ---
            type: prompt
            ---

            Newest version body.
            """);
        fx.WritePrompt("text/summarize/v2.prmpt.md",
            """
            ---
            type: prompt
            ---

            Middle version body.
            """);
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        var result = await service.GetPromptAsync("text/summarize");

        Assert.NotNull(result);
        Assert.Contains("Newest version", result!.Content);
    }

    [Fact]
    public async Task GetPromptAsync_uses_label_from_config_when_providedAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/summarize", 1, ("production", 1), ("test", 2));
        fx.WritePrompt("text/summarize/v1.prmpt.md",
            """
            ---
            type: prompt
            ---

            Production body.
            """);
        fx.WritePrompt("text/summarize/v2.prmpt.md",
            """
            ---
            type: prompt
            ---

            Test body.
            """);
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        var result = await service.GetPromptAsync("text/summarize", label: "test");

        Assert.NotNull(result);
        Assert.Contains("Test body", result!.Content);
    }

    [Fact]
    public async Task GetPromptAsync_throws_when_text_key_resolves_to_chat_promptAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("chat/qa", 1, ("production", 1));
        fx.WritePrompt("chat/qa/v1.prmpt.md",
            """
            ---
            type: chat
            ---

            system:
            Hi.

            user:
            {{question}}
            """);
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        await Assert.ThrowsAsync<PromptResolutionException>(() =>
            service.GetPromptAsync("chat/qa"));
    }

    [Fact]
    public async Task GetPromptAsync_allows_variables_without_inputs_declarationAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/bad", 1, ("production", 1));
        fx.WritePrompt("text/bad/v1.prmpt.md",
            """
            ---
            type: prompt
            ---

            Uses {{notDeclared}} variable.
            """);
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        var result = await service.GetPromptAsync("text/bad");

        Assert.NotNull(result);
        Assert.Equal("Uses {{notDeclared}} variable.", result!.Content);
    }

    [Fact]
    public async Task GetChatPromptAsync_allows_chat_prompt_with_only_system_blockAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("chat/no-user", 1, ("production", 1));
        fx.WritePrompt("chat/no-user/v1.prmpt.md",
            """
            ---
            type: chat
            ---

            system:
            Only a system block here.
            """);
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        var result = await service.GetChatPromptAsync("chat/no-user");

        Assert.NotNull(result);
        Assert.Single(result!.ChatMessages);
        Assert.Equal("system", result.ChatMessages[0].Role);
        Assert.Equal("Only a system block here.", result.ChatMessages[0].Content);
    }

    [Fact]
    public async Task GetPromptAsync_throws_PromptSchemaException_for_invalid_typeAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/bad-type", 1, ("production", 1));
        fx.WritePrompt("text/bad-type/v1.prmpt.md",
            """
            ---
            type: not-a-real-type
            ---

            body
            """);
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        await Assert.ThrowsAsync<PromptSchemaException>(() =>
            service.GetPromptAsync("text/bad-type"));
    }

    [Fact]
    public async Task GetPromptAsync_throws_PromptParsingException_when_frontmatter_missingAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/no-fm", 1, ("production", 1));
        fx.WritePrompt("text/no-fm/v1.prmpt.md", "Just body, no frontmatter.\n");
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        await Assert.ThrowsAsync<PromptParsingException>(() =>
            service.GetPromptAsync("text/no-fm"));
    }

    [Fact]
    public async Task GetPromptAsync_returns_null_when_explicit_version_missesAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/summarize", 1, ("production", 1));
        fx.WritePrompt("text/summarize/v1.prmpt.md",
            """
            ---
            type: prompt
            ---

            v1 body
            """);
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        var result = await service.GetPromptAsync("text/summarize", version: 9);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPromptAsync_throws_PromptSchemaException_when_legacy_name_or_version_fields_are_presentAsync()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/version-mismatch", 2, ("production", 2));
        fx.WritePrompt("text/version-mismatch/v2.prmpt.md",
            """
            ---
            name: version-mismatch
            description: Legacy frontmatter fields should be rejected.
            version: v1
            type: prompt
            ---

            Broken prompt.
            """);
        var service = new FilePromptService(fx.PromptsDir, fx.SchemaPath);

        await Assert.ThrowsAsync<PromptSchemaException>(() =>
            service.GetPromptAsync("text/version-mismatch"));
    }
}
