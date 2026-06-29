# SurrealDb.Net.Design

SurrealDb.Net.Design is the Visual Studio Package Manager Console package for scaffolding C# record models from a SurrealDB database schema.

Use this package when you want design-time commands inside Visual Studio's Package Manager Console. For command prompt, shell, CI, or `dotnet tool` usage, install the companion `SurrealDb.Net.Design.Tool` package instead.

## Usage

```powershell
Scaffold-SurrealDbDatabase `
  -Connection "Server=ws://localhost:8000/rpc;Namespace=app;Database=app;Username=root;Password=secret" `
  -Output Models `
  -ModelNamespace MyApp.Models `
  -Overwrite
```

`Scaffold-SurrealDbContext` is available as an alias for the same command. The optional `-Project` parameter runs the command from a specific project's directory so relative output paths land in that project.

The command connects to SurrealDB, inspects table and field metadata, and generates editable `partial` C# model classes plus an optional query context.

## Command Options

```text
-Connection <value>        SurrealDB connection string. Defaults to $SURREALDB_CONNECTION_STRING.
-Endpoint <url>            Override connection string server. Defaults to http://localhost:8000.
-Namespace <name>          Override connection string namespace. Defaults to $SURREALDB_NS or main.
-Database <name>           Override connection string database. Defaults to $SURREALDB_DB or main.
-User <name>               Override connection string username. Defaults to $SURREALDB_USER.
-Password <value>          Override connection string password. Defaults to $SURREALDB_PASS.
-Token <value>             Override connection string bearer token. Defaults to $SURREALDB_TOKEN.
-Output <path>             Output directory. Defaults to Generated.
-ModelNamespace <name>     Namespace for generated classes. Defaults to SurrealDb.Generated.
-Context <name>            Name for the generated query context. Defaults to <Database>DbContext.
-ContextNamespace <name>   Namespace for the generated query context. Defaults to -ModelNamespace.
-NoContext                 Do not generate a query context.
-RecordBaseType <type>     Base type for generated records. Defaults to IRecord.
-RecordNamespace <name>    Namespace imported for the record base type. Defaults to SurrealDb.Net.Models.
-Table <name>              Restrict generation to one table. Can be repeated or comma-separated.
-SchemaFile <path>         Read schema from a local JSON file instead of SurrealDB.
-Overwrite                 Replace existing generated files.
```
