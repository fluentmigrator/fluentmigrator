#region License
// Copyright (c) 2018, FluentMigrator Project
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

using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;

using Microsoft.Extensions.DependencyInjection;

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
        public static IMigrationRunnerBuilder UseSqlServer(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<IMigrationProcessor, SqlServer2016Processor>()
                .AddScoped<IMigrationGenerator, SqlServer2016Generator>();
            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2000 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder UseSqlServer2000(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<IMigrationProcessor, SqlServer2000Processor>()
                .AddScoped<IMigrationGenerator, SqlServer2000Generator>();
            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2005 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder UseSqlServer2005(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<IMigrationProcessor, SqlServer2005Processor>()
                .AddScoped<IMigrationGenerator, SqlServer2005Generator>();
            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2008 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder UseSqlServer2008(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<IMigrationProcessor, SqlServer2008Processor>()
                .AddScoped<IMigrationGenerator, SqlServer2008Generator>();
            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2012 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder UseSqlServer2012(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<IMigrationProcessor, SqlServer2012Processor>()
                .AddScoped<IMigrationGenerator, SqlServer2012Generator>();
            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2014 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder UseSqlServer2014(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<IMigrationProcessor, SqlServer2014Processor>()
                .AddScoped<IMigrationGenerator, SqlServer2014Generator>();
            return builder;
        }

        /// <summary>
        /// Adds SQL Server 2016 support
        /// </summary>
        /// <param name="builder">The builder to add the SQL Server-specific services to</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder UseSqlServer2016(this IMigrationRunnerBuilder builder)
        {
            builder.Services
                .AddScoped<IMigrationProcessor, SqlServer2016Processor>()
                .AddScoped<IMigrationGenerator, SqlServer2016Generator>();
            return builder;
        }
    }
}
