#region License
// Copyright (c) 2019, FluentMigrator Project
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

using System;

using FluentMigrator.IntegrationTests.Migrations;
using FluentMigrator.IntegrationTests.Settings;
using FluentMigrator.Runner;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FluentMigrator.IntegrationTests.Fixtures.MySql5
{
    public class Startup
    {
        public Startup(IHostEnvironment env)
        {
            // ReSharper disable once LocalizableElement
            Console.WriteLine($"Running on {env.EnvironmentName.ToLower()} environment.");

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName.ToLower()}.json", true, true);

            Configuration = builder.Build();
        }
        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add support for Options
            services.AddOptions();

            // Set IConfiguration in DI
            services.AddSingleton(Configuration);

            // Database configuration
            services.Configure<FluentMigratorDatabase>(Configuration.GetSection("Database"));

            // Setup Fluent Migrator Core
            services.AddFluentMigratorCore()
                .ConfigureRunner(
                    r => r
                        .AddMySql5()
                        .WithGlobalConnectionString(
                            sp =>
                            {
                                var opt = sp.GetRequiredService<IOptions<FluentMigratorDatabase>>();
                                return opt.Value.MySql56ConnectionString;
                            })
                        .ScanIn(typeof(TestCreateAndDropTableMigration).Assembly).For.Migrations())
                // .AddLogging(l => l.AddFluentMigratorConsole())
                .BuildServiceProvider(validateScopes: false);
        }

        public void Configure()
        {

        }
    }
}
