using System.Linq;
using System;

namespace FluentMigrator.Builders.Delete.ForeignKey
{
    public interface IDeleteForeignKeyPrimaryColumnOrInSchemaSyntax : IDeleteForeignKeyPrimaryColumnSyntax
    {
        IDeleteForeignKeyPrimaryColumnSyntax InSchema(string schema);
    }
}