using System.Text;

namespace SurrealDb.Net.Design.Generation;

internal static partial class CSharpIdentifier
{
    private static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override",
        "params", "private", "protected", "public", "readonly", "record", "ref", "return", "sbyte", "sealed", "short",
        "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof",
        "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

    public static string ForClassName(string tableName)
    {
        return ToPascalCase(Singularize(tableName));
    }

    public static string ForPropertyName(string fieldName)
    {
        return ToPascalCase(fieldName);
    }

    public static string Escape(string identifier)
    {
        return Keywords.Contains(identifier) ? $"@{identifier}" : identifier;
    }

    public static string EscapeStringLiteral(string value)
    {
        return value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private static string Singularize(string value)
    {
        if (value.EndsWith("ies", StringComparison.OrdinalIgnoreCase) && value.Length > 3)
        {
            return $"{value[..^3]}y";
        }

        if (value.EndsWith('s') && !value.EndsWith("ss", StringComparison.OrdinalIgnoreCase) && value.Length > 1)
        {
            return value[..^1];
        }

        return value;
    }

    private static string ToPascalCase(string value)
    {
        var span = value.AsSpan();
        var builder = new StringBuilder(value.Length);
        var capitalizeNext = true;

        foreach (char current in span)
        {
            if (!char.IsAsciiLetterOrDigit(current))
            {
                capitalizeNext = true;
                continue;
            }

            builder.Append(capitalizeNext
                ? char.ToUpperInvariant(current)
                : current);

            capitalizeNext = false;
        }

        if (builder.Length == 0)
        {
            return "SurrealRecord";
        }

        if (char.IsDigit(builder[0]))
        {
            builder.Insert(0, '_');
        }

        return builder.ToString();
    }
}
