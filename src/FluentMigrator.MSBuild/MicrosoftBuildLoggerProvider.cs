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

using System;

using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace FluentMigrator.MSBuild
{
    public class MicrosoftBuildLoggerProvider : ILoggerProvider
    {
        private readonly ITask _task;

        public MicrosoftBuildLoggerProvider(ITask task)
        {
            _task = task ?? throw new ArgumentNullException(nameof(task));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return new MicrosoftBuildLogger(_task);
        }
    }
}
