namespace SurrealDb.Net.Design.Schema;

internal sealed record RecordSchema(string TableName, IReadOnlyList<FieldSchema> Fields);
