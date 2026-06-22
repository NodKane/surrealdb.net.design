using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SurrealDb.Net.Design.Cli;

namespace SurrealDb.Net.Design.Schema;

internal sealed partial class SurrealDbHttpSchemaReader(ScaffoldOptions options) : ISchemaReader
{
    public async Task<DatabaseSchema> ReadAsync(CancellationToken cancellationToken)
    {
        using HttpClient client = CreateClient();

        var databaseInfo = await QuerySingleResultAsync(client, "INFO FOR DB;", cancellationToken);
        var tableNames = ReadTableNames(databaseInfo)
            .Where(table => options.Tables.Count == 0 || options.Tables.Contains(table))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var records = new List<RecordSchema>();
        foreach (string tableName in tableNames)
        {
            var tableInfo = await QuerySingleResultAsync(client, $"INFO FOR TABLE {SurrealIdentifier(tableName)} STRUCTURE;", cancellationToken);
            records.Add(new RecordSchema(tableName, ReadFields(tableInfo)));
        }

        return new DatabaseSchema(records);
    }

    private HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Surreal-NS", options.Namespace);
        client.DefaultRequestHeaders.Add("Surreal-DB", options.Database);

        if (!string.IsNullOrWhiteSpace(options.Token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Token);
        }
        else if (!string.IsNullOrWhiteSpace(options.User) || !string.IsNullOrWhiteSpace(options.Password))
        {
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.User}:{options.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
        }

        return client;
    }

    private async Task<JsonElement> QuerySingleResultAsync(HttpClient client, string query, CancellationToken cancellationToken)
    {
        using var content = new StringContent(query, Encoding.UTF8, "text/plain");
        using var response = await client.PostAsync(SqlEndpoint(options.Endpoint), content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new CliException($"SurrealDB returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        using var document = JsonDocument.Parse(body);
        var result = ReadFirstResult(document.RootElement.Clone());

        if (result.ValueKind == JsonValueKind.Undefined)
        {
            throw new CliException("SurrealDB returned an empty response.");
        }

        if (result.TryGetProperty("status", out JsonElement status) && string.Equals(status.GetString(), "ERR", StringComparison.OrdinalIgnoreCase))
        {
            var detail = result.TryGetProperty("detail", out JsonElement detailElement) 
                ? detailElement.ToString() 
                : result.ToString();
            throw new CliException($"SurrealDB query failed: {detail}");
        }

        return result.TryGetProperty("result", out JsonElement resultElement) 
            ? resultElement.Clone() 
            : result.Clone();
    }

    private static JsonElement ReadFirstResult(JsonElement root)
    {
        return root.ValueKind == JsonValueKind.Array ? root.EnumerateArray().FirstOrDefault() : root;
    }

    private static Uri SqlEndpoint(Uri endpoint)
    {
        if (endpoint.AbsolutePath.EndsWith("/sql", StringComparison.OrdinalIgnoreCase))
        {
            return endpoint;
        }

        var builder = new UriBuilder(endpoint);
        builder.Path = $"{builder.Path.TrimEnd('/')}/sql";
        return builder.Uri;
    }

    private static IReadOnlyList<string> ReadTableNames(JsonElement databaseInfo)
    {
        if (!databaseInfo.TryGetProperty("tables", out JsonElement tables))
        {
            return [];
        }

        return tables.ValueKind switch
        {
            JsonValueKind.Object => tables.EnumerateObject().Select(property => property.Name).ToArray(),
            JsonValueKind.Array => tables.EnumerateArray().Select(ReadName).Where(name => name is not null).Cast<string>().ToArray(),
            _ => []
        };
    }

    private static IReadOnlyList<FieldSchema> ReadFields(JsonElement tableInfo)
    {
        if (!tableInfo.TryGetProperty("fields", out JsonElement fields))
        {
            return [];
        }

        return fields.ValueKind switch
        {
            JsonValueKind.Array => fields.EnumerateArray().Select(ReadField).Where(field => field is not null).Cast<FieldSchema>().ToArray(),
            JsonValueKind.Object => fields.EnumerateObject().Select(ReadField).Where(field => field is not null).Cast<FieldSchema>().ToArray(),
            _ => []
        };
    }

    private static FieldSchema? ReadField(JsonElement field)
    {
        string? name = ReadName(field);
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        string? kind = field.TryGetProperty("kind", out JsonElement kindElement) ? JsonValueToType(kindElement) : null;
        return new FieldSchema(name, kind);
    }

    private static FieldSchema? ReadField(JsonProperty property)
    {
        string? kind = property.Value.ValueKind == JsonValueKind.String 
            ? ParseTypeFromDefineField(property.Value.GetString()) 
            : null;
        return new FieldSchema(property.Name, kind);
    }

    private static string? ReadName(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
        }

        return element.TryGetProperty("name", out JsonElement name) 
            ? name.GetString() 
            : null;
    }

    private static string? JsonValueToType(JsonElement element)
    {
        return element.ValueKind == JsonValueKind.String 
            ? element.GetString() 
            : element.ToString();
    }

    private static string? ParseTypeFromDefineField(string? defineField)
    {
        if (string.IsNullOrWhiteSpace(defineField))
        {
            return null;
        }

        Match match = DefineFieldTypeRegex().Match(defineField);
        return match.Success 
            ? match.Groups["type"].Value 
            : null;
    }

    private static string SurrealIdentifier(string value)
    {
        if (!SurrealIdentifierRegex().IsMatch(value))
        {
            throw new CliException($"Table '{value}' cannot be scaffolded yet because it is not a simple SurrealQL identifier.");
        }

        return value;
    }

    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*$")]
    private static partial Regex SurrealIdentifierRegex();

    [GeneratedRegex(@"\bTYPE\s+(?<type>[^\s]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DefineFieldTypeRegex();
}
