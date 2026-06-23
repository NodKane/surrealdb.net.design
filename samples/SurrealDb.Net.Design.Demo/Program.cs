using Microsoft.AspNetCore.Mvc;
using SurrealDb.Net;
using SurrealDb.Net.Design.Demo.Generated;

var builder = WebApplication.CreateBuilder(args);

var surrealDbConnectionString = builder.Configuration.GetConnectionString("SurrealDB")
    ?? throw new InvalidOperationException("SurrealDB connection string not found");
var surrealDbOptions = SurrealDbOptions
    .Create()
    .FromConnectionString(surrealDbConnectionString)
    .Build();

builder.Services.AddSingleton(_ => new SurrealDbClient(surrealDbOptions));
builder.Services.AddSingleton<ISurrealDbClient>(services => services.GetRequiredService<SurrealDbClient>());

var app = builder.Build();

app.MapGet("/users", async ([FromServices] ISurrealDbClient surrealDbClient, CancellationToken cancellationToken) =>
{
    return await TrySurrealAsync(async () =>
    {
        var users = await surrealDbClient.Select<User>("user", cancellationToken);
        return Results.Ok(users);
    });
});

app.MapGet("/orders", async ([FromServices] ISurrealDbClient surrealDbClient, CancellationToken cancellationToken) =>
{
    return await TrySurrealAsync(async () =>
    {
        var orders = await surrealDbClient.Select<Order>("orders", cancellationToken);
        return Results.Ok(orders);
    });
});

await InitializeDbAsync();
await app.RunAsync();

static async Task<IResult> TrySurrealAsync(Func<Task<IResult>> action)
{
    try
    {
        return await action();
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "SurrealDB request failed",
            detail: ex.Message,
            statusCode: StatusCodes.Status502BadGateway);
    }
}

async Task<SeedResult> InitializeDbAsync()
{
    const string seedEmail = "ada@example.com";
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    var surrealDbClient = app.Services.GetRequiredService<ISurrealDbClient>();

    await EnsureSchemaAsync(surrealDbClient, app.Environment.ContentRootPath);

    var users = await surrealDbClient.Select<User>("user");
    var user = users.FirstOrDefault(candidate =>
        string.Equals(candidate.Email, seedEmail, StringComparison.OrdinalIgnoreCase));
    var userCreated = false;

    if (user is null)
    {
        user = await surrealDbClient.Create(
            "user",
            new User
            {
                Name = "Ada Lovelace",
                Email = seedEmail
            });

        userCreated = true;
    }

    var orders = await surrealDbClient.Select<Order>("orders");
    var order = orders.FirstOrDefault(candidate =>
        string.Equals(candidate.UserEmail, seedEmail, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(candidate.Status, "paid", StringComparison.OrdinalIgnoreCase) &&
        Math.Abs(candidate.Total - 42.5) < 0.0001);
    var orderCreated = false;

    if (order is null)
    {
        order = await surrealDbClient.Create(
            "orders",
            new Order
            {
                UserEmail = user.Email,
                Total = 42.5,
                Status = "paid",
                CreatedAt = DateTime.UtcNow
            });

        orderCreated = true;
    }

    logger?.LogDebug("Seeded demo user {Email} and order {OrderId}", user.Email, order.Id);
    return new SeedResult(user, order, userCreated, orderCreated);
}

static async Task EnsureSchemaAsync(ISurrealDbClient surrealDbClient, string contentRootPath)
{
    var schemaPath = Path.Combine(contentRootPath, "schema.surql");
    var schema = await File.ReadAllTextAsync(schemaPath);
    await surrealDbClient.RawQuery(schema);
}

public sealed record SeedResult(User User, Order Order, bool UserCreated, bool OrderCreated);
