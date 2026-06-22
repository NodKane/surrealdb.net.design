using System.Text;
using SurrealDb.Net.Design.Cli;
using SurrealDb.Net.Design.Generation;

namespace SurrealDb.Net.Design.Scaffolding;

internal static class GeneratedFileWriter
{
    public static async Task WriteAsync(IReadOnlyList<GeneratedFile> files, ScaffoldOptions options)
    {
        Directory.CreateDirectory(options.OutputDirectory);

        foreach (var file in files)
        {
            if (File.Exists(file.Path) && !options.Overwrite)
            {
                Console.WriteLine($"Skipped {file.Path} because it already exists. Pass --overwrite to replace it.");
                continue;
            }

            await File.WriteAllTextAsync(file.Path, file.Content, Encoding.UTF8, options.CancellationToken);
            Console.WriteLine($"Generated {file.Path}");
        }
    }
}
