using FluentMigrator.Expressions;

namespace FluentMigrator
{
    public interface IMigrationGenerator
    {
        string Generate(CreateSchemaExpression expression);
        string Generate(DeleteSchemaExpression expression);
        string Generate(CreateTableExpression expression);
        string Generate(AlterTableExpression expression);
        string Generate(AlterColumnExpression expression);
        string Generate(CreateColumnExpression expression);
        string Generate(DeleteTableExpression expression);
        string Generate(DeleteColumnExpression expression);
        string Generate(CreateForeignKeyExpression expression);
        string Generate(DeleteForeignKeyExpression expression);
        string Generate(CreateIndexExpression expression);
        string Generate(DeleteIndexExpression expression);
        string Generate(RenameTableExpression expression);
        string Generate(RenameColumnExpression expression);
        string Generate(InsertDataExpression expression);
        string Generate(AlterDefaultConstraintExpression expression);
        string Generate(DeleteDataExpression expression);
        string Generate(UpdateDataExpression expression);
        string Generate(AlterSchemaExpression expression);
        string Generate(CreateSequenceExpression expression);

        string Generate(DeleteSequenceExpression expression);
        string Generate(DeleteDefaultConstraintExpression expression);
    }
}