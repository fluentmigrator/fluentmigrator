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

using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    /// <summary>
    /// The SQL Server 2016 SQL generator for FluentMigrator.
    /// </summary>
    public class SqlServer2016Generator : SqlServer2014Generator
    {
        /// <inheritdoc />
        public SqlServer2016Generator()
            : this(new SqlServer2008Quoter())
        {
        }

        /// <inheritdoc />
        public SqlServer2016Generator(
            [NotNull] SqlServer2008Quoter quoter)
            : base(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        /// <inheritdoc />
        public SqlServer2016Generator(
            [NotNull] SqlServer2008Quoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(quoter, generatorOptions)
        {
        }

        /// <inheritdoc />
        protected SqlServer2016Generator(
            [NotNull] IColumn column,
            [NotNull] IQuoter quoter,
            [NotNull] IDescriptionGenerator descriptionGenerator,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, descriptionGenerator, generatorOptions)
        {
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.SqlServer2016;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases =>
            [GeneratorIdConstants.SqlServer2016, GeneratorIdConstants.SqlServer];
    }
}
