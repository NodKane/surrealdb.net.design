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
    }

    [Fact]
    public void Generate_adds_record_id_mapping_to_records()
    {
        var schema = new DatabaseSchema([
            new RecordSchema("user", [
                new FieldSchema("name", "string")
            ])
        ]);
        var files = CSharpModelGenerator.Generate(schema, CreateOptions());
        var userFile = files.Should().Contain(file => file.Path.EndsWith("User.cs")).Subject;

        userFile.Content.Should().Contain("using Dahomey.Cbor.Attributes;");
        userFile.Content.Should().Contain("using SurrealDb.Net.Json;");
        userFile.Content.Should().Contain("using SurrealDb.Net.Models;");
        userFile.Content.Should().Contain("using System.Text.Json.Serialization;");
        userFile.Content.Should().Contain("[JsonConverter(typeof(ReadOnlyRecordIdJsonConverter))]");
        userFile.Content.Should().Contain("[CborProperty(\"id\")]");
        userFile.Content.Should().Contain("[CborIgnoreIfDefault]");
        userFile.Content.Should().Contain("public RecordId? Id { get; set; }");
    }

    [Fact]
    public void Generate_treats_schema_id_field_as_record_id()
    {
        var schema = new DatabaseSchema([
            new RecordSchema("user", [
                new FieldSchema("id", "string"),
                new FieldSchema("name", "string")
            ])
        ]);
        var files = CSharpModelGenerator.Generate(schema, CreateOptions());
        var userFile = files.Should().Contain(file => file.Path.EndsWith("User.cs")).Subject;

        userFile.Content.Split("public RecordId? Id { get; set; }").Length.Should().Be(2);
        userFile.Content.Should().NotContain("[Column(\"id\")]");
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

    [Fact]
    public void Generate_maps_datetime_to_datetime()
    {
        var schema = new DatabaseSchema([
            new RecordSchema("orders", [
                new FieldSchema("created_at", "datetime")
            ])
        ]);
        var files = CSharpModelGenerator.Generate(schema, CreateOptions());
        var orderFile = files.Should().Contain(file => file.Path.EndsWith("Order.cs")).Subject;

        orderFile.Content.Should().Contain("public DateTime CreatedAt { get; set; }");
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
            RecordBaseType: "IRecord",
            RecordNamespace: "SurrealDb.Net.Models",
            Tables: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            SchemaFile: null,
            Overwrite: true,
            CancellationToken.None);
    }
}
