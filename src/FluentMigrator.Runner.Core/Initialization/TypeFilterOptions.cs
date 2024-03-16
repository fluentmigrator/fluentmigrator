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
    /// Type filtering options
    /// </summary>
    public class TypeFilterOptions
    {
        /// <summary>
        /// Gets or sets the root namespace for filtering
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether all sub-namespaces should be included
        /// </summary>
        public bool NestedNamespaces { get; set; }
    }
}
