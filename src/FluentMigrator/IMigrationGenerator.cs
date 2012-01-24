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

using FluentMigrator.Expressions;

namespace FluentMigrator
{
    public interface IMigrationGenerator
    {
        /// <summary>Whether to throw a exception when a SQL command is not supported by the underlying database type.</summary>
        bool StrictCompatibility { get; set; }

        /// <summary>Whether to imitate database support for some SQL commands that are not supported by the underlying database type.</summary>
        /// <remarks>For example, schema support can be emulated by prefixing the schema name to the table name (<c>`schema`.`table`</c> => <c>`schema_table`</c>).</remarks>
        bool EmulateCompatibility { get; set; }
        
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
        string Generate(CreateConstraintExpression expression);
        string Generate(DeleteConstraintExpression expression);
        string Generate(DeleteDefaultConstraintExpression expression);
    }
}
