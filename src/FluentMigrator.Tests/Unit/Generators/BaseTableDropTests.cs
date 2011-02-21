
using FluentMigrator.Expressions;
using FluentMigrator.Model;
namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseTableDropTests : GeneratorTestBase
    {
        //Drop Tests
        public abstract void CanDropColumn();
        public abstract void CanDropForeignKey();
        public abstract void CanDropTable();
        public abstract void CanDeleteIndex();

        public abstract void CanDeleteSchema();
        
        

    }
}
