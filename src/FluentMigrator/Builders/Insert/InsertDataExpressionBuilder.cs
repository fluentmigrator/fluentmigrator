#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Insert
{
    /// <summary>
    /// An expression builder for a <see cref="InsertDataExpression"/>
    /// </summary>
    public class InsertDataExpressionBuilder : IInsertDataOrInSchemaSyntax, ISupportAdditionalFeatures
    {
        private readonly InsertDataExpression _expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertDataExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public InsertDataExpressionBuilder(InsertDataExpression expression)
        {
            _expression = expression;
        }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures => _expression.AdditionalFeatures;

        /// <inheritdoc />
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
        public IInsertDataSyntax Row(object recordAsAnonymousType)
        {
            return Rows(recordAsAnonymousType);
        }

        /// <inheritdoc />
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
        public IInsertDataSyntax Rows(params object[] recordsAsAnonymousTypes)
        {
            var records = recordsAsAnonymousTypes.Select(ExtractData).ToArray();
            return Rows(records);
        }

        /// <inheritdoc />
        public IInsertDataSyntax Row(IDictionary<string, object> record)
        {
            return Rows(record);
        }

        /// <inheritdoc />
        public IInsertDataSyntax Rows(params IDictionary<string, object>[] records)
        {
            var dataDefinitions = records.Select(record =>
            {
                var dataDefinition = new InsertionDataDefinition();
                dataDefinition.AddRange(record);
                return dataDefinition;
            });

            _expression.Rows.AddRange(dataDefinitions);

            return this;
        }

        /// <inheritdoc />
        public IInsertDataSyntax InSchema(string schemaName)
        {
            _expression.SchemaName = schemaName;
            return this;
        }

#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
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
