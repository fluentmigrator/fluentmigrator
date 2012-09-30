namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServer2012Generator : SqlServer2008Generator
    {
        public SqlServer2012Generator()
            :base(new SqlServerColumn(new SqlServer2008TypeMap()))
        {
        }
    }
}