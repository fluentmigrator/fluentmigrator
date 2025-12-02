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
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Runner.Generators;

using System.Data;
using System.Text;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.DB2;
using FluentMigrator.Runner.Generators.Firebird;
using FluentMigrator.Runner.Generators.Hana;
#if NETFRAMEWORK
using FluentMigrator.Runner.Generators.Jet;
#endif
using FluentMigrator.Runner.Generators.MySql;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.Generators.Redshift;
using FluentMigrator.Runner.Generators.Snowflake;
using FluentMigrator.Runner.Generators.SQLite;
using FluentMigrator.Runner.Generators.SqlServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentMigrator.Example.TypeMapper
{
    internal static class Program
    {
        static void Main()
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(lb => lb.AddDebug().AddFluentMigratorConsole())
                .AddFluentMigratorCore()
                .ConfigureRunner(
                    builder => builder
                        .AddAllDatabases())
                .BuildServiceProvider();

            var typeMaps = serviceProvider.GetServices<ISqlServerTypeMap>().Cast<ITypeMap>()
                .Union(serviceProvider.GetServices<IDb2TypeMap>())
                .Union(serviceProvider.GetServices<IFirebirdTypeMap>())
                .Union(serviceProvider.GetServices<IHanaTypeMap>())
#if NETFRAMEWORK
                .Union(serviceProvider.GetServices<IJetTypeMap>())
#endif
                .Union(serviceProvider.GetServices<IMySqlTypeMap>())
                .Union(serviceProvider.GetServices<IPostgresTypeMap>())
                .Union(serviceProvider.GetServices<IOracleTypeMap>())
                .Union(serviceProvider.GetServices<IRedshiftTypeMap>())
                .Union(serviceProvider.GetServices<ISnowflakeTypeMap>())
                .Union(serviceProvider.GetServices<ISQLiteTypeMap>()).ToList();

            var sb = new StringBuilder();
            sb.AppendLine(@"DbProvider,SqlTypeGeneratorExpression,DbType,Size,Precision");
            foreach (var typeMap in typeMaps)
            {
                var name = typeMap.GetType().FullName;

                var results = BuildTypeMapping(name, typeMap);

                AppendResultsToStringBuilder(results, sb);
            }

            var sqliteStrictTableTypeMap = new SQLiteTypeMap(true);
            var sqliteStrictTableResults = BuildTypeMapping(
                $"{sqliteStrictTableTypeMap.GetType().Name}(useStrictTables=true)", sqliteStrictTableTypeMap);

            AppendResultsToStringBuilder(sqliteStrictTableResults, sb);

            Console.WriteLine(sb);
        }

        private static void AppendResultsToStringBuilder(List<(string, string, DbType, int?, int?)> results, StringBuilder sb)
        {
            foreach (var result in results)
            {
                sb.AppendLine($@"{result.Item1},{result.Item2},DbType.{result.Item3},{result.Item4.ToString().Replace(int.MaxValue.ToString(), "int.MaxValue")},{result.Item5}");
            }
        }

        private static List<(string, string, DbType, int?, int?)> BuildTypeMapping(string name, ITypeMap typeMap1)
        {
            {
                return new List<(string, string, DbType, int?, int?)>
                {
                    (name, GetTypeMap(typeMap1, DbType.AnsiString, null, null), DbType.AnsiString, null, null),
                    (name, GetTypeMap(typeMap1, DbType.AnsiString, 8000, null), DbType.AnsiString, 8000, null),
                    (name, GetTypeMap(typeMap1, DbType.AnsiString, int.MaxValue, null), DbType.AnsiString, int.MaxValue, null),
                    (name, GetTypeMap(typeMap1, DbType.Binary, null, null), DbType.Binary, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Binary, 8000, null), DbType.Binary, 8000, null),
                    (name, GetTypeMap(typeMap1, DbType.Binary, int.MaxValue, null), DbType.Binary, int.MaxValue, null),
                    (name, GetTypeMap(typeMap1, DbType.Byte, null, null), DbType.Byte, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Boolean, null, null), DbType.Boolean, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Currency, null, null), DbType.Currency, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Date, null, null), DbType.Date, null, null),
                    (name, GetTypeMap(typeMap1, DbType.DateTime, null, null), DbType.DateTime, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Decimal, null, null), DbType.Decimal, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Decimal, int.MaxValue, int.MaxValue), DbType.Decimal, int.MaxValue, int.MaxValue),
                    (name, GetTypeMap(typeMap1, DbType.Double, null, null), DbType.Double, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Guid, null, null), DbType.Guid, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Int16, null, null), DbType.Int16, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Int32, null, null), DbType.Int32, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Int64, null, null), DbType.Int64, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Object, null, null), DbType.Object, null, null),
                    (name, GetTypeMap(typeMap1, DbType.SByte, null, null), DbType.SByte, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Single, null, null), DbType.Single, null, null),
                    (name, GetTypeMap(typeMap1, DbType.String, null, null), DbType.String, null, null),
                    (name, GetTypeMap(typeMap1, DbType.String, 8000, null), DbType.String, 8000, null),
                    (name, GetTypeMap(typeMap1, DbType.String, int.MaxValue, null), DbType.String, int.MaxValue, null),
                    (name, GetTypeMap(typeMap1, DbType.Time, null, null), DbType.Time, null, null),
                    (name, GetTypeMap(typeMap1, DbType.UInt16, null, null), DbType.UInt16, null, null),
                    (name, GetTypeMap(typeMap1, DbType.UInt32, null, null), DbType.UInt32, null, null),
                    (name, GetTypeMap(typeMap1, DbType.UInt64, null, null), DbType.UInt64, null, null),
                    (name, GetTypeMap(typeMap1, DbType.VarNumeric, null, null), DbType.VarNumeric, null, null),
                    (name, GetTypeMap(typeMap1, DbType.AnsiStringFixedLength, null, null), DbType.AnsiStringFixedLength, null, null),
                    (name, GetTypeMap(typeMap1, DbType.StringFixedLength, null, null), DbType.StringFixedLength, null, null),
                    (name, GetTypeMap(typeMap1, DbType.Xml, null, null), DbType.Xml, null, null),
                    (name, GetTypeMap(typeMap1, DbType.DateTime2, null, null), DbType.DateTime2, null, null),
                    (name, GetTypeMap(typeMap1, DbType.DateTimeOffset, null, null), DbType.DateTimeOffset, null, null),

                };
            }

            string GetTypeMap(ITypeMap typeMap, DbType dbType, int? size, int? precision)
            {
                try
                {
                    return typeMap.GetTypeMap(dbType, size, precision);
                }
                catch
                {
                    return "*** Unsupported";
                }
            }
        }
    }
}
