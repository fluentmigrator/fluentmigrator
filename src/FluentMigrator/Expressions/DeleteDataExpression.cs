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

using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
    public class DeleteDataExpression : IMigrationExpression
    {
        private readonly List<DeletionDataDefinition> _rows = new List<DeletionDataDefinition>();
        public virtual string SchemaName { get; set; }
        public string TableName { get; set; }
        public virtual bool IsAllRows { get; set; }

        public List<DeletionDataDefinition> Rows
        {
            get { return _rows; }
        }

        public void CollectValidationErrors(ICollection<string> errors)
        {
        }

        public void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        public IMigrationExpression Reverse()
        {
            var expression = new InsertDataExpression
            {
                SchemaName = SchemaName,
                TableName = TableName
            };

            foreach (var row in Rows)
            {
                var dataDefinition = new InsertionDataDefinition();
                dataDefinition.AddRange(row);

                expression.Rows.Add(dataDefinition);
            }

            return expression;
        }

        public void ApplyConventions(IMigrationConventions conventions)
        {
        }
    }
}