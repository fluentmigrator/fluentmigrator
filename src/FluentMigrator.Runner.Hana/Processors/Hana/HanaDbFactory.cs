using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Runner.Processors.Hana
{
    public class HanaDbFactory : ReflectionBasedDbFactory
    {
        public HanaDbFactory()
            : base(GetTestEntries().ToArray())
        {
        }

        private static IEnumerable<TestEntry> GetTestEntries()
        {
            yield return new TestEntry("Sap.Data.Hana", "Sap.Data.Hana.HanaFactory");
#if NETSTANDARD2_0
            yield return new TestEntry("Sap.Data.Hana.v4.5", "Sap.Data.Hana.HanaFactory");
#elif NET45
            yield return new TestEntry("Sap.Data.Hana.v4.5", "Sap.Data.Hana.HanaFactory");
            yield return new TestEntry("Sap.Data.Hana.v3.5", "Sap.Data.Hana.HanaFactory");
#else
            yield return new TestEntry("Sap.Data.Hana.v3.5", "Sap.Data.Hana.HanaFactory");
#endif
        }
    }
}
