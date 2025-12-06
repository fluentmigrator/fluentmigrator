#region License
// Copyright (c) 2024, Fluent Migrator Project
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

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.Snowflake;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;

using JetBrains.Annotations;

using NUnit.Framework;

namespace FluentMigrator.Tests.Integration.TestCases;

public class ProcessorTestCaseSource : IEnumerable
{
    private static readonly List<ProcessorTestCase> Processors =
    [
        new ProcessorTestCase<SqlServer2005Processor>(() => IntegrationTestOptions.SqlServer2005, "SqlServer", "SqlServer2005"),
        new ProcessorTestCase<SqlServer2008Processor>(() => IntegrationTestOptions.SqlServer2008, "SqlServer", "SqlServer2008"),
        new ProcessorTestCase<SqlServer2012Processor>(() => IntegrationTestOptions.SqlServer2012, "SqlServer", "SqlServer2012"),
        new ProcessorTestCase<SqlServer2014Processor>(() => IntegrationTestOptions.SqlServer2014, "SqlServer", "SqlServer2014"),
        new ProcessorTestCase<SqlServer2016Processor>(() => IntegrationTestOptions.SqlServer2016, "SqlServer", "SqlServer2016"),
        new ProcessorTestCase<SQLiteProcessor>(() => IntegrationTestOptions.SQLite, "SQLite"),
        new ProcessorTestCase<FirebirdProcessor>(() => IntegrationTestOptions.Firebird, "Firebird"),
        new ProcessorTestCase<PostgresProcessor>(() => IntegrationTestOptions.Postgres, "Postgres"),
        new ProcessorTestCase<MySql4Processor>(() => IntegrationTestOptions.MySql, "MySql"),
        new ProcessorTestCase<SnowflakeProcessor>(() => IntegrationTestOptions.Snowflake, "Snowflake"),
        new ProcessorTestCase<Oracle12CManagedProcessor>(() => IntegrationTestOptions.Oracle, "Oracle", "OracleManaged"),
    ];

    private Type[] ProcessorTypes { get; } = [];

    private bool Exclude { get; } = true;

    [UsedImplicitly]
    public ProcessorTestCaseSource()
    {
    }

    protected ProcessorTestCaseSource(bool exclude, params Type[] processorTypes)
    {
        Exclude = exclude;
        ProcessorTypes = processorTypes;
    }

    public IEnumerator GetEnumerator() => Processors
        .Where(p => Exclude ? !ProcessorTypes.Any(ignored => ignored.IsAssignableFrom(p.ProcessorType)) : ProcessorTypes.Any(ignored => ignored.IsAssignableFrom(p.ProcessorType)))
        .Select(p =>
        {
            var testCaseData = new TestCaseData(p.ProcessorType, p.ServerOptions).SetName(p.ProcessorType.Name);

            foreach (var category in p.Categories)
            {
                testCaseData.SetCategory(category);
            }

            return testCaseData;
        })
        .GetEnumerator();

    private class ProcessorTestCase
    {
        public Type ProcessorType { get; }
        public Func<IntegrationTestOptions.DatabaseServerOptions> ServerOptions { get; }
        public string[] Categories { get; }

        protected ProcessorTestCase(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions, params string[]? categories)
        {
            ProcessorType = processorType;
            ServerOptions = serverOptions;
            Categories = categories ?? [];
        }
    }

    private class ProcessorTestCase<TProcessor> : ProcessorTestCase
        where TProcessor : ProcessorBase
    {
        public ProcessorTestCase(
            Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions,
            params string[]? categories) : base(typeof(TProcessor), serverOptions, categories)
        {
        }
    }
}
