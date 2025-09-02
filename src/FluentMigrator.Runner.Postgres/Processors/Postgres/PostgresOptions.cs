#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Text.RegularExpressions;

namespace FluentMigrator.Runner.Processors.Postgres
{
    /// <summary>
    /// Represents configuration options for the PostgreSQL database processor.
    /// </summary>
    /// <remarks>
    /// This class provides settings that influence the behavior of PostgreSQL database processing,
    /// such as whether to force quoting of identifiers. It also includes functionality to parse
    /// provider-specific switches into configuration options.
    /// </remarks>
    public class PostgresOptions : ICloneable
    {
        /// <summary>
        /// Gets or sets a value indicating whether all names should be quoted unconditionally.
        /// </summary>
        public bool ForceQuote { get; set; } = true;

        /// <summary>
        /// Parses provider-specific switches into a <see cref="PostgresOptions"/> instance.
        /// </summary>
        /// <param name="providerSwitches">
        /// A string containing key-value pairs of provider-specific options, separated by spaces.
        /// Each key-value pair should be in the format <c>key=value</c>.
        /// </param>
        /// <returns>
        /// A new instance of <see cref="PostgresOptions"/> configured based on the parsed switches.
        /// </returns>
        /// <remarks>
        /// This method processes the provided switches to configure options such as 
        /// <see cref="PostgresOptions.ForceQuote"/>. If a switch is not recognized or cannot be parsed, 
        /// it is ignored.
        /// </remarks>
        public static PostgresOptions ParseProviderSwitches(string providerSwitches)
        {
            var retval = new PostgresOptions();

            var switchesParsed = Regex.Matches(providerSwitches ?? string.Empty, @"(?<key>[^=]+)=(?<value>[^\s]+)");
            foreach (Match match in switchesParsed)
            {
                if (!match.Success)
                {
                    continue;
                }

                var key = match.Groups["key"].Value;
                var value = match.Groups["value"].Value;

                if ("Force Quote".Equals(key, StringComparison.OrdinalIgnoreCase) && bool.TryParse(value, out var forceQuoteParsed))
                {
                    retval.ForceQuote = forceQuoteParsed;
                }
            }

            return retval;
        }

        /// <inheritdoc />
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
