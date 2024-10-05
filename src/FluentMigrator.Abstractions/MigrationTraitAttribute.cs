#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System;

namespace FluentMigrator
{
    /// <summary>
    /// A trait for a migration
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MigrationTraitAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationTraitAttribute"/> class.
        /// </summary>
        /// <param name="name"></param>
        public MigrationTraitAttribute(string name)
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationTraitAttribute"/> class.
        /// </summary>
        /// <param name="name">The trait name</param>
        /// <param name="value">The trait value</param>
        public MigrationTraitAttribute(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets the trait name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the trait value
        /// </summary>
        public object Value { get; }
    }
}
