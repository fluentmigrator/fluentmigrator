using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace FluentMigrator.Analyzers
{
    internal static class SymbolExtensions
    {
        internal static bool ContainsAttributeType(this ImmutableArray<AttributeData> attributes, INamedTypeSymbol attributeType, bool exactMatch = false)
            => attributes.Any(a => attributeType.IsAssignableFrom(a.AttributeClass, exactMatch));

        internal static bool IsAssignableFrom(this ITypeSymbol targetType, ITypeSymbol sourceType, bool exactMatch = false)
        {
            if (targetType != null)
            {
                while (sourceType != null)
                {
                    if (sourceType.Equals(targetType))
                    {
                        return true;
                    }

                    if (exactMatch)
                    {
                        return false;
                    }

                    if (targetType.TypeKind == TypeKind.Interface)
                    {
                        return sourceType.AllInterfaces.Any(i => i.Equals(targetType));
                    }

                    sourceType = sourceType.BaseType;
                }
            }

            return false;
        }
    }
}
