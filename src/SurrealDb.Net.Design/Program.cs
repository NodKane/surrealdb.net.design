using SurrealDb.Net.Design.Cli;
using SurrealDb.Net.Design.Scaffolding;
using System.Text.Json;

if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase) || args.Contains("-h", StringComparer.OrdinalIgnoreCase))
{
    HelpText.Print();
    return args.Length == 0 ? 1 : 0;
}

if (!IsScaffoldDbCommand(args))
{
    Console.Error.WriteLine("Unknown command. Expected: scaffold db");
    HelpText.Print();
    return 1;
}

try
{
    var options = ScaffoldOptions.Parse(args.Skip(2).ToArray());
    return await ScaffoldDbCommand.ExecuteAsync(options);
}
catch (CliException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"SurrealDB request failed: {ex.Message}");
    return 1;
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"Could not parse SurrealDB response: {ex.Message}");
    return 1;
}

static bool IsScaffoldDbCommand(string[] args)
{
    return string.Equals(args.ElementAtOrDefault(0), "scaffold", StringComparison.OrdinalIgnoreCase) &&
        string.Equals(args.ElementAtOrDefault(1), "db", StringComparison.OrdinalIgnoreCase);
}
