# SurrealDb.Net.Design Demo

This Minimal API uses the official `SurrealDb.Net` NuGet package and scaffolded `User` and `Order` models.
It intentionally uses scalar fields only in the generated demo schema so it works with the current stable `SurrealDb.Net` package.

## Run SurrealDB

```bash
surreal start --user root --pass root memory
```

The demo connection string uses `ws://127.0.0.1:8000/rpc` and `root/root`. The WebSocket endpoint avoids the HTTP CBOR response parsing issue that `SurrealDb.Net 0.10.2` can hit with newer SurrealDB 3.1.x servers.

## Scaffold the demo models

The current official `SurrealDb.Net` package exposes `Select<T>("table", cancellationToken)`. The upcoming `IQueryable` API is expected to add `Select<T>()`, so this demo disables context generation while using the stable package:

```bash
dotnet run --project ../../src/SurrealDb.Net.Design -- scaffold db \
  --schema-file schema.json \
  --database app \
  --output Generated \
  --model-namespace SurrealDb.Net.Design.Demo.Generated \
  --no-context \
  --overwrite
```

After the official package includes `IQueryable Select<T>()`, remove `--no-context` to generate `AppDbContext`.

## Run the API

```bash
dotnet run
```

Then open:

- `GET /users`
- `GET /orders`
