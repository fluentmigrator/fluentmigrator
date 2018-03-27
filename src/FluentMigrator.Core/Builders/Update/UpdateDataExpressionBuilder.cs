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
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Update
{
    public class UpdateDataExpressionBuilder : IUpdateSetOrInSchemaSyntax,
        IUpdateWhereSyntax
    {
        private readonly UpdateDataExpression _expression;
        private readonly IMigrationContext _context;

        public UpdateDataExpressionBuilder(UpdateDataExpression expression, IMigrationContext context)
        {
            _context = context;
            _expression = expression;
        }

        public IUpdateSetSyntax InSchema(string schemaName)
        {
            _expression.SchemaName = schemaName;
            return this;
        }

        public IUpdateWhereSyntax Set(object dataAsAnonymousType)
        {
            _expression.Set = GetData(dataAsAnonymousType);
            return this;
        }

        public void Where(object dataAsAnonymousType)
        {
            _expression.Where = GetData(dataAsAnonymousType);
        }

        public void AllRows()
        {
            _expression.IsAllRows = true;
        }

        private static List<KeyValuePair<string, object>> GetData(object dataAsAnonymousType)
        {
            var data = new List<KeyValuePair<string, object>>();
            var properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

            foreach (PropertyDescriptor property in properties)
                data.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(dataAsAnonymousType)));
            return data;
        }
    }
}
