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
using System.Data;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
	public class ForeignKeyDefinition : ICloneable, ICanBeConventional, ICanBeValidated
	{
		public virtual string Name { get; set; }
		public virtual string TableContainingForeignKey { get; set; }
		public virtual string SchemaOfTableContainingForeignKey { get; set; }
		public virtual string TableContainingPrimayKey { get; set; }
		public virtual string SchemaOfTableContainingPrimaryKey { get; set; }
		public virtual Rule OnDelete { get; set; }
		public virtual Rule OnUpdate { get; set; }
		public virtual ICollection<string> ColumnsInForeignKeyTableToInclude { get; set; }
		public virtual ICollection<string> ColumnsInPrimaryKeyTableToInclude { get; set; }

		public ForeignKeyDefinition()
		{
			ColumnsInForeignKeyTableToInclude = new List<string>();
			ColumnsInPrimaryKeyTableToInclude = new List<string>();
		}

		public void ApplyConventions(IMigrationConventions conventions)
		{
			if (String.IsNullOrEmpty(Name))
				Name = conventions.GetForeignKeyName(this);
		}

		public virtual void CollectValidationErrors(ICollection<string> errors)
		{
			if (String.IsNullOrEmpty(Name))
				errors.Add(ErrorMessages.ForeignKeyNameCannotBeNullOrEmpty);

			if (String.IsNullOrEmpty(TableContainingForeignKey))
				errors.Add(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);

			if (String.IsNullOrEmpty(TableContainingPrimayKey))
				errors.Add(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);

			if (!String.IsNullOrEmpty(TableContainingForeignKey) && !String.IsNullOrEmpty(TableContainingPrimayKey) && TableContainingForeignKey.Equals(TableContainingPrimayKey))
				errors.Add(ErrorMessages.ForeignKeyCannotBeSelfReferential);

			if (ColumnsInForeignKeyTableToInclude.Count == 0)
				errors.Add(ErrorMessages.ForeignKeyMustHaveOneOrMoreForeignColumns);

			if (ColumnsInPrimaryKeyTableToInclude.Count == 0)
				errors.Add(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns);
		}

		public object Clone()
		{
			return new ForeignKeyDefinition
			{
				Name = Name,
				SchemaOfTableContainingForeignKey = SchemaOfTableContainingForeignKey,
				TableContainingForeignKey = TableContainingForeignKey,
				SchemaOfTableContainingPrimaryKey = SchemaOfTableContainingPrimaryKey,
				TableContainingPrimayKey = TableContainingPrimayKey,
				ColumnsInForeignKeyTableToInclude = new List<string>(ColumnsInForeignKeyTableToInclude),
				ColumnsInPrimaryKeyTableToInclude = new List<string>(ColumnsInPrimaryKeyTableToInclude),
                OnDelete = OnDelete,
                OnUpdate = OnUpdate
			};
		}
	}
}