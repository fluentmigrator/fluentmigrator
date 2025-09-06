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

using FluentMigrator;
using System.Data;

namespace FluentMigrator.Example.MultiColumnForeignKey
{
    /// <summary>
    /// Example migration demonstrating multi-column foreign key support for SQLite
    /// 
    /// This creates the exact scenario from the GitHub issue:
    /// - AreaGroup table with composite primary key (ArticleId, Index)
    /// - Area table with composite primary key (ArticleId, AreaGroupIndex, Index)
    /// - Area table has a foreign key (ArticleId, AreaGroupIndex) referencing AreaGroup(ArticleId, Index)
    /// </summary>
    [Migration(202412130001)]
    public class CreateAreaTablesWithMultiColumnForeignKey : Migration
    {
        public override void Up()
        {
            // Create AreaGroup table first (referenced table)
            Create.Table("AreaGroup")
                .WithColumn("ArticleId").AsString().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                .WithColumn("Name").AsString().Nullable();

            // Add composite primary key to AreaGroup
            Create.PrimaryKey("PK_AreaGroup")
                .OnTable("AreaGroup")
                .Columns("ArticleId", "Index");

            // Create Area table with composite primary key and foreign key
            // This demonstrates the new multi-column foreign key fluent syntax
            Create.Table("Area")
                .WithColumn("ArticleId").AsString().NotNullable()
                .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                // The following line uses the new multi-column foreign key syntax
                .ForeignKey(["ArticleId", "AreaGroupIndex"], "AreaGroup", ["ArticleId", "Index"])
                .OnDelete(Rule.Cascade);

            // Add composite primary key to Area
            Create.PrimaryKey("PK_Area")
                .OnTable("Area")
                .Columns("ArticleId", "AreaGroupIndex", "Index");
        }

        public override void Down()
        {
            Delete.Table("Area");
            Delete.Table("AreaGroup");
        }
    }

    /// <summary>
    /// Alternative example showing named foreign key
    /// </summary>
    [Migration(202412130002)]
    public class CreateAreaTablesWithNamedMultiColumnForeignKey : Migration
    {
        public override void Up()
        {
            // Create AreaGroup table first (referenced table)
            Create.Table("AreaGroup")
                .WithColumn("ArticleId").AsString().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                .WithColumn("Name").AsString().Nullable();

            // Add composite primary key to AreaGroup
            Create.PrimaryKey("PK_AreaGroup")
                .OnTable("AreaGroup")
                .Columns("ArticleId", "Index");

            // Create Area table with named multi-column foreign key
            Create.Table("Area")
                .WithColumn("ArticleId").AsString().NotNullable()
                .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                // This demonstrates the named version of the new multi-column foreign key syntax
                .ForeignKey("FK_Area_AreaGroup", ["ArticleId", "AreaGroupIndex"], "AreaGroup", ["ArticleId", "Index"])
                .OnDelete(Rule.Cascade);

            // Add composite primary key to Area
            Create.PrimaryKey("PK_Area")
                .OnTable("Area")
                .Columns("ArticleId", "AreaGroupIndex", "Index");
        }

        public override void Down()
        {
            Delete.Table("Area");
            Delete.Table("AreaGroup");
        }
    }

    /// <summary>
    /// Example showing foreign key with schema specification
    /// </summary>
    [Migration(202412130003)]
    public class CreateAreaTablesWithSchemaMultiColumnForeignKey : Migration
    {
        public override void Up()
        {
            // Create schema first
            Create.Schema("content");

            // Create AreaGroup table first (referenced table)
            Create.Table("AreaGroup").InSchema("content")
                .WithColumn("ArticleId").AsString().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                .WithColumn("Name").AsString().Nullable();

            // Add composite primary key to AreaGroup
            Create.PrimaryKey("PK_AreaGroup")
                .OnTable("AreaGroup").InSchema("content")
                .Columns("ArticleId", "Index");

            // Create Area table with schema-aware multi-column foreign key
            Create.Table("Area").InSchema("content")
                .WithColumn("ArticleId").AsString().NotNullable()
                .WithColumn("AreaGroupIndex").AsInt32().NotNullable()
                .WithColumn("Index").AsInt32().NotNullable()
                // This demonstrates the schema-aware version of the new multi-column foreign key syntax
                .ForeignKey("FK_Area_AreaGroup", ["ArticleId", "AreaGroupIndex"], "content", "AreaGroup", ["ArticleId", "Index"])
                .OnDelete(Rule.Cascade);

            // Add composite primary key to Area
            Create.PrimaryKey("PK_Area")
                .OnTable("Area").InSchema("content")
                .Columns("ArticleId", "AreaGroupIndex", "Index");
        }

        public override void Down()
        {
            Delete.Table("Area").InSchema("content");
            Delete.Table("AreaGroup").InSchema("content");
            Delete.Schema("content");
        }
    }
}