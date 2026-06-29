# SurrealDb.Net.Design.Tool

SurrealDb.Net.Design.Tool is the .NET command-line tool for scaffolding C# record models from a SurrealDB database schema.

Use this package when you want the `surrealdb-design` command in a terminal, CI job, or local tool manifest. For Visual Studio Package Manager Console commands such as `Scaffold-SurrealDbDatabase`, install the `SurrealDb.Net.Design` package instead.

## Usage

```bash
surrealdb-design scaffold db \
  --connection "Server=ws://localhost:8000/rpc;Namespace=app;Database=app;Username=root;Password=secret" \
  --output Models \
  --model-namespace MyApp.Models \
  --overwrite
```

With a local tool manifest, run the command through `dotnet`:

```bash
dotnet surrealdb-design scaffold db --connection "Server=ws://localhost:8000/rpc;Namespace=app;Database=app;Username=root;Password=secret"
```

The command connects to SurrealDB, inspects table and field metadata, and generates editable `partial` C# model classes plus an optional query context.

## Command Options

```text
surrealdb-design scaffold db [options]

Options:
  --connection <value>         SurrealDB connection string. Defaults to $SURREALDB_CONNECTION_STRING.
  --endpoint <url>             Override connection string server. Defaults to http://localhost:8000.
  --namespace <name>           Override connection string namespace. Defaults to $SURREALDB_NS or main.
  --database <name>            Override connection string database. Defaults to $SURREALDB_DB or main.
  --user <name>                Override connection string username. Defaults to $SURREALDB_USER.
  --password <value>           Override connection string password. Defaults to $SURREALDB_PASS.
  --token <value>              Override connection string bearer token. Defaults to $SURREALDB_TOKEN.
  --output <path>              Output directory. Defaults to Generated.
  --model-namespace <name>     Namespace for generated classes. Defaults to SurrealDb.Generated.
  --context <name>             Name for the generated query context. Defaults to <Database>DbContext.
  --context-namespace <name>   Namespace for the generated query context. Defaults to --model-namespace.
  --no-context                 Do not generate a query context.
  --record-base-type <type>    Base type for generated records. Defaults to IRecord.
  --record-namespace <name>    Namespace imported for the record base type. Defaults to SurrealDb.Net.Models.
  --table <name>               Restrict generation to one table. Can be repeated or comma-separated.
  --schema-file <path>         Read schema from a local JSON file instead of SurrealDB.
  --overwrite                  Replace existing generated files.
```
