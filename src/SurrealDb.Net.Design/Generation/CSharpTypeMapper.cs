namespace SurrealDb.Net.Design.Generation;

internal sealed class CSharpTypeMapper(IReadOnlyDictionary<string, string> recordTypeNames)
{
    public CSharpPropertyType Map(string? surrealType)
    {
        if (string.IsNullOrWhiteSpace(surrealType))
        {
            return new CSharpPropertyType("object?", IsNullable: true, RequiresDefaultInitializer: false);
        }

        string normalized = surrealType.Trim();
        string lower = normalized.ToLowerInvariant();

        if (TryReadGeneric(normalized, "option", out string optionInner))
        {
            return MapNullable(optionInner);
        }

        if (TryReadGeneric(normalized, "array", out string arrayInner) || TryReadGeneric(normalized, "set", out arrayInner))
        {
            var innerType = Map(arrayInner);
            return new CSharpPropertyType($"List<{innerType.Name}>", IsNullable: false, RequiresDefaultInitializer: true);
        }

        if (TryReadGeneric(normalized, "record", out string recordTarget))
        {
            return new CSharpPropertyType(ResolveRecordTypeName(recordTarget), IsNullable: false, RequiresDefaultInitializer: true);
        }

        if (lower == "record")
        {
            return new CSharpPropertyType("string", IsNullable: false, RequiresDefaultInitializer: true);
        }

        return lower switch
        {
            "any" => ReferenceType("object"),
            "object" => ReferenceType("object"),
            "string" => ReferenceType("string"),
            "uuid" => ValueType("Guid"),
            "bool" or "boolean" => ValueType("bool"),
            "int" or "integer" => ValueType("long"),
            "float" or "number" => ValueType("double"),
            "decimal" => ValueType("decimal"),
            "datetime" => ValueType("DateTime"),
            "duration" => ValueType("TimeSpan"),
            "bytes" => ReferenceType("byte[]"),
            _ => new CSharpPropertyType("object?", IsNullable: true, RequiresDefaultInitializer: false)
        };
    }

    private CSharpPropertyType MapNullable(string surrealType)
    {
        var innerType = Map(surrealType);
        return new CSharpPropertyType(EnsureNullable(innerType.Name), IsNullable: true, RequiresDefaultInitializer: false);
    }

    private static CSharpPropertyType ReferenceType(string typeName)
    {
        return new CSharpPropertyType(typeName, IsNullable: false, RequiresDefaultInitializer: true);
    }

    private static CSharpPropertyType ValueType(string typeName)
    {
        return new CSharpPropertyType(typeName, IsNullable: false, RequiresDefaultInitializer: false);
    }

    private static bool TryReadGeneric(string value, string name, out string inner)
    {
        string prefix = $"{name}<";
        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && value.EndsWith('>'))
        {
            inner = value[prefix.Length..^1];
            return true;
        }

        inner = string.Empty;
        return false;
    }

    private string ResolveRecordTypeName(string recordTarget)
    {
        string tableName = recordTarget.Trim();
        if (string.IsNullOrWhiteSpace(tableName) || tableName.Contains('|', StringComparison.Ordinal))
        {
            return "object";
        }

        return recordTypeNames.TryGetValue(tableName, out string? typeName)
            ? typeName
            : CSharpIdentifier.ForClassName(tableName);
    }

    private static string EnsureNullable(string typeName)
    {
        return typeName.EndsWith('?') ? typeName : $"{typeName}?";
    }
}
