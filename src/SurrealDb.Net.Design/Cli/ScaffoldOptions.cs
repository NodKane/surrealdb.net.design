namespace SurrealDb.Net.Design.Cli;

internal sealed record ScaffoldOptions(
    Uri Endpoint,
    string Namespace,
    string Database,
    string? User,
    string? Password,
    string? Token,
    string OutputDirectory,
    string ModelNamespace,
    string ContextName,
    string ContextNamespace,
    bool GenerateContext,
    string RecordBaseType,
    string? RecordNamespace,
    IReadOnlySet<string> Tables,
    string? SchemaFile,
    bool Overwrite,
    CancellationToken CancellationToken)
{
    public static ScaffoldOptions Parse(string[] args)
    {
        var values = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var flags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                throw new CliException($"Unexpected argument '{arg}'. Options must start with --.");
            }

            var name = arg[2..];
            if (string.Equals(name, "overwrite", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(name, "no-context", StringComparison.OrdinalIgnoreCase))
            {
                flags.Add(name);
                continue;
            }

            if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                throw new CliException($"Option '{arg}' requires a value.");
            }

            if (!values.TryGetValue(name, out List<string>? optionValues))
            {
                optionValues = [];
                values.Add(name, optionValues);
            }

            optionValues.Add(args[++i]);
        }

        var endpoint = Uri.TryCreate(Option(values, "endpoint") ?? "http://localhost:8000", UriKind.Absolute, out Uri? parsedEndpoint)
            ? parsedEndpoint
            : throw new CliException("--endpoint must be an absolute URI.");

        var outputDirectory = Path.GetFullPath(Option(values, "output") ?? "Generated");
        var schemaFile = Option(values, "schema-file");
        if (!string.IsNullOrWhiteSpace(schemaFile))
        {
            schemaFile = Path.GetFullPath(schemaFile);
        }

        var database = Option(values, "database") ?? Environment.GetEnvironmentVariable("SURREALDB_DB") ?? "main";
        var modelNamespace = Option(values, "model-namespace") ?? "SurrealDb.Generated";

        return new ScaffoldOptions(
            endpoint,
            Option(values, "namespace") ?? Environment.GetEnvironmentVariable("SURREALDB_NS") ?? "main",
            database,
            Option(values, "user") ?? Environment.GetEnvironmentVariable("SURREALDB_USER"),
            Option(values, "password") ?? Environment.GetEnvironmentVariable("SURREALDB_PASS"),
            Option(values, "token") ?? Environment.GetEnvironmentVariable("SURREALDB_TOKEN"),
            outputDirectory,
            modelNamespace,
            Option(values, "context") ?? Generation.CSharpIdentifier.ForContextName(database),
            Option(values, "context-namespace") ?? modelNamespace,
            !flags.Contains("no-context"),
            Option(values, "record-base-type") ?? "SurrealDbRecord",
            Option(values, "record-namespace") ?? "SurrealDb.Net.Models",
            ParseTables(values),
            schemaFile,
            flags.Contains("overwrite"),
            CancellationToken.None);
    }

    private static string? Option(IReadOnlyDictionary<string, List<string>> values, string name)
    {
        return values.TryGetValue(name, out List<string>? optionValues) ? optionValues.LastOrDefault() : null;
    }

    private static HashSet<string> ParseTables(IReadOnlyDictionary<string, List<string>> values)
    {
        var tables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!values.TryGetValue("table", out List<string>? tableValues))
        {
            return tables;
        }

        foreach (string value in tableValues)
        {
            foreach (string table in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                tables.Add(table);
            }
        }

        return tables;
    }
}
