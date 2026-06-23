using SurrealDb.Net.Design.Cli;

namespace SurrealDb.Net.Design.Schema;

internal static class SchemaReaderFactory
{
    public static ISchemaReader Create(ScaffoldOptions options)
    {
        return string.IsNullOrWhiteSpace(options.SchemaFile)
            ? new SurrealDbHttpSchemaReader(options)
            : new JsonSchemaFileReader(options);
    }
}
