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
using System.ComponentModel;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Delete
{
    public class DeleteDataExpressionBuilder : IDeleteDataOrInSchemaSyntax
    {
        private readonly DeleteDataExpression _expression;

        public DeleteDataExpressionBuilder(DeleteDataExpression expression)
        {
            _expression = expression;
        }

        public void IsNull(string columnName)
        {
            _expression.Rows.Add(new DeletionDataDefinition
                                    {
                                        new KeyValuePair<string, object>(columnName, null)
                                    });
        }

        public IDeleteDataSyntax Row(object dataAsAnonymousType)
        {
            _expression.Rows.Add(GetData(dataAsAnonymousType));
            return this;
        }

        public IDeleteDataSyntax InSchema(string schemaName)
        {
            _expression.SchemaName = schemaName;
            return this;
        }

        public void AllRows()
        {
            _expression.IsAllRows = true;
        }

        private static DeletionDataDefinition GetData(object dataAsAnonymousType)
        {
            var data = new DeletionDataDefinition();
            var properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

            foreach (PropertyDescriptor property in properties)
                data.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(dataAsAnonymousType)));
            return data;
        }
    }
}