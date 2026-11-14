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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Builders.Execute
{
    /// <summary>
    /// The implementation of the <see cref="IExecuteExpressionRoot"/> interface.
    /// </summary>
    public class ExecuteExpressionRoot : IExecuteExpressionRoot, IMigrationContextAccessor
    {
        private const string ObsoleteParameterDictionaryMessage =
            "Use the IDictionary<string, object> parameter overload instead. The IDictionary<string, string> overload is kept for backwards compatibility and will be removed in a future major version.";

        private readonly IMigrationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteExpressionRoot"/> class.
        /// </summary>
        /// <param name="context">The migration context</param>
        public ExecuteExpressionRoot(IMigrationContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        IMigrationContext IMigrationContextAccessor.GetMigrationContext() => _context;

        /// <inheritdoc />
        public void Sql([StringSyntax("sql")] string sqlStatement)
        {
            var expression = new ExecuteSqlStatementExpression
            {
                SqlStatement = sqlStatement,
                SqlScriptTokenProviders = GetSqlScriptTokenProviders(),
            };
            _context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        public void Sql([StringSyntax("sql")] string sqlStatement, IDictionary<string, object> parameters)
        {
            var expression = new ExecuteSqlStatementExpression
            {
                SqlStatement = sqlStatement,
                Parameters = parameters,
                SqlScriptTokenProviders = GetSqlScriptTokenProviders(),
            };

            _context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        [Obsolete(ObsoleteParameterDictionaryMessage)]
        public void Sql([StringSyntax("sql")] string sqlStatement, IDictionary<string, string> parameters)
        {
            Sql(sqlStatement, ToObjectParameters(parameters));
        }

        /// <inheritdoc />
        public void Sql([StringSyntax("sql")] string sqlStatement, string description)
        {
            var expression = new ExecuteSqlStatementExpression
            {
                SqlStatement = sqlStatement,
                Description = description,
                SqlScriptTokenProviders = GetSqlScriptTokenProviders(),
            };

            _context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        public void Sql([StringSyntax("sql")] string sqlStatement, string description, IDictionary<string, object> parameters)
        {
            var expression = new ExecuteSqlStatementExpression
            {
                SqlStatement = sqlStatement,
                Description = description,
                Parameters = parameters,
                SqlScriptTokenProviders = GetSqlScriptTokenProviders(),
            };

            _context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        [Obsolete(ObsoleteParameterDictionaryMessage)]
        public void Sql([StringSyntax("sql")] string sqlStatement, string description, IDictionary<string, string> parameters)
        {
            Sql(sqlStatement, description, ToObjectParameters(parameters));
        }

        /// <inheritdoc />
        public void Script(string pathToSqlScript, IDictionary<string, object> parameters)
        {
            var expression = new ExecuteSqlScriptExpression
            {
                SqlScript = pathToSqlScript,
                Parameters = parameters,
                SqlScriptTokenProviders = GetSqlScriptTokenProviders(),
            };

            _context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        [Obsolete(ObsoleteParameterDictionaryMessage)]
        public void Script(string pathToSqlScript, IDictionary<string, string> parameters)
        {
            Script(pathToSqlScript, ToObjectParameters(parameters));
        }

        /// <inheritdoc />
        public void Script(string pathToSqlScript)
        {
            var expression = new ExecuteSqlScriptExpression
            {
                SqlScript = pathToSqlScript,
                SqlScriptTokenProviders = GetSqlScriptTokenProviders(),
            };
            _context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        public void WithConnection(Action<IDbConnection, IDbTransaction> operation)
        {
            var expression = new PerformDBOperationExpression { Operation = operation };
            _context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        public void WithConnection(Action<IDbConnection, IDbTransaction> operation, string description)
        {
            var expression = new PerformDBOperationExpression
            {
                Operation = operation,
                Description = description
            };
            _context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        public void EmbeddedScript(string embeddedSqlScriptName)
        {
            var embeddedResourceProviders = _context.ServiceProvider.GetService<IEnumerable<IEmbeddedResourceProvider>>();
            if (embeddedResourceProviders == null)
            {
                throw new InvalidOperationException(
                    $"The caller forgot to configure the service provider with at least one {nameof(IEmbeddedResourceProvider)}");
            }

            var expression = new ExecuteEmbeddedSqlScriptExpression(embeddedResourceProviders)
            {
                SqlScript = embeddedSqlScriptName,
                SqlScriptTokenProviders = GetSqlScriptTokenProviders(),
            };
            _context.Expressions.Add(expression);

        }

        /// <inheritdoc />
        public void EmbeddedScript(string embeddedSqlScriptName, IDictionary<string, object> parameters)
        {
            var embeddedResourceProviders = _context.ServiceProvider.GetService<IEnumerable<IEmbeddedResourceProvider>>();
            if (embeddedResourceProviders == null)
            {
                throw new InvalidOperationException(
                    $"The caller forgot to configure the service provider with at least one {nameof(IEmbeddedResourceProvider)}");
            }

            var expression = new ExecuteEmbeddedSqlScriptExpression(embeddedResourceProviders)
            {
                SqlScript = embeddedSqlScriptName,
                Parameters = parameters,
                SqlScriptTokenProviders = GetSqlScriptTokenProviders(),
            };

            _context.Expressions.Add(expression);
        }

        /// <inheritdoc />
        [Obsolete(ObsoleteParameterDictionaryMessage)]
        public void EmbeddedScript(string embeddedSqlScriptName, IDictionary<string, string> parameters)
        {
            EmbeddedScript(embeddedSqlScriptName, ToObjectParameters(parameters));
        }

        /// <summary>
        /// Converts a legacy <see cref="IDictionary{TKey,TValue}"/> of <see cref="string"/> values
        /// into the <see cref="IDictionary{TKey,TValue}"/> of <see cref="object"/> values expected by
        /// the current parameter overloads.
        /// </summary>
        /// <param name="parameters">The legacy string-valued parameters, or <c>null</c></param>
        /// <returns>An object-valued copy of <paramref name="parameters"/>, or <c>null</c> when
        /// <paramref name="parameters"/> is <c>null</c></returns>
        /// <remarks>
        /// The key comparer of <paramref name="parameters"/> is carried over for the built-in
        /// dictionary implementations that expose an <see cref="IEqualityComparer{T}"/>, so
        /// that callers passing e.g. a <see cref="StringComparer.OrdinalIgnoreCase"/> dictionary
        /// keep the token lookup semantics they had before the conversion.
        /// </remarks>
        private static IDictionary<string, object> ToObjectParameters(IDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return null;
            }

            var comparer = GetKeyComparer(parameters);
            var converted = new Dictionary<string, object>(parameters.Count, comparer);
            foreach (var parameter in parameters)
            {
                converted[parameter.Key] = parameter.Value;
            }

            return converted;
        }

        private static IEqualityComparer<string> GetKeyComparer(IDictionary<string, string> parameters)
        {
            return parameters switch
            {
                Dictionary<string, string> dictionary => dictionary.Comparer,
                SortedDictionary<string, string> dictionary => dictionary.Comparer as IEqualityComparer<string>,
                _ => null,
            };
        }

        /// <summary>
        /// Resolves the registered <see cref="ISqlTokenProvider"/> instances from the
        /// current migration context's service provider
        /// </summary>
        /// <returns>The registered SQL script token providers, or <c>null</c> when none are registered</returns>
        private IEnumerable<ISqlTokenProvider> GetSqlScriptTokenProviders()
        {
            return _context.ServiceProvider.GetService<IEnumerable<ISqlTokenProvider>>();
        }
    }
}
