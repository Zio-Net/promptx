using System.Globalization;
using System.Text;
using System.Text.Json;
using Zionet.Prompting.Exceptions;
using NJsonSchema;
using YamlDotNet.RepresentationModel;

namespace Zionet.Prompting.Validation;

/// <summary>
/// Validates a YAML frontmatter document against the .promptx JSON Schema.
/// </summary>
internal sealed class SchemaValidator
{
    private readonly JsonSchema _schema;

    /// <summary>
    /// Creates a validator that uses the schema loaded from the given path.
    /// </summary>
    public SchemaValidator(string schemaFilePath)
    {
        var schemaJson = File.ReadAllText(schemaFilePath);
        _schema = JsonSchema.FromJsonAsync(schemaJson).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Validates the YAML frontmatter (provided as a YAML string).
    /// </summary>
    /// <param name="yaml">Raw YAML frontmatter.</param>
    /// <param name="sourcePath">Path used in error messages.</param>
    /// <exception cref="PromptSchemaException">When validation fails.</exception>
    public void Validate(string yaml, string sourcePath)
    {
        var json = ConvertYamlToJson(yaml, sourcePath);
        var errors = _schema.Validate(json);

        if (errors.Count == 0)
            return;

        var messages = errors
            .Select(e => $"{e.Path}: {e.Kind}")
            .ToArray();
        throw new PromptSchemaException(
            $"Frontmatter in '{sourcePath}' failed schema validation: {string.Join("; ", messages)}",
            messages);
    }

    private static string ConvertYamlToJson(string yaml, string sourcePath)
    {
        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));

            if (stream.Documents.Count == 0)
                return "{}";

            using var ms = new MemoryStream();
            using (var jsonWriter = new Utf8JsonWriter(ms))
            {
                WriteNode(jsonWriter, stream.Documents[0].RootNode);
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }
        catch (Exception ex)
        {
            throw new PromptParsingException(
                $"Frontmatter in '{sourcePath}' could not be converted to JSON for validation: {ex.Message}", ex);
        }
    }

    private static void WriteNode(Utf8JsonWriter writer, YamlNode node)
    {
        switch (node)
        {
            case YamlMappingNode map:
                writer.WriteStartObject();
                foreach (var (keyNode, valueNode) in map.Children)
                {
                    var key = ((YamlScalarNode)keyNode).Value ?? string.Empty;
                    writer.WritePropertyName(key);
                    WriteNode(writer, valueNode);
                }

                writer.WriteEndObject();
                break;

            case YamlSequenceNode seq:
                writer.WriteStartArray();
                foreach (var item in seq.Children)
                {
                    WriteNode(writer, item);
                }

                writer.WriteEndArray();
                break;

            case YamlScalarNode scalar:
                WriteScalar(writer, scalar);
                break;

            default:
                writer.WriteNullValue();
                break;
        }
    }

    private static void WriteScalar(Utf8JsonWriter writer, YamlScalarNode scalar)
    {
        var raw = scalar.Value;

        if (scalar.Style == YamlDotNet.Core.ScalarStyle.SingleQuoted
            || scalar.Style == YamlDotNet.Core.ScalarStyle.DoubleQuoted)
        {
            writer.WriteStringValue(raw ?? string.Empty);
            return;
        }

        if (raw is null || raw == "null" || raw == "~")
        {
            writer.WriteNullValue();
            return;
        }

        if (raw == "true")
        {
            writer.WriteBooleanValue(true);
            return;
        }

        if (raw == "false")
        {
            writer.WriteBooleanValue(false);
            return;
        }

        if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
        {
            writer.WriteNumberValue(longValue);
            return;
        }

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
        {
            writer.WriteNumberValue(doubleValue);
            return;
        }

        writer.WriteStringValue(raw);
    }
}
