#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.Runner;

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Tests.SyntaxRoots
{
    /// <summary>
    /// Keeps the SQL the processor logs (<see cref="RunnerEventIds.Sql"/>), one entry per statement as logged, and
    /// nothing else: no headers, timings or other messages.
    /// </summary>
    public sealed class SqlCaptureLoggerProvider : ILoggerProvider
    {
        private readonly List<string> _sql = new List<string>();

        public IReadOnlyList<string> Sql => _sql;

        public void Clear() => _sql.Clear();

        public ILogger CreateLogger(string categoryName) => new SqlCaptureLogger(_sql);

        public void Dispose()
        {
        }

        private sealed class SqlCaptureLogger : ILogger
        {
            private readonly List<string> _sql;

            public SqlCaptureLogger(List<string> sql)
            {
                _sql = sql;
            }

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (eventId.Id == RunnerEventIds.Sql.Id)
                {
                    _sql.Add(formatter(state, exception));
                }
            }
        }
    }
}
