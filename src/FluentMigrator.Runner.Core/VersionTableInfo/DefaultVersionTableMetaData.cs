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

using System;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.VersionTableInfo
{
    /// <summary>
    /// Provides default version table metadata for tracking migrations.
    /// </summary>
    public class DefaultVersionTableMetaData : IVersionTableMetaData, ISchemaExpression
    {
        /// <inheritdoc />
        public DefaultVersionTableMetaData(IConventionSet conventionSet, IOptions<RunnerOptions> runnerOptions)
        {
            conventionSet.SchemaConvention?.Apply(this);
        }

        /// <inheritdoc />
        public virtual string SchemaName { get; set; }

        /// <inheritdoc />
        public virtual string TableName => "VersionInfo";

        /// <inheritdoc />
        public virtual string ColumnName => "Version";

        /// <inheritdoc />
        public virtual string UniqueIndexName => "UC_Version";

        /// <inheritdoc />
        public virtual string AppliedOnColumnName => "AppliedOn";

        /// <inheritdoc />
        public bool CreateWithPrimaryKey => false;

        /// <inheritdoc />
        public virtual string DescriptionColumnName => "Description";

        /// <inheritdoc />
        public virtual bool OwnsSchema => true;
    }
}
