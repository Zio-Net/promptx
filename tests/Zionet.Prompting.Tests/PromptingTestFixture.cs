using System.Text.Json;

namespace Zionet.Prompting.Tests;

/// <summary>
/// Creates a per-test temp directory and seeds the schema file so loaders can run in isolation.
/// </summary>
internal sealed class PromptingTestFixture : IDisposable
{
    public string Root { get; }
    public string PromptsDir { get; }
    public string SchemaPath { get; }

    public PromptingTestFixture()
    {
        Root = Path.Combine(Path.GetTempPath(), "prmptmd-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);

        PromptsDir = Path.Combine(Root, "Prompts");
        Directory.CreateDirectory(PromptsDir);

        var schemaSource = LocateSchemaFile();
        SchemaPath = Path.Combine(Root, "prmptmd.schema.json");
        File.Copy(schemaSource, SchemaPath);
    }

    /// <summary>
    /// Writes a .prmpt.md file at the given relative path under the prompts directory.
    /// </summary>
    public string WritePrompt(string relativePath, string contents)
    {
        var full = Path.Combine(PromptsDir, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, contents);
        return full;
    }

    /// <summary>
    /// Writes a config.json file for the given prompt folder.
    /// </summary>
    public string WritePromptConfig(
        string relativeFolderPath,
        int defaultVersion,
        params (string Label, int Version)[] labels)
    {
        var full = Path.Combine(
            PromptsDir,
            relativeFolderPath.Replace('/', Path.DirectorySeparatorChar),
            "config.json");

        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        var payload = new
        {
            defaultVersion,
            labels = labels.ToDictionary(pair => pair.Label, pair => pair.Version, StringComparer.OrdinalIgnoreCase),
        };

        File.WriteAllText(full, JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true,
        }));

        return full;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(Root, recursive: true);
        }
        catch
        {
            // Best-effort cleanup; ignore failures from antivirus locks etc.
        }
    }

    private static string LocateSchemaFile()
    {
        // Walk up from the test binary directory until we find the schema in the repo.
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            var candidate = Path.Combine(dir, "src", "Zionet.Prompting", "Assets", "Schemas", "prmptmd.schema.json");
            if (File.Exists(candidate))
                return candidate;

            var parent = Directory.GetParent(dir);
            if (parent is null)
                break;
            dir = parent.FullName;
        }

        throw new FileNotFoundException(
            "Could not locate prmptmd.schema.json from test base directory.");
    }
}
