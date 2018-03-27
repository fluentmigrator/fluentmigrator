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
using System.Collections.Generic;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
    public class CreateTableExpression : MigrationExpressionBase
    {
        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual IList<ColumnDefinition> Columns { get; set; }
        public virtual string TableDescription { get; set; }

        public CreateTableExpression()
        {
            Columns = new List<ColumnDefinition>();
        }

        public override void ApplyConventions(IMigrationConventions conventions)
        {
            foreach (var column in Columns)
                column.ApplyConventions(conventions);
        }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (string.IsNullOrEmpty(TableName))
                errors.Add(ErrorMessages.TableNameCannotBeNullOrEmpty);

            foreach (var column in Columns)
                column.CollectValidationErrors(errors);
        }

        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        public override IMigrationExpression Reverse()
        {
            return new DeleteTableExpression
                    {
                        TableName = TableName,
                        SchemaName = SchemaName
                    };
        }

        public override string ToString()
        {
            return base.ToString() + TableName;
        }
    }
}