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
    public class SQLiteGenerator : GenericGenerator
    {
        public SQLiteGenerator()
            : base(new SQLiteColumn(), new SQLiteQuoter(), new EmptyDescriptionGenerator())
        {
        }

        public override string RenameTable { get { return "ALTER TABLE {0} RENAME TO {1}"; } }

        public override string Generate(AlterColumnExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("SQLite does not support alter column");
        }

        public override string Generate(RenameColumnExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("SQLite does not support renaming of columns");
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("SQLite does not support deleting of columns");
        }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("SQLite does not support altering of default constraints");
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Foreign keys are not supported in SQLite");
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Foreign keys are not supported in SQLite");
        }

        public override string Generate(CreateSequenceExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Sequences are not supported in SQLite");
        }

        public override string Generate(DeleteSequenceExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Sequences are not supported in SQLite");
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Default constraints are not supported");
        }
    }
}
