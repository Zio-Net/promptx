using Zionet.Prompting.Exceptions;

namespace Zionet.Prompting.Tests;

public sealed class PromptKeyResolverTests
{
    [Fact]
    public void Resolve_with_explicit_version_finds_file()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/foo", 1, ("production", 1));
        var path = fx.WritePrompt("text/foo/v1.prmpt.md", "ignored");
        var resolver = new PromptKeyResolver(fx.PromptsDir);

        var resolved = resolver.Resolve("text/foo", version: 1, label: null);

        Assert.Equal(path, resolved);
    }

    [Fact]
    public void Resolve_with_label_uses_config_mapping()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/foo", 1, ("production", 1), ("test", 2));
        fx.WritePrompt("text/foo/v1.prmpt.md", "v1");
        var pathV2 = fx.WritePrompt("text/foo/v2.prmpt.md", "v2");
        var resolver = new PromptKeyResolver(fx.PromptsDir);

        var resolved = resolver.Resolve("text/foo", version: null, label: "test");

        Assert.Equal(pathV2, resolved);
    }

    [Fact]
    public void Resolve_without_selector_uses_config_default_version()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/foo", 2, ("production", 2));
        fx.WritePrompt("text/foo/v1.prmpt.md", "v1");
        var pathV2 = fx.WritePrompt("text/foo/v2.prmpt.md", "v2");
        var resolver = new PromptKeyResolver(fx.PromptsDir);

        var resolved = resolver.Resolve("text/foo", version: null, label: null);

        Assert.Equal(pathV2, resolved);
    }

    [Fact]
    public void Resolve_throws_when_both_version_and_label_are_provided()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/foo", 1, ("production", 1));
        fx.WritePrompt("text/foo/v1.prmpt.md", "x");
        var resolver = new PromptKeyResolver(fx.PromptsDir);

        Assert.Throws<PromptResolutionException>(() =>
            resolver.Resolve("text/foo", version: 1, label: "production"));
    }

    [Fact]
    public void Resolve_returns_null_when_version_does_not_exist()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePromptConfig("text/foo", 1, ("production", 1));
        fx.WritePrompt("text/foo/v1.prmpt.md", "v1");
        var resolver = new PromptKeyResolver(fx.PromptsDir);

        var resolved = resolver.Resolve("text/foo", version: 9, label: null);

        Assert.Null(resolved);
    }

    [Fact]
    public void Resolve_returns_null_when_prompt_folder_does_not_exist()
    {
        using var fx = new PromptingTestFixture();
        var resolver = new PromptKeyResolver(fx.PromptsDir);

        var resolved = resolver.Resolve("text/does-not-exist", version: null, label: null);

        Assert.Null(resolved);
    }

    [Fact]
    public void Resolve_throws_when_existing_prompt_folder_has_no_config()
    {
        using var fx = new PromptingTestFixture();
        fx.WritePrompt("text/foo/v1.prmpt.md", "v1");
        var resolver = new PromptKeyResolver(fx.PromptsDir);

        Assert.Throws<PromptResolutionException>(() =>
            resolver.Resolve("text/foo", version: null, label: null));
    }
}
