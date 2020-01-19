#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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

using System;

using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Postgres;

namespace FluentMigrator.Runner.Generators.Postgres
{
    class Postgres100Column : PostgresColumn
    {
        public Postgres100Column(PostgresQuoter quoter, ITypeMap typeMap)
            : base(quoter, typeMap)
        {
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            if (!column.IsIdentity)
            {
                return string.Empty;
            }

            string generationType;
            switch (column.GetAdditionalFeature(PostgresExtensions.IdentityGeneration, PostgresGenerationType.Always))
            {
                case PostgresGenerationType.Always:
                    generationType = "ALWAYS";
                    break;

                case PostgresGenerationType.ByDefault:
                    generationType = "BY DEFAULT";
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"GENERATED {generationType} AS IDENTITY";
        }

        protected override string FormatType(ColumnDefinition column)
        {
            //Really want to skip PostgresColumn.FormatType and go straight to ColumnBase.FormatType, but not sure how to do that without reflection...
            //return ((ColumnBase)this).FormatType(column);
            //This thanks to polymorphism will throw a StackOverflowException... seems I need to edit MSIL to do this
            //var superClassMethodInfp = typeof(ColumnBase).GetMethod(nameof(FormatType), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            //var abc = (string)superclassMethodInfo.Invoke(this, new[] { column });

            //This is a copy of the base class implementation
            if (!column.Type.HasValue)
            {
                return column.CustomType;
            }
            return GetTypeMap(column.Type.Value, column.Size, column.Precision);
        }
    }
}
