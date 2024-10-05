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

using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="IMigrationRunnerBuilder"/>
    /// </summary>
    public static class SqlServerRunnerBuilderExtensions
    {
        /// <summary>
        /// Adds SQL Server support
        /// </summary>
        /// <remarks>
        /// This always selects the latest supported SQL server version.
        /// </remarks>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSqlServer(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddTransient<SqlServerBatchParser>();
            builder.Services.TryAddScoped<SqlServer2008Quoter>();
            builder.Services.TryAddScoped<ISqlServerTypeMap>(sp => new SqlServer2008TypeMap());
            builder.Services
                .AddScoped<SqlServer2016Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SqlServer2016Processor>())
                .AddScoped<SqlServer2016Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SqlServer2016Generator>());

            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2000 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSqlServer2000(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddTransient<SqlServerBatchParser>();
            builder.Services.TryAddScoped<SqlServer2000Quoter>();
            builder.Services
                .AddScoped<SqlServer2000Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SqlServer2000Processor>())
                .AddScoped<ISqlServerTypeMap>(sp => new SqlServer2000TypeMap())
                .AddScoped<SqlServer2000Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SqlServer2000Generator>());

            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2005 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSqlServer2005(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddTransient<SqlServerBatchParser>();
            builder.Services.TryAddScoped<SqlServer2005Quoter>();
            builder.Services
                .AddScoped<SqlServer2005Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SqlServer2005Processor>())
                .AddScoped<ISqlServerTypeMap>(sp => new SqlServer2005TypeMap())
                .AddScoped<SqlServer2005Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SqlServer2005Generator>());

            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2008 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSqlServer2008(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddTransient<SqlServerBatchParser>();
            builder.Services.TryAddScoped<SqlServer2008Quoter>();
            builder.Services.TryAddScoped<ISqlServerTypeMap>(sp => sp.GetRequiredService<SqlServer2008TypeMap>());
            builder.Services
                .AddScoped<SqlServer2008Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SqlServer2008Processor>())
                .AddScoped<SqlServer2008Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SqlServer2008Generator>());

            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2012 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSqlServer2012(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddTransient<SqlServerBatchParser>();
            builder.Services.TryAddScoped<SqlServer2008Quoter>();
            builder.Services
                .AddScoped<SqlServer2012Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SqlServer2012Processor>())
                .AddScoped<ISqlServerTypeMap>(sp => new SqlServer2008TypeMap())
                .AddScoped<SqlServer2012Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SqlServer2012Generator>());

            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2014 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSqlServer2014(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddTransient<SqlServerBatchParser>();
            builder.Services.TryAddScoped<SqlServer2008Quoter>();
            builder.Services
                .AddScoped<SqlServer2014Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SqlServer2014Processor>())
                .AddScoped<ISqlServerTypeMap>(sp => new SqlServer2008TypeMap())
                .AddScoped<SqlServer2014Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SqlServer2014Generator>());

            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2016 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddSqlServer2016(this IMigrationRunnerBuilder builder)
        {
            builder.Services.TryAddTransient<SqlServerBatchParser>();
            builder.Services.TryAddScoped<SqlServer2008Quoter>();
            builder.Services
                .AddScoped<SqlServer2016Processor>()
                .AddScoped<IMigrationProcessor>(sp => sp.GetRequiredService<SqlServer2016Processor>())
                .AddScoped<ISqlServerTypeMap>(sp => new SqlServer2008TypeMap())
                .AddScoped<SqlServer2016Generator>()
                .AddScoped<IMigrationGenerator>(sp => sp.GetRequiredService<SqlServer2016Generator>());

            return builder;
        }
    }
}
