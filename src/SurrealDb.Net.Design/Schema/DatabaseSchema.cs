namespace SurrealDb.Net.Design.Schema;

internal sealed record DatabaseSchema(IReadOnlyList<RecordSchema> Records);
