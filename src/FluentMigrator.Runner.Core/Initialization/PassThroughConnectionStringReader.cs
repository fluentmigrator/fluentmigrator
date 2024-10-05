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

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// A connection string provider that just passes through the given connection string
    /// </summary>
    public class PassThroughConnectionStringReader : IConnectionStringReader
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassThroughConnectionStringReader"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        public PassThroughConnectionStringReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <inheritdoc />
        public int Priority { get; } = 200;

        /// <inheritdoc />
        public string GetConnectionString(string connectionStringName)
        {
            return _connectionString;
        }
    }
}
