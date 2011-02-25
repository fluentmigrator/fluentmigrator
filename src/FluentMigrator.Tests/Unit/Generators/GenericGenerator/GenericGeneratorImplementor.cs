

namespace FluentMigrator.Tests.Unit.Generators
{
    using System;
    using FluentMigrator.Runner.Generators.Generic;

    public class GenericGeneratorImplementor : GenericGenerator
    {
        //This class is only used to allow the functions to be accessibly by the test classes

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GenericBaseImplementor"/> class.
        /// </summary>
        public GenericGeneratorImplementor() : base(null,null)
        {
            
        }

        public override string Generate(FluentMigrator.Expressions.CreateSchemaExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.DeleteSchemaExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.CreateTableExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.AlterColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.CreateColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.DeleteTableExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.DeleteColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.CreateForeignKeyExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.DeleteForeignKeyExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.CreateIndexExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.DeleteIndexExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.RenameTableExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.RenameColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.InsertDataExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.AlterDefaultConstraintExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.DeleteDataExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.UpdateDataExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(FluentMigrator.Expressions.AlterSchemaExpression expression)
        {
            throw new NotImplementedException();
        }
    }
}
