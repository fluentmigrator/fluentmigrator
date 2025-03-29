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

namespace FluentMigrator.Runner.Generators.MySql
{
    public class MySql5Generator : MySql4Generator
    {
        public MySql5Generator()
            : this(new MySqlQuoter())
        {
        }

        public MySql5Generator(
            [NotNull] MySqlQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public MySql5Generator(
            [NotNull] MySqlQuoter quoter,
            [NotNull] IMySqlTypeMap typeMap)
            : this(quoter, typeMap, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public MySql5Generator(
            [NotNull] MySqlQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : this(new MySqlColumn(new MySql5TypeMap(), quoter), quoter, new EmptyDescriptionGenerator(), generatorOptions)
        {
        }

        public MySql5Generator(
            [NotNull] MySqlQuoter quoter,
            [NotNull] IMySqlTypeMap typeMap,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new MySqlColumn(typeMap, quoter), quoter, new EmptyDescriptionGenerator(), generatorOptions)
        {
        }

        protected MySql5Generator(
            [NotNull] IColumn column,
            [NotNull] IQuoter quoter,
            [NotNull] IDescriptionGenerator descriptionGenerator,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, descriptionGenerator, generatorOptions)
        {
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.MySql5;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases =>
        [
            GeneratorIdConstants.MySql5, GeneratorIdConstants.MySql, GeneratorIdConstants.MariaDB
        ];
    }
}
