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

using FluentMigrator.Runner.Logging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Extension methods for <see cref="ILoggerFactory"/> and <see cref="ILoggingBuilder"/>
    /// </summary>
    public static class FluentMigratorLoggingExtensions
    {
        /// <summary>
        /// Adds the default FluentMigrator console logger
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        /// <returns>The same logger factory</returns>
        public static ILoggerFactory AddFluentMigratorConsole(this ILoggerFactory loggerFactory)
        {
            return loggerFactory.AddFluentMigratorConsole(new FluentMigratorLoggerOptions());
        }

        /// <summary>
        /// Adds the default FluentMigrator console logger
        /// </summary>
        /// <param name="loggerFactory">The logger factory</param>
        /// <param name="options">The logger options</param>
        /// <returns>The same logger factory</returns>
        public static ILoggerFactory AddFluentMigratorConsole(
            this ILoggerFactory loggerFactory,
            FluentMigratorLoggerOptions options)
        {
            loggerFactory.AddProvider(
                new FluentMigratorConsoleLoggerProvider(
                    new OptionsWrapper<FluentMigratorLoggerOptions>(options)));
            return loggerFactory;
        }

        /// <summary>
        /// Adds the default FluentMigrator console logger
        /// </summary>
        /// <param name="loggingBuilder">The logging builder</param>
        /// <returns>The same logging builder</returns>
        public static ILoggingBuilder AddFluentMigratorConsole(this ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.Services.AddSingleton<ILoggerProvider, FluentMigratorConsoleLoggerProvider>();
            return loggingBuilder;
        }
    }
}
