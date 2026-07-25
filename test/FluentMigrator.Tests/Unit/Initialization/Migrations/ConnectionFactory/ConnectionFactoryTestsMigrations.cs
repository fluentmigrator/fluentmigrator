#region License
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace FluentMigrator.Tests.Unit.Initialization.Migrations.ConnectionFactory
{
    [Migration(2026041901)]
    public sealed class CreateGlobalConnectionStringTable : Migration
    {
        public const long Version = 2026041901;

        public const string TableName = "GlobalConnectionStringFactoryTest";

        public override void Up()
        {
            Create.Table(TableName)
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey();
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }

    [Migration(2026041902)]
    public sealed class CreateFactoryConnectionTable : Migration
    {
        public const long Version = 2026041902;

        public const string TableName = "FactoryConnectionTest";

        public override void Up()
        {
            Create.Table(TableName)
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey();
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }

    [Migration(2026041903)]
    public sealed class CreatePreviewOnlyTable : Migration
    {
        public const long Version = 2026041903;

        public const string TableName = "PreviewOnlyConnectionFactoryTest";

        public override void Up()
        {
            Create.Table(TableName)
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey();
        }

        public override void Down()
        {
            Delete.Table(TableName);
        }
    }
}
