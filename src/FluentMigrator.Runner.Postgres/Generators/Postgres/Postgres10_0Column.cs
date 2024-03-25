#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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
    class Postgres10_0Column : PostgresColumn
    {
        public Postgres10_0Column(PostgresQuoter quoter, IPostgresTypeMap typeMap)
            : base(quoter, typeMap)
        {
            AlterClauseOrder.Add(FormatAlterIdentity);
        }

        private string FormatAlterIdentity(ColumnDefinition column)
        {
            if (!column.IsIdentity)
            {
                return string.Empty;
            }

            switch (column.GetAdditionalFeature(PostgresExtensions.IdentityModificationType, PostgresIdentityModificationType.Add))
            {
                case PostgresIdentityModificationType.Add:
                    return string.Format("ADD {0} AS IDENTITY", FormatIdentity(column));

                case PostgresIdentityModificationType.Set:
                    return string.Format("SET {0}", FormatIdentity(column));

                case PostgresIdentityModificationType.Drop:
                    return "DROP IDENTITY";

                case PostgresIdentityModificationType.DropIfExists:
                    return "DROP IDENTITY IF EXISTS";

                default:
                    throw new ArgumentOutOfRangeException();
            }
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

            if (column.ModificationType == ColumnModificationType.Create)
            {
                return $"GENERATED {generationType} AS IDENTITY";
            }

            return $"GENERATED {generationType}";
        }

        protected override string FormatType(ColumnDefinition column)
        {
            FormatTypeValidator(column);

            //rather than base.FormatType, which will use serials for identities, we go instead to ColumnBase.FormatType, exposed via base.ColumnBaseFormatType
            return base.ColumnBaseFormatType(column);
        }
    }
}
