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
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Insert
{
    public class InsertDataExpressionBuilder : IInsertDataOrInSchemaSyntax, ISupportAdditionalFeatures
    {
        private readonly InsertDataExpression _expression;

        public InsertDataExpressionBuilder(InsertDataExpression expression)
        {
            _expression = expression;
        }

        public IInsertDataSyntax Row(object dataAsAnonymousType)
        {
            IDictionary<string, object> data = ExtractData(dataAsAnonymousType);

            return Row(data);
        }

        public IInsertDataSyntax Row(IDictionary<string, object> data)
        {
            var dataDefinition = new InsertionDataDefinition();

            dataDefinition.AddRange(data);

            _expression.Rows.Add(dataDefinition);

            return this;
        }

        void ISupportAdditionalFeatures.AddAdditionalFeature(string feature, object value)
        {
            if (!_expression.AdditionalFeatures.ContainsKey(feature))
            {
                _expression.AdditionalFeatures.Add(feature, value);
            }
            else
            {
                _expression.AdditionalFeatures[feature] = value;
            }
        }

        public IInsertDataSyntax InSchema(string schemaName)
        {
            _expression.SchemaName = schemaName;
            return this;
        }

        private static IDictionary<string, object> ExtractData(object dataAsAnonymousType)
        {
            var data = new Dictionary<string, object>();

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

            foreach (PropertyDescriptor property in properties)
            {
                data.Add(property.Name, property.GetValue(dataAsAnonymousType));
            }

            return data;
        }
    }
}