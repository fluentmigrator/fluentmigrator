
namespace FluentMigrator.Tests.Unit.Generators
{


    using System.Collections.Generic;
    using System.Data;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;

    public abstract class BaseTableCreateTests : GeneratorTestBase
    {
        //Create Tests
        public abstract void CanCreateTable();
        public abstract void CanCreateTableWithCustomColumnType();

        public abstract void CanCreateTableWithPrimaryKey();
        public abstract void CanCreateTableNamedPrimaryKey();
        public abstract void CanCreateTableNamedMultiColumnPrimaryKey();

        public abstract void CanCreateTableWithIdentity();
        public abstract void CanCreateTableWithNullableField();
        public abstract void CanCreateTableWithDefaultValue();
        public abstract void CanCreateTableWithDefaultValueExplicitlySetToNull();
        public abstract void CanCreateTableWithMultiColumnPrimaryKey();

        public abstract void CanCreateIndex();
        public abstract void CanCreateMultiColumnIndex();
        public abstract void CanCreateUniqueIndex();
        public abstract void CanCreateMultiColumnUniqueIndex();

        public abstract void CanCreateSchema();

        public abstract void CanCreateTableWithIFNotExists();

        

       

        

       
    }
}
