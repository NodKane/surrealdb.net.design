using FluentAssertions;
using SurrealDb.Net.Design.Cli;

namespace SurrealDb.Net.Design.Tests.Cli;

public sealed class ScaffoldOptionsTests
{
    [Fact]
    public void Parse_uses_connection_string_for_database_connection_options()
    {
        var options = ScaffoldOptions.Parse([
            "--conection",
            "Server=ws://127.0.0.1:8000/rpc;Namespace=app;Database=shop;Username=root;Password=secret"
        ]);

        options.Endpoint.Should().Be(new Uri("http://127.0.0.1:8000"));
        options.Namespace.Should().Be("app");
        options.Database.Should().Be("shop");
        options.User.Should().Be("root");
        options.Password.Should().Be("secret");
    }

    [Fact]
    public void Parse_lets_explicit_options_override_connection_string_values()
    {
        var options = ScaffoldOptions.Parse([
            "--conection",
            "Server=ws://127.0.0.1:8000/rpc;Namespace=app;Database=shop;Username=root;Password=secret",
            "--endpoint",
            "https://surreal.example.test",
            "--namespace",
            "custom_ns",
            "--database",
            "custom_db",
            "--user",
            "admin",
            "--password",
            "override"
        ]);

        options.Endpoint.Should().Be(new Uri("https://surreal.example.test"));
        options.Namespace.Should().Be("custom_ns");
        options.Database.Should().Be("custom_db");
        options.User.Should().Be("admin");
        options.Password.Should().Be("override");
    }

    [Fact]
    public void Parse_accepts_connection_string_aliases()
    {
        var options = ScaffoldOptions.Parse([
            "--conection",
            "Endpoint=wss://surreal.example.test/rpc;NS=app;DB=shop;UserName=root;Pass=secret;AuthToken=token"
        ]);

        options.Endpoint.Should().Be(new Uri("https://surreal.example.test"));
        options.Namespace.Should().Be("app");
        options.Database.Should().Be("shop");
        options.User.Should().Be("root");
        options.Password.Should().Be("secret");
        options.Token.Should().Be("token");
    }
}
