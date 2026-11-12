#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.Oracle;

namespace FluentMigrator.Tests.Integration.CreationScript.Example
{
    /// <summary>
    /// FluentMigrator.Extensions.Oracle.
    /// </summary>
    [Migration(22, "Oracle extensions")]
    public class M0022_Oracle : ForwardOnlyMigration
    {
        public override void Up()
        {
            var oracle = IfDatabase(Db.Oracle);

            oracle.Create.Table("Ticket")
                .WithColumn("Id").AsInt64().NotNullable().PrimaryKey("PK_Ticket").Identity(OracleGenerationType.ByDefault, 100, 1)
                .WithColumn("Subject").AsString(200).NotNullable();

            // BY DEFAULT: the database numbers a row unless the insert gives the id.
            oracle.Insert.IntoTable("Ticket").Row(new { Subject = "Numbered by the database" });
            oracle.Insert.IntoTable("Ticket").Row(new { Id = 1, Subject = "Numbered by the insert" });
        }
    }
}
