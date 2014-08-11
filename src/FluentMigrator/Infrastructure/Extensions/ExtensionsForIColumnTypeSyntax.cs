using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders
{
    /* TODO: possibly full namespace: FluentMigrator.Infrastructure.Extensions
     * placed in the namespace that it serves: FluentMigrator.Builders */
    public static class ExtensionsForIColumnTypeSyntax
    {
        public static TNext AsVarcharString<TNext>(this IColumnTypeSyntax<TNext> columnTypeSyntax, int size)
            where TNext : IFluentSyntax
        {
            //TODO: there's probably a better way and/or place to put this for it to appear transparent
            var customType = string.Format(@"VARCHAR({0})", size);
            return columnTypeSyntax.AsCustom(customType);
        }
    }
}
