using SurrealDb.Net.Design.Cli;
using SurrealDb.Net.Design.Generation;
using SurrealDb.Net.Design.Schema;

namespace SurrealDb.Net.Design.Scaffolding;

internal static class ScaffoldDbCommand
{
    public static async Task<int> ExecuteAsync(ScaffoldOptions options)
    {
        var reader = SchemaReaderFactory.Create(options);
        var schema = await reader.ReadAsync(options.CancellationToken);
        var files = CSharpModelGenerator.Generate(schema, options);

        if (files.Count == 0)
        {
            Console.WriteLine("No tables were found.");
            return 0;
        }

        await GeneratedFileWriter.WriteAsync(files, options);
        return 0;
    }
}
