using FluentAssertions;
using SurrealDb.Net.Design.Cli;
using SurrealDb.Net.Design.Scaffolding;

namespace SurrealDb.Net.Design.Tests.Scaffolding;

public sealed class ScaffoldDbCommandTests
{
    [Fact]
    public async Task ExecuteAsync_generates_context_for_selected_tables()
    {
        var outputDirectory = Path.Combine(Path.GetTempPath(), $"surrealdb-design-{Guid.NewGuid():N}");
        var schemaFile = Path.Combine(outputDirectory, "schema.json");
        try
        {
            Directory.CreateDirectory(outputDirectory);
            await File.WriteAllTextAsync(schemaFile, SchemaJson);

            var options = ScaffoldOptions.Parse([
                "--schema-file", schemaFile,
                "--database", "app",
                "--output", outputDirectory,
                "--model-namespace", "MyApp.Models",
                "--table", "orders",
                "--overwrite"
            ]);

            var result = await ScaffoldDbCommand.ExecuteAsync(options);

            result.Should().Be(0);
            var context = await File.ReadAllTextAsync(Path.Combine(outputDirectory, "AppDbContext.cs"));
            context.Should().Contain("public IQueryable<Order> Orders => _surrealDbClient.Select<Order>();");
            context.Should().NotContain("Users =>");
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_skips_context_when_disabled()
    {
        var outputDirectory = Path.Combine(Path.GetTempPath(), $"surrealdb-design-{Guid.NewGuid():N}");
        var schemaFile = Path.Combine(outputDirectory, "schema.json");
        try
        {
            Directory.CreateDirectory(outputDirectory);
            await File.WriteAllTextAsync(schemaFile, SchemaJson);

            var options = ScaffoldOptions.Parse([
                "--schema-file", schemaFile,
                "--database", "app",
                "--output", outputDirectory,
                "--model-namespace", "MyApp.Models",
                "--no-context",
                "--overwrite"
            ]);

            var result = await ScaffoldDbCommand.ExecuteAsync(options);

            result.Should().Be(0);
            File.Exists(Path.Combine(outputDirectory, "AppDbContext.cs")).Should().BeFalse();
        }
        finally
        {
            if (Directory.Exists(outputDirectory))
            {
                Directory.Delete(outputDirectory, recursive: true);
            }
        }
    }

    private const string SchemaJson = """
        {
          "tables": [
            {
              "name": "orders",
              "fields": [
                { "name": "total", "kind": "number" }
              ]
            },
            {
              "name": "user",
              "fields": [
                { "name": "name", "kind": "string" }
              ]
            }
          ]
        }
        """;
}
