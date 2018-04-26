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

using System;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Initialization.NetFramework
{
#pragma warning disable 1584,1711,1572,1581,1580
    /// <summary>
    /// Options for the <c>AppConfigConnectionStringReader</c>
    /// </summary>
    [Obsolete]
    public class AppConfigConnectionStringAccessorOptions
    {
        /// <summary>
        /// Gets or sets the path where the config file can be found
        /// </summary>
        [CanBeNull]
        public string ConnectionStringConfigPath { get; set; }

        /// <summary>
        /// Gets or sets the machine name
        /// </summary>
        [CanBeNull]
        public string MachineName { get; set; } = Environment.MachineName;
    }
#pragma warning restore 1584,1711,1572,1581,1580
}
