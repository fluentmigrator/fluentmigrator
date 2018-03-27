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

using System;
using System.Data;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Expressions;

namespace FluentMigrator
{
    public interface IMigrationProcessor : IQuerySchema, IDisposable
    {
        IMigrationProcessorOptions Options { get; }
        string ConnectionString { get; }

        void Execute(string template, params object[] args);
        DataSet ReadTableData(string schemaName, string tableName);
        DataSet Read(string template, params object[] args);
        bool Exists(string template, params object[] args);

        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();

        void Process(CreateSchemaExpression expression);
        void Process(DeleteSchemaExpression expression);
        void Process(AlterTableExpression expression);
        void Process(AlterColumnExpression expression);
        void Process(CreateTableExpression expression);
        void Process(CreateColumnExpression expression);
        void Process(DeleteTableExpression expression);
        void Process(DeleteColumnExpression expression);
        void Process(CreateForeignKeyExpression expression);
        void Process(DeleteForeignKeyExpression expression);
        void Process(CreateIndexExpression expression);
        void Process(DeleteIndexExpression expression);
        void Process(RenameTableExpression expression);
        void Process(RenameColumnExpression expression);
        void Process(InsertDataExpression expression);
        void Process(AlterDefaultConstraintExpression expression);
        void Process(PerformDBOperationExpression expression);
        void Process(DeleteDataExpression expression);
        void Process(UpdateDataExpression expression);
        void Process(AlterSchemaExpression expression);

        void Process(CreateSequenceExpression expression);

        void Process(DeleteSequenceExpression expression);

        void Process(CreateConstraintExpression expression);
        void Process(DeleteConstraintExpression expression);
        void Process(DeleteDefaultConstraintExpression expression);
    }
}
