namespace SurrealDb.Net.Design.Schema;

internal interface ISchemaReader
{
    Task<DatabaseSchema> ReadAsync(CancellationToken cancellationToken);
}
