using FluentAssertions;
using SurrealDb.Net.Design.Cli;
using SurrealDb.Net.Design.Generation;
using SurrealDb.Net.Design.Schema;

namespace SurrealDb.Net.Design.Tests.Generation;

public sealed class CSharpContextGeneratorTests
{
    [Fact]
    public void Generate_uses_database_name_for_default_context_name()
    {
        var options = ScaffoldOptions.Parse([
            "--database", "app",
            "--output", "Generated",
            "--model-namespace", "MyApp.Models"
        ]);
        var schema = new DatabaseSchema([
            new RecordSchema("orders", []),
            new RecordSchema("user", [])
        ]);

        var file = CSharpContextGenerator.Generate(schema, options);

        file.Path.Should().EndWith("AppDbContext.cs");
        file.Content.Should().Contain("public partial class AppDbContext");
        file.Content.Should().Contain("public AppDbContext(ISurrealDbClient surrealDbClient)");
        file.Content.Should().Contain("private readonly ISurrealDbClient _surrealDbClient;");
        file.Content.Should().Contain("public IQueryable<Order> Orders => _surrealDbClient.Select<Order>();");
        file.Content.Should().Contain("public IQueryable<User> Users => _surrealDbClient.Select<User>();");
    }

    [Fact]
    public void Generate_does_not_add_duplicate_db_suffix()
    {
        var options = ScaffoldOptions.Parse(["--database", "shop_db"]);

        options.ContextName.Should().Be("ShopDbContext");
    }

    [Fact]
    public void Generate_does_not_singularize_database_name_for_default_context_name()
    {
        var options = ScaffoldOptions.Parse(["--database", "todos"]);

        options.ContextName.Should().Be("TodosDbContext");
    }

    [Fact]
    public void Generate_uses_explicit_context_name()
    {
        var options = ScaffoldOptions.Parse(["--database", "app", "--context", "MyContext"]);
        var schema = new DatabaseSchema([
            new RecordSchema("orders", [])
        ]);

        var file = CSharpContextGenerator.Generate(schema, options);

        file.Path.Should().EndWith("MyContext.cs");
        file.Content.Should().Contain("public partial class MyContext");
        file.Content.Should().Contain("public MyContext(ISurrealDbClient surrealDbClient)");
    }

    [Fact]
    public void Generate_imports_model_namespace_when_context_namespace_differs()
    {
        var options = ScaffoldOptions.Parse([
            "--database", "app",
            "--model-namespace", "MyApp.Models",
            "--context-namespace", "MyApp.Data"
        ]);
        var schema = new DatabaseSchema([
            new RecordSchema("orders", [])
        ]);

        var file = CSharpContextGenerator.Generate(schema, options);

        file.Content.Should().Contain("using MyApp.Models;");
        file.Content.Should().Contain("namespace MyApp.Data;");
    }

    [Fact]
    public void Parse_disables_context_generation()
    {
        var options = ScaffoldOptions.Parse(["--no-context"]);

        options.GenerateContext.Should().BeFalse();
    }
}
