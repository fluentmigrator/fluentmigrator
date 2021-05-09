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

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Conventions of how C# values are converted to SQL
    /// </summary>
    public sealed class QuoterOptions
    {
        /// <summary>
        /// Gets or sets the value indicating how enum values are handled.<br/>
        /// If <see langword="true"/> enums are converted to a string, if <see langword="false"/> enums are converted to the underlying numeric type.<br/>
        /// Default value: <see langword="true"/>.
        /// </summary>
        public bool EnumAsString { get; set; } = true;
    }
}
