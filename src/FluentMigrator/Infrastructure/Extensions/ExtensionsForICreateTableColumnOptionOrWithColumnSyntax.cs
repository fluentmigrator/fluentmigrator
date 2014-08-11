using System;
using System.Linq;
using FluentMigrator.Builders.Create.Table;

namespace FluentMigrator
{
    public static class ExtensionsForICreateTableColumnOptionOrWithColumnSyntax
    {
        internal static ICreateTableColumnOptionOrWithColumnSyntax AsVarcharString(
            this ICreateTableColumnAsTypeSyntax columnTypeSyntax,
            int size,
            bool nullable = false)
        {
            var customType = string.Format(@"VARCHAR({0})", size);
            var intermediate = columnTypeSyntax.AsCustom(customType);
            return nullable ? intermediate.Nullable() : intermediate.NotNullable();
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax WithEnumColumn<TEnum>(
            this ICreateTableColumnOptionOrWithColumnSyntax tableColumnOptionOrWithColumnSyntax,
            string name, bool nullable = false)
        {
            var type = typeof(TEnum);

            if (!type.IsEnum)
                throw new InvalidOperationException(
                    string.Format(@"{0} is not an enum.", type));

            var values = Enum.GetValues(type).Cast<TEnum>().ToList();
            var valueSize = values.Select(x => x.ToString().Length).Max();

            //TODO: ditto other AsString comments: will use AsVarcharString instead for now
            var intermediate = tableColumnOptionOrWithColumnSyntax
                .WithColumn(name).AsVarcharString(valueSize);

            return nullable ? intermediate.Nullable() : intermediate.NotNullable();
        }
    }
}
