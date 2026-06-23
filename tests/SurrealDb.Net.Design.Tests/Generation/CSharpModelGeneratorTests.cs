using FluentAssertions;
using SurrealDb.Net.Design.Cli;
using SurrealDb.Net.Design.Generation;
using SurrealDb.Net.Design.Schema;

namespace SurrealDb.Net.Design.Tests.Generation;

public sealed class CSharpModelGeneratorTests
{
    [Fact]
    public void Generate_uses_plain_cs_file_names()
    {
        var schema = new DatabaseSchema([
            new RecordSchema("orders", [])
        ]);
        var files = CSharpModelGenerator.Generate(schema, CreateOptions());

        files.Should().ContainSingle();
        files[0].Path.Should().EndWith("Order.cs");
        files[0].Path.Should().NotEndWith(".g.cs");
    }

    [Fact]
    public void Generate_maps_record_links_to_non_nullable_generated_record_types()
    {
        var schema = new DatabaseSchema([
            new RecordSchema("orders", [
                new FieldSchema("user", "record<user>")
            ]),
            new RecordSchema("user", [
                new FieldSchema("name", "string")
            ])
        ]);
        var files = CSharpModelGenerator.Generate(schema, CreateOptions());
        var orderFile = files.Should().Contain(file => file.Path.EndsWith("Order.cs")).Subject;

        orderFile.Content.Should().Contain("public User User { get; set; } = default!;");
        orderFile.Content.Should().NotContain("[CborIgnoreIfDefault]");
        orderFile.Content.Should().NotContain("using Dahomey.Cbor.Attributes;");
    }

    [Fact]
    public void Generate_maps_record_link_arrays_to_non_nullable_generated_record_type_lists()
    {
        var schema = new DatabaseSchema([
            new RecordSchema("orders", [
                new FieldSchema("users", "array<record<user>>")
            ]),
            new RecordSchema("user", [
                new FieldSchema("name", "string")
            ])
        ]);
        var files = CSharpModelGenerator.Generate(schema, CreateOptions());
        var orderFile = files.Should().Contain(file => file.Path.EndsWith("Order.cs")).Subject;

        orderFile.Content.Should().Contain("public List<User> Users { get; set; } = default!;");
        orderFile.Content.Should().NotContain("[CborIgnoreIfDefault]");
        orderFile.Content.Should().NotContain("using Dahomey.Cbor.Attributes;");
    }

    [Fact]
    public void Generate_marks_option_record_links_as_nullable_and_ignores_default_cbor_values()
    {
        var schema = new DatabaseSchema([
            new RecordSchema("orders", [
                new FieldSchema("user", "option<record<user>>")
            ]),
            new RecordSchema("user", [
                new FieldSchema("name", "string")
            ])
        ]);
        var files = CSharpModelGenerator.Generate(schema, CreateOptions());
        var orderFile = files.Should().Contain(file => file.Path.EndsWith("Order.cs")).Subject;

        orderFile.Content.Should().Contain("[CborIgnoreIfDefault]");
        orderFile.Content.Should().Contain("using Dahomey.Cbor.Attributes;");
        orderFile.Content.Should().Contain("public User? User { get; set; }");
    }

    [Fact]
    public void Generate_marks_option_scalars_as_nullable_and_ignores_default_cbor_values()
    {
        var schema = new DatabaseSchema([
            new RecordSchema("orders", [
                new FieldSchema("total", "option<number>")
            ])
        ]);
        var files = CSharpModelGenerator.Generate(schema, CreateOptions());
        var orderFile = files.Should().Contain(file => file.Path.EndsWith("Order.cs")).Subject;

        orderFile.Content.Should().Contain("[CborIgnoreIfDefault]");
        orderFile.Content.Should().Contain("using Dahomey.Cbor.Attributes;");
        orderFile.Content.Should().Contain("public double? Total { get; set; }");
    }

    private static ScaffoldOptions CreateOptions()
    {
        return new ScaffoldOptions(
            new Uri("http://localhost:8000"),
            "main",
            "main",
            User: null,
            Password: null,
            Token: null,
            OutputDirectory: "Generated",
            ModelNamespace: "MyApp.Models",
            ContextName: "MainDbContext",
            ContextNamespace: "MyApp.Models",
            GenerateContext: true,
            RecordBaseType: "SurrealDbRecord",
            RecordNamespace: "SurrealDb.Net.Models",
            Tables: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            SchemaFile: null,
            Overwrite: true,
            CancellationToken.None);
    }
}
