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
using System.Diagnostics.CodeAnalysis;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// The base class for SQL script execution
    /// </summary>
    public abstract class ExecuteSqlScriptExpressionBase : MigrationExpressionBase
    {
        /// <summary>
        /// Gets or sets parameters to be replaced before script execution
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the token providers used to resolve additional tokens
        /// (e.g. <c>DefaultSchema</c>) that can be referenced from the SQL script/statement.
        /// </summary>
        /// <remarks>
        /// Tokens are merged in registration order and are overridden by any
        /// entry with the same name in <see cref="Parameters"/>.
        /// </remarks>
        public IEnumerable<ISqlTokenProvider> SqlScriptTokenProviders { get; set; }

        /// <summary>
        /// Executes the <paramref name="sqlScript"/> with the given <paramref name="processor"/>
        /// </summary>
        /// <param name="processor">The processor to execute the script with</param>
        /// <param name="sqlScript">The SQL script to execute</param>
        protected void Execute(IMigrationProcessor processor, [StringSyntax("sql")] string sqlScript)
        {
            var finalSqlScript = SqlScriptTokenReplacer.ReplaceSqlScriptTokens(sqlScript, GetMergedParameters(), processor.Quoter);
            processor.Execute(finalSqlScript);
        }

        /// <summary>
        /// Merges the tokens supplied by <see cref="SqlScriptTokenProviders"/> with
        /// the user-supplied <see cref="Parameters"/>, giving precedence to <see cref="Parameters"/>
        /// whenever a token name is defined in both.
        /// </summary>
        /// <returns>The merged token map, or <see cref="Parameters"/> unchanged when there are no
        /// tokens to merge</returns>
        /// <remarks>
        /// The merged map reuses the key comparer of built-in dictionary implementations that
        /// expose an <see cref="IEqualityComparer{T}"/>, so that merging in provider tokens does
        /// not silently change token lookup semantics for callers who supplied e.g. a
        /// <see cref="System.StringComparer.OrdinalIgnoreCase"/> dictionary.
        /// </remarks>
        protected IDictionary<string, object> GetMergedParameters()
        {
            if (SqlScriptTokenProviders == null)
            {
                return Parameters;
            }

            var comparer = GetKeyComparer(Parameters);
            var mergedParameters = new Dictionary<string, object>(comparer);
            foreach (var provider in SqlScriptTokenProviders)
            {
                var tokenMap = provider?.GetTokens();
                if (tokenMap == null)
                {
                    continue;
                }

                foreach (var token in tokenMap)
                {
                    mergedParameters[token.Key] = token.Value;
                }
            }

            if (mergedParameters.Count == 0)
            {
                return Parameters;
            }

            if (Parameters != null)
            {
                foreach (var parameter in Parameters)
                {
                    mergedParameters[parameter.Key] = parameter.Value;
                }
            }

            return mergedParameters;
        }

        private static IEqualityComparer<string> GetKeyComparer(IDictionary<string, object> parameters)
        {
            return parameters switch
            {
                Dictionary<string, object> dictionary => dictionary.Comparer,
                SortedDictionary<string, object> dictionary => dictionary.Comparer as IEqualityComparer<string>,
                _ => null,
            };
        }
    }
}
