using System;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Tests.Unit.Generators
{
    public class GenericGeneratorImplementor : GenericGenerator
    {
        //This class is only used to allow the functions to be accessibly by the test classes

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GenericBaseImplementor"/> class.
        /// </summary>
        public GenericGeneratorImplementor() : base(null, null)
        {
        }

        public override string Generate(CreateSchemaExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(DeleteSchemaExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(CreateTableExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(AlterColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(CreateColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(DeleteTableExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(CreateIndexExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(RenameTableExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(RenameColumnExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(InsertDataExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(DeleteDataExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(UpdateDataExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(AlterSchemaExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            throw new NotImplementedException();
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            throw new NotImplementedException();
        }
    }
}