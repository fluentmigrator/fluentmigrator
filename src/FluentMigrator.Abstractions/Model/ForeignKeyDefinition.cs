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
        public virtual string ForeignTable { get; set; }
        public virtual string ForeignTableSchema { get; set; }
        public virtual string PrimaryTable { get; set; }
        public virtual string PrimaryTableSchema { get; set; }
        public virtual Rule OnDelete { get; set; }
        public virtual Rule OnUpdate { get; set; }
        public virtual ICollection<string> ForeignColumns { get; set; }
        public virtual ICollection<string> PrimaryColumns { get; set; }

        public ForeignKeyDefinition()
        {
            ForeignColumns = new List<string>();
            PrimaryColumns = new List<string>();
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

            if (String.IsNullOrEmpty(ForeignTable))
                errors.Add(ErrorMessages.ForeignTableNameCannotBeNullOrEmpty);

            if (String.IsNullOrEmpty(PrimaryTable))
                errors.Add(ErrorMessages.PrimaryTableNameCannotBeNullOrEmpty);

            if (ForeignColumns.Count == 0)
                errors.Add(ErrorMessages.ForeignKeyMustHaveOneOrMoreForeignColumns);

            if (PrimaryColumns.Count == 0)
                errors.Add(ErrorMessages.ForeignKeyMustHaveOneOrMorePrimaryColumns);
        }

        public object Clone()
        {
            return new ForeignKeyDefinition
            {
                Name = Name,
                ForeignTableSchema = ForeignTableSchema,
                ForeignTable = ForeignTable,
                PrimaryTableSchema = PrimaryTableSchema,
                PrimaryTable = PrimaryTable,
                ForeignColumns = new List<string>(ForeignColumns),
                PrimaryColumns = new List<string>(PrimaryColumns),
                OnDelete = OnDelete,
                OnUpdate = OnUpdate
            };
        }

        public bool HasForeignAndPrimaryColumnsDefined()
        {
            return ForeignColumns.Count > 0 && PrimaryColumns.Count > 0;
        }
    }
}