#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2010, Nathan Brown
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

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.SQLite
{
    public class SqliteGenerator : GenericGenerator
    {
        public SqliteGenerator()
            : base(new SqliteColumn(), new SqliteQuoter())
        {
        }

        public override string RenameTable { get { return "ALTER TABLE {0} RENAME TO {1}"; } }

        public override string Generate(AlterColumnExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("SQLite does not support altering column.");
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("SQLite does not support renaming columns.");
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("SQLite does not support deleting columns.");
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("SQLite does not support default constraints.");
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("SQLite does not support foreign keys.");
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("SQLite does not support foreign keys.");
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("SQLite does not support sequences.");
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("SQLite does not support sequences.");
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return this.CompatibilityMode.GetNotSupported("SQLite does not support default constraints.");
        }
    }
}
