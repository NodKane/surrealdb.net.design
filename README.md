# SurrealDb.Net.Design

SurrealDb.Net.Design is an early design-time CLI for generating C# record models from a SurrealDB database schema.

Project: https://github.com/NodKane/surrealdb.net.design

The goal is intentionally similar to the design-time tooling people know from Entity Framework Core: connect to a database, inspect the schema, and scaffold source files that can be edited safely through `partial` classes.

## Status

This repository currently contains the first minimal CLI shape:

- `scaffold db` connects to SurrealDB over the HTTP SQL endpoint.
- The command reads table metadata with `INFO FOR DB`.
- For every table, it reads field metadata with `INFO FOR TABLE <table> STRUCTURE`.
- It generates one C# `partial` class per table.
- It generates one C# `partial` query context with `IQueryable<T>` table entry points.
- Generated classes implement `IRecord` by default.
- Generated classes map SurrealDB's intrinsic `id` value to `RecordId? Id`.
- Generated classes use `[Table("...")]` and `[Column("...")]` annotations from `System.ComponentModel.DataAnnotations.Schema`.

## Usage

From the repository:

```bash
dotnet run --project src/SurrealDb.Net.Design -- scaffold db \
  --connection "Server=ws://localhost:8000/rpc;Namespace=app;Database=app;Username=root;Password=secret" \
  --output Models \
  --model-namespace MyApp.Models \
  --overwrite
```

For command prompt usage, install the companion .NET tool package. The command name is:

```bash
dotnet tool install --global SurrealDb.Net.Design.Tool
surrealdb-design scaffold db --connection "Server=ws://localhost:8000/rpc;Namespace=app;Database=app;Username=root;Password=secret"
```

From Visual Studio's Package Manager Console, install the NuGet package and use the package command:

```powershell
Install-Package SurrealDb.Net.Design

Scaffold-SurrealDbDatabase `
  -Connection "Server=ws://localhost:8000/rpc;Namespace=app;Database=app;Username=root;Password=secret" `
  -Output Models `
  -ModelNamespace MyApp.Models `
  -Overwrite
```

`Scaffold-SurrealDbContext` is available as an alias for the same Package Manager Console command. The optional `-Project` parameter runs the command from a specific project's directory so relative output paths land in that project.

The Package Manager Console command is provided by the `SurrealDb.Net.Design` package. The command prompt command is provided by the `SurrealDb.Net.Design.Tool` dotnet tool package.

For local command prompt usage, you can install the tool through a tool manifest:

```bash
dotnet new tool-manifest
dotnet tool install SurrealDb.Net.Design.Tool
dotnet surrealdb-design scaffold db --connection "Server=ws://localhost:8000/rpc;Namespace=app;Database=app;Username=root;Password=secret"
```

## Generated Code

For a SurrealDB table named `orders` with `user` defined as `record<user>` and `total` defined as `option<number>`, the generated class looks like this:

```csharp
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Dahomey.Cbor.Attributes;
using SurrealDb.Net.Json;
using SurrealDb.Net.Models;

namespace MyApp.Models;

[Table("orders")]
public partial class Order : IRecord
{
    [JsonConverter(typeof(ReadOnlyRecordIdJsonConverter))]
    [CborProperty("id")]
    [CborIgnoreIfDefault]
    public RecordId? Id { get; set; }

    [Column("user")]
    public User User { get; set; }

    [Column("total")]
    [CborIgnoreIfDefault]
    public double? Total { get; set; }
}
```

Because the classes are generated as `partial`, custom behavior can live in separate partial files next to the generated model files.

For a SurrealDB database named `app`, the command also generates an `AppDbContext` by default:

```csharp
using System.Linq;
using SurrealDb.Net;

namespace MyApp.Models;

public partial class AppDbContext
{
    private readonly ISurrealDbClient _surrealDbClient;

    public AppDbContext(ISurrealDbClient surrealDbClient)
    {
        _surrealDbClient = surrealDbClient;
    }

    public IQueryable<Order> Orders => _surrealDbClient.Select<Order>();
}
```

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

## Local Schema File

For quick testing without a running SurrealDB instance, `--schema-file` accepts a small JSON schema:

```json
{
  "tables": [
    {
      "name": "orders",
      "fields": [
        { "name": "user", "kind": "record<user>" },
        { "name": "total", "kind": "option<number>" }
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
```
