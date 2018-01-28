#if !NETSTANDARD2_0
using System.Data.Common;

namespace FluentMigrator.Runner.Processors.Hana
{
    public class HanaDbFactory : DbFactoryBase
    {
        protected override DbProviderFactory CreateFactory()
        {
            return DbProviderFactories.GetFactory("Sap.Data.Hana");
        }
    }
}
#endif