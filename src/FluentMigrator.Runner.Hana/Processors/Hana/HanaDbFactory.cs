namespace FluentMigrator.Runner.Processors.Hana
{
    public class HanaDbFactory : ReflectionBasedDbFactory
    {
        public HanaDbFactory()
            : base("Sap.Data.Hana", "Sap.Data.Hana.HanaFactory")
        {
        }
    }
}
