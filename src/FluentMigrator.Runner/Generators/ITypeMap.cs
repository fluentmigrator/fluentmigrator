using System.Data;

namespace FluentMigrator.Runner.Generators
{
    public interface ITypeMap
    {
        string GetTypeMap(DbType type, int size, int precision);
    }
}
