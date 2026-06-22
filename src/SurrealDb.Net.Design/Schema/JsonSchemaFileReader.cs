using System.Text.Json;
using SurrealDb.Net.Design.Cli;

namespace SurrealDb.Net.Design.Schema;

internal sealed class JsonSchemaFileReader(string schemaFile) : ISchemaReader
{
    public async Task<DatabaseSchema> ReadAsync(CancellationToken cancellationToken)
    {
        await using FileStream stream = File.OpenRead(schemaFile);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        JsonElement root = document.RootElement;

        if (!root.TryGetProperty("tables", out JsonElement tables) || tables.ValueKind != JsonValueKind.Array)
        {
            throw new CliException("Schema file must contain a 'tables' array.");
        }

        List<RecordSchema> records = [];
        foreach (JsonElement table in tables.EnumerateArray())
        {
            string name = table.GetProperty("name").GetString() ?? throw new CliException("Every table entry needs a name.");
            records.Add(new RecordSchema(name, ReadFields(table, name)));
        }

        return new DatabaseSchema(records);
    }

    private static IReadOnlyList<FieldSchema> ReadFields(JsonElement table, string tableName)
    {
        if (!table.TryGetProperty("fields", out JsonElement fields) || fields.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        List<FieldSchema> result = [];
        foreach (JsonElement field in fields.EnumerateArray())
        {
            string fieldName = field.GetProperty("name").GetString() ?? throw new CliException($"A field on table '{tableName}' is missing a name.");
            string? kind = field.TryGetProperty("kind", out JsonElement kindElement) ? kindElement.GetString() : null;
            result.Add(new FieldSchema(fieldName, kind));
        }

        return result;
    }
}
