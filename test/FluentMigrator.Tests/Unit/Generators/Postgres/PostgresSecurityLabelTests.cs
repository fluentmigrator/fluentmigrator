#region License
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
#endregion

using System;

using FluentMigrator.Builder.SecurityLabel;
using FluentMigrator.Postgres;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Generators.Postgres
{
    [TestFixture]
    [Category("Generator")]
    [Category("Postgres")]
    public class PostgresSecurityLabelTests
    {
        [Test]
        public void CanGenerateSecurityLabelOnTable()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Table,
                ObjectName = "users",
                Label = "TABLESAMPLE BERNOULLI(10)"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL ON TABLE \"users\" IS 'TABLESAMPLE BERNOULLI(10)';");
        }

        [Test]
        public void CanGenerateSecurityLabelOnTableWithSchema()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Table,
                SchemaName = "public",
                ObjectName = "users",
                Label = "some label"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL ON TABLE \"public\".\"users\" IS 'some label';");
        }

        [Test]
        public void CanGenerateSecurityLabelOnTableWithProvider()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Table,
                SchemaName = "public",
                ObjectName = "users",
                Provider = "anon",
                Label = "TABLESAMPLE BERNOULLI(10)"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL FOR \"anon\" ON TABLE \"public\".\"users\" IS 'TABLESAMPLE BERNOULLI(10)';");
        }

        [Test]
        public void CanGenerateSecurityLabelOnColumn()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Column,
                ObjectName = "users",
                ColumnName = "email",
                Label = "MASKED WITH FUNCTION anon.fake_email()"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL ON COLUMN \"users\".\"email\" IS 'MASKED WITH FUNCTION anon.fake_email()';");
        }

        [Test]
        public void CanGenerateSecurityLabelOnColumnWithSchema()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Column,
                SchemaName = "public",
                ObjectName = "users",
                ColumnName = "email",
                Label = "MASKED WITH FUNCTION anon.fake_email()"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL ON COLUMN \"public\".\"users\".\"email\" IS 'MASKED WITH FUNCTION anon.fake_email()';");
        }

        [Test]
        public void CanGenerateSecurityLabelOnColumnWithProvider()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Column,
                SchemaName = "public",
                ObjectName = "users",
                ColumnName = "email",
                Provider = "anon",
                Label = "MASKED WITH FUNCTION anon.fake_email()"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL FOR \"anon\" ON COLUMN \"public\".\"users\".\"email\" IS 'MASKED WITH FUNCTION anon.fake_email()';");
        }

        [Test]
        public void CanGenerateSecurityLabelOnSchema()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Schema,
                ObjectName = "public",
                Provider = "sepgsql",
                Label = "system_u:object_r:sepgsql_schema_t:s0"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL FOR \"sepgsql\" ON SCHEMA \"public\" IS 'system_u:object_r:sepgsql_schema_t:s0';");
        }

        [Test]
        public void CanGenerateSecurityLabelOnRole()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Role,
                ObjectName = "masked_user",
                Provider = "anon",
                Label = "MASKED"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL FOR \"anon\" ON ROLE \"masked_user\" IS 'MASKED';");
        }

        [Test]
        public void CanGenerateSecurityLabelOnView()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.View,
                SchemaName = "public",
                ObjectName = "user_view",
                Provider = "anon",
                Label = "VIEW LABEL"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL FOR \"anon\" ON VIEW \"public\".\"user_view\" IS 'VIEW LABEL';");
        }

        [Test]
        public void CanDeleteSecurityLabelFromTable()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Table,
                SchemaName = "public",
                ObjectName = "users",
                Provider = "anon"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateDeleteSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL FOR \"anon\" ON TABLE \"public\".\"users\" IS NULL;");
        }

        [Test]
        public void CanDeleteSecurityLabelFromColumn()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Column,
                SchemaName = "public",
                ObjectName = "users",
                ColumnName = "email",
                Provider = "anon"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateDeleteSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL FOR \"anon\" ON COLUMN \"public\".\"users\".\"email\" IS NULL;");
        }

        [Test]
        public void CanDeleteSecurityLabelFromSchema()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Schema,
                ObjectName = "public",
                Provider = "sepgsql"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateDeleteSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL FOR \"sepgsql\" ON SCHEMA \"public\" IS NULL;");
        }

        [Test]
        public void CanDeleteSecurityLabelFromRole()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Role,
                ObjectName = "masked_user",
                Provider = "anon"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateDeleteSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL FOR \"anon\" ON ROLE \"masked_user\" IS NULL;");
        }

        [Test]
        public void CanDeleteSecurityLabelFromView()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.View,
                SchemaName = "public",
                ObjectName = "user_view",
                Provider = "anon"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateDeleteSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL FOR \"anon\" ON VIEW \"public\".\"user_view\" IS NULL;");
        }

        [Test]
        public void LabelEscapesSingleQuotes()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Table,
                ObjectName = "users",
                Label = "It's a label"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL ON TABLE \"users\" IS 'It''s a label';");
        }

        [Test]
        public void IdentifierEscapesDoubleQuotes()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Table,
                ObjectName = "my\"table",
                Label = "test"
            };

            var result = PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition);

            result.ShouldBe("SECURITY LABEL ON TABLE \"my\"\"table\" IS 'test';");
        }

        [Test]
        public void ThrowsWhenLabelIsNullForCreate()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Table,
                ObjectName = "users"
            };

            Should.Throw<ArgumentException>(() =>
                PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition));
        }

        [Test]
        public void ThrowsWhenDefinitionIsNull()
        {
            Should.Throw<ArgumentNullException>(() =>
                PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(null));
        }

        [Test]
        public void ThrowsWhenColumnNameMissingForColumn()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Column,
                ObjectName = "users",
                Label = "test"
            };

            Should.Throw<ArgumentException>(() =>
                PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition));
        }

        [Test]
        public void ThrowsWhenTableNameMissingForColumn()
        {
            var definition = new PostgresSecurityLabelDefinition
            {
                ObjectType = PostgresSecurityLabelObjectType.Column,
                ColumnName = "email",
                Label = "test"
            };

            Should.Throw<ArgumentException>(() =>
                PostgresSecurityLabelSqlGenerator.GenerateCreateSecurityLabelSql(definition));
        }
    }
}
