namespace SurrealDb.Net.Design.Cli;

internal static class HelpText
{
    public static void Print()
    {
        Console.WriteLine(
            """
            SurrealDb.Net.Design

            Usage:
              surrealdb-design scaffold db [options]

            Options:
              --conection <value>          SurrealDB connection string. Defaults to $SURREALDB_CONNECTION_STRING.
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
              -h, --help                   Show help.

            Example:
              surrealdb-design scaffold db --conection "Server=ws://localhost:8000/rpc;Namespace=app;Database=app;Username=root;Password=secret" --output Models --model-namespace MyApp.Models --overwrite
            """);
    }
}
