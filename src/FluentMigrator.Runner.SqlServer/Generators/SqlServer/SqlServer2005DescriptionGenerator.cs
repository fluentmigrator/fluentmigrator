#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServer2005DescriptionGenerator : GenericDescriptionGenerator
    {
        #region Constants

        //language=sql
        private const string TableDescriptionTemplate = "EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{0}', @level0type=N'SCHEMA', @level0name='{1}', @level1type=N'TABLE', @level1name='{2}'";

        //language=sql
        private const string ColumnDescriptionTemplate = "EXEC sys.sp_addextendedproperty @name = N'MS_{0}', @value = N'{1}', @level0type = N'SCHEMA', @level0name = '{2}', @level1type = N'Table', @level1name = '{3}', @level2type = N'Column',  @level2name = '{4}'";

        //language=sql
        private const string RemoveTableDescriptionTemplate = "EXEC sys.sp_dropextendedproperty @name=N'MS_Description', @level0type=N'SCHEMA', @level0name='{0}', @level1type=N'TABLE', @level1name='{1}'";

        //language=sql
        private const string RemoveColumnDescriptionTemplate = "EXEC sys.sp_dropextendedproperty @name=N'MS_{0}', @level0type = N'SCHEMA', @level0name = '{1}', @level1type = N'Table', @level1name = '{2}', @level2type = N'Column',  @level2name = '{3}'";

        //language=sql
        private const string TableDescriptionVerificationTemplate = "IF EXISTS ( SELECT * FROM fn_listextendedproperty(N'MS_Description', N'SCHEMA', N'{0}', N'TABLE', N'{1}', NULL, NULL)) ";

        //language=sql
        private const string ColumnDescriptionVerificationTemplate = "IF EXISTS (SELECT * FROM fn_listextendedproperty(N'MS_{0}', N'SCHEMA', N'{1}', N'TABLE', N'{2}', N'Column', N'{3}' )) ";

        #endregion

        public override string GenerateDescriptionStatement(AlterTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableDescription))
                return string.Empty;

            var formattedSchemaName = FormatSchemaName(expression.SchemaName);

            // For this, we need to remove the extended property first if exists (or implement verification and use sp_updateextendedproperty)
            var tableVerificationStatement = string.Format(TableDescriptionVerificationTemplate, formattedSchemaName, expression.TableName);
            var removalStatement = GenerateTableDescriptionRemoval(formattedSchemaName, expression.TableName);
            var newDescriptionStatement = GenerateTableDescription(formattedSchemaName, expression.TableName, expression.TableDescription);

            return string.Join("", new[] { tableVerificationStatement, removalStatement, newDescriptionStatement });
        }

        public override string GenerateDescriptionStatement(AlterColumnExpression expression)
        {
            if (string.IsNullOrEmpty(expression.Column.ColumnDescription))
                return string.Empty;

            var formattedSchemaName = FormatSchemaName(expression.SchemaName);

            // For this, we need to remove the extended property first if exists (or implement verification and use sp_updateextendedproperty)
            var columnVerificationStatement = string.Format(ColumnDescriptionVerificationTemplate, "Description", formattedSchemaName, expression.TableName, expression.Column.Name);
            var removalStatement = GenerateColumnDescriptionRemoval("Description", formattedSchemaName, expression.TableName, expression.Column.Name);
            var newDescriptionStatement = GenerateColumnDescription("Description", formattedSchemaName, expression.TableName, expression.Column.Name, expression.Column.ColumnDescription);

            var initialDescriptionSatetement = string.Join("", new[] { columnVerificationStatement, removalStatement, newDescriptionStatement });

            if (expression.Column.AdditionalColumnDescriptions.Count == 0)
                return initialDescriptionSatetement;

            var descriptionsList = new List<string>
            {
                initialDescriptionSatetement
            };

            foreach (var description in expression.Column.AdditionalColumnDescriptions)
            {
                // For this, we need to remove the extended property first if exists (or implement verification and use sp_updateextendedproperty)
                columnVerificationStatement = string.Format(ColumnDescriptionVerificationTemplate, description.Key, formattedSchemaName, expression.TableName, expression.Column.Name);
                removalStatement = GenerateColumnDescriptionRemoval(description.Key, formattedSchemaName, expression.TableName, expression.Column.Name);
                newDescriptionStatement = GenerateColumnDescription(description.Key, formattedSchemaName, expression.TableName, expression.Column.Name, expression.Column.ColumnDescription);

                var newDescriptionStatementToAdd = string.Join("", new[] { columnVerificationStatement, removalStatement, newDescriptionStatement });
                descriptionsList.Add(newDescriptionStatementToAdd);
            }

            var joined = string.Join("" + Environment.NewLine, descriptionsList);

            return joined;
        }

        protected override string GenerateTableDescription(string schemaName, string tableName, string tableDescription)
        {
            if (string.IsNullOrEmpty(tableDescription))
                return string.Empty;

            var formattedSchemaName = FormatSchemaName(schemaName);

            return FormatStatement(TableDescriptionTemplate,
                tableDescription.Replace("'", "''"),
                formattedSchemaName,
                tableName);
        }

        protected override string GenerateColumnDescription(string descriptionName, string schemaName, string tableName, string columnName, string columnDescription)
        {
            if (string.IsNullOrEmpty(columnDescription))
                return string.Empty;

            var formattedSchemaName = FormatSchemaName(schemaName);

            return FormatStatement(ColumnDescriptionTemplate, descriptionName, columnDescription.Replace("'", "''"), formattedSchemaName, tableName, columnName);
        }

        private string GenerateTableDescriptionRemoval(string schemaName, string tableName)
        {
            return FormatStatement(RemoveTableDescriptionTemplate, schemaName, tableName);
        }

        private string GenerateColumnDescriptionRemoval(string descriptionName, string schemaName, string tableName, string columnName)
        {
            return FormatStatement(RemoveColumnDescriptionTemplate, descriptionName, schemaName, tableName, columnName);
        }

        private string FormatSchemaName(string schemaName)
        {
            return (string.IsNullOrEmpty(schemaName)) ? "dbo" : schemaName;
        }
    }
}
