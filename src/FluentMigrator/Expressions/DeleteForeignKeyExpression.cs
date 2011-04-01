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

using System;
using System.Collections.Generic;
using FluentMigrator.Model;
using System.Linq;

namespace FluentMigrator.Expressions
{
	public class DeleteForeignKeyExpression : MigrationExpressionBase
	{
		public virtual ForeignKeyDefinition ForeignKey { get; set; }

		public DeleteForeignKeyExpression()
		{
			ForeignKey = new ForeignKeyDefinition();
		}

        public override void ApplyConventions(IMigrationConventions conventions)
        {
            ForeignKey.ApplyConventions(conventions);
        }

		public override void CollectValidationErrors(ICollection<string> errors)
		{
			ForeignKey.CollectValidationErrors(errors);
		}

		public override void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}

        public override IMigrationExpression Reverse()
        {
            // there are 2 types of delete FK statements
            //  1) Delete.ForeignKey("FK_Name").OnTable("Table")
            //  2) Delete.ForeignKey()
            //      .FromTable("Table1").ForeignColumn("Id")
            //      .ToTable("Table2").PrimaryColumn("Id");

            // there isn't a way to autoreverse the type 1
            //  but we can turn the type 2 into Create.ForeignKey().FromTable() ...

            // only type 1 has the specific FK Name so if it's there then we can't auto-reverse
            if (!String.IsNullOrEmpty(ForeignKey.Name))
            {
                return base.Reverse();
            }

            return new CreateForeignKeyExpression { ForeignKey = ForeignKey.Clone() as ForeignKeyDefinition };
        }

		public override string ToString()
		{
			return base.ToString() + ForeignKey.Name + " "
				+ ForeignKey.ForeignTable + " (" + string.Join(", ", ForeignKey.ForeignColumns.ToArray()) + ") "
				+ ForeignKey.PrimaryTable + " (" + string.Join(", ", ForeignKey.PrimaryColumns.ToArray()) + ")";
		}
	}
}