using System;

using Microsoft.CodeAnalysis;

namespace FluentMigrator.Analyzers
{
    public class FluentMigratorContext
    {
        readonly Lazy<INamedTypeSymbol> _lazyMigrationAttributeType;

        internal FluentMigratorContext(Compilation compilation)
        {
            Compilation = compilation;

            _lazyMigrationAttributeType = new Lazy<INamedTypeSymbol>(() => compilation.GetTypeByMetadataName(Constants.Types.FluentMigratorMigrationAttribute));
        }

        public Compilation Compilation { get; set; }

        public INamedTypeSymbol MigrationAttributeType
            => _lazyMigrationAttributeType?.Value;
    }
}
