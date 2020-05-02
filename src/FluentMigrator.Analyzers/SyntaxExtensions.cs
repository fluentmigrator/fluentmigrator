using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentMigrator.Analyzers
{
    internal static class SyntaxExtensions
    {
        //internal static bool ContainsAttributeType(this SyntaxList<AttributeListSyntax> attributeLists, SemanticModel semanticModel, INamedTypeSymbol attributeType, bool exactMatch = false)
        //{
        //    foreach (var attributeList in attributeLists)
        //    {
        //        foreach (var attribute in attributeList.Attributes)
        //        {
        //            var type = semanticModel.GetTypeInfo(attribute).Type;
        //            if (attributeType.IsAssignableFrom(type, exactMatch))
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}
    }
}
