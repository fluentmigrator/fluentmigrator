#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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

namespace FluentMigrator.Runner.Conventions
{
    /// <summary>
    /// The default implementation of the <see cref="IDefaultSchemaNameConvention"/>
    /// </summary>
    public class DefaultSchemaNameConvention : IDefaultSchemaNameConvention
    {
        private readonly string _defaultSchemaName;
        private readonly bool _isActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="IDefaultSchemaNameConvention"/> class.
        /// </summary>
        /// <param name="defaultSchemaName">The default schema name (can be null or empty)</param>
        public DefaultSchemaNameConvention(string defaultSchemaName)
        {
            _defaultSchemaName = defaultSchemaName;
            _isActive = !string.IsNullOrEmpty(defaultSchemaName);
        }

        /// <inheritdoc />
        public string GetSchemaName(string originalSchemaName)
        {
            if (!_isActive)
                return originalSchemaName;
            if (!string.IsNullOrEmpty(originalSchemaName))
                return originalSchemaName;
            return _defaultSchemaName;
        }
    }
}
