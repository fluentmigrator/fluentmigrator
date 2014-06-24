#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Builders.Create.ForeignKey;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Create.Sequence;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create
{
    public interface ICreateExpressionRoot : IFluentSyntax
    {
        void Schema(string schemaName);
        ICreateTableWithColumnOrSchemaOrDescriptionSyntax Table(string tableName);
        ICreateColumnOnTableSyntax Column(string columnName);
        ICreateForeignKeyFromTableSyntax ForeignKey();
        ICreateForeignKeyFromTableSyntax ForeignKey(string foreignKeyName);
        ICreateIndexForTableSyntax Index();
        ICreateIndexForTableSyntax Index(string indexName);

        ICreateSequenceInSchemaSyntax Sequence(string sequenceName);

        ICreateConstraintOnTableSyntax PrimaryKey();
        ICreateConstraintOnTableSyntax PrimaryKey(string primaryKeyName);

        ICreateConstraintOnTableSyntax UniqueConstraint();
        ICreateConstraintOnTableSyntax UniqueConstraint(string constraintName);
    }
}
