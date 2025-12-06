#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.DB2.iSeries
{
    /// <summary>
    /// Represents a generator for creating database migration scripts specifically for IBM Db2 for iSeries.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="FluentMigrator.Runner.Generators.DB2.Db2Generator"/> to provide
    /// functionality tailored to the IBM Db2 for iSeries database platform. It includes specific quoting
    /// and generator options to ensure compatibility with the iSeries environment.
    /// </remarks>
    public class Db2ISeriesGenerator : Db2Generator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Db2ISeriesGenerator"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the generator with default options and a default
        /// <see cref="Db2ISeriesQuoter"/> for quoting database objects specific to IBM Db2 for iSeries.
        /// </remarks>
        public Db2ISeriesGenerator()
            : this(new Db2ISeriesQuoter())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Db2ISeriesGenerator"/> class with a specified quoter.
        /// </summary>
        /// <param name="quoter">
        /// An instance of <see cref="Db2ISeriesQuoter"/> used for quoting database objects specific to IBM Db2 for iSeries.
        /// </param>
        /// <remarks>
        /// This constructor allows customization of the quoter used for generating database migration scripts,
        /// ensuring compatibility with the IBM Db2 for iSeries platform.
        /// </remarks>
        public Db2ISeriesGenerator(
            Db2ISeriesQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Db2ISeriesGenerator"/> class with a specified quoter and generator options.
        /// </summary>
        /// <param name="quoter">
        /// An instance of <see cref="Db2ISeriesQuoter"/> used for quoting database objects specific to IBM Db2 for iSeries.
        /// </param>
        /// <param name="generatorOptions">
        /// An instance of <see cref="IOptions{GeneratorOptions}"/> that provides configuration options for the generator.
        /// </param>
        /// <remarks>
        /// This constructor allows customization of both the quoter and the generator options used for creating database migration scripts,
        /// ensuring compatibility with the IBM Db2 for iSeries platform.
        /// </remarks>
        public Db2ISeriesGenerator(
            Db2ISeriesQuoter quoter,
            IOptions<GeneratorOptions> generatorOptions)
            : base(quoter, generatorOptions)
        {
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.Db2ISeries;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.Db2ISeries, GeneratorIdConstants.DB2 };
    }
}
