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

using System;
using System.Collections.Generic;
using System.ComponentModel;

using FluentMigrator.Builders.Merge;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Builders.Merge
{
    /// <summary>
    /// An expression builder for a <see cref="MergeDataExpression"/>
    /// </summary>
    public class MergeDataExpressionBuilder : IMergeDataOrInSchemaSyntax, IMergeRowSyntax, ISupportAdditionalFeatures
    {
        private readonly MergeDataExpression _expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeDataExpressionBuilder"/> class.
        /// </summary>
        /// <param name="expression">The underlying expression</param>
        public MergeDataExpressionBuilder(MergeDataExpression expression)
        {
            _expression = expression;
        }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures => _expression.AdditionalFeatures;

        /// <inheritdoc />
        public IMergeRowSyntax Row(object recordAsAnonymousType)
        {
            var data = ExtractData(recordAsAnonymousType);
            return Row(data);
        }

        /// <inheritdoc />
        public IMergeRowSyntax Row(IDictionary<string, object> record)
        {
            var dataDefinition = new InsertionDataDefinition();
            dataDefinition.AddRange(record);
            _expression.Rows.Add(dataDefinition);
            return this;
        }

        /// <inheritdoc />
        public IMergeDataSyntax InSchema(string schemaName)
        {
            _expression.SchemaName = schemaName;
            return this;
        }

        /// <inheritdoc />
        public void Match<T>(Func<T, object> selector)
        {
            // For simplicity, we'll add a method overload that accepts column names directly
            // The selector approach is complex to implement generically
            // Users can call Match with column names for now
            throw new NotSupportedException("Use Match(string[]) or Match(params string[]) overload to specify match columns by name.");
        }

        /// <summary>
        /// Specify the columns to match for determining if a row should be updated or inserted
        /// </summary>
        /// <param name="columnNames">The column names to use for matching</param>
        public void Match(params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                if (!_expression.MatchColumns.Contains(columnName))
                {
                    _expression.MatchColumns.Add(columnName);
                }
            }
        }

        private static IDictionary<string, object> ExtractData(object dataAsAnonymousType)
        {
            var data = new Dictionary<string, object>();
            var properties = TypeDescriptor.GetProperties(dataAsAnonymousType);

            foreach (PropertyDescriptor property in properties)
            {
                data.Add(property.Name, property.GetValue(dataAsAnonymousType));
            }

            return data;
        }
    }
}