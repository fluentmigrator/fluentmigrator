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
using System.Collections.Generic;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// The expression requires an automatically generated name
    /// </summary>
    public interface IAutoNameExpression
    {
        /// <summary>
        /// Gets or sets the automatically generated names
        /// </summary>
        IList<string> AutoNames { get; set; }

        /// <summary>
        /// Gets or sets the context in which the automatically generated name gets used
        /// </summary>
        AutoNameContext AutoNameContext { get; }

        /// <summary>
        /// Gets the type of the migration object
        /// </summary>
        Type MigrationType { get; }

        /// <summary>
        /// Gets the database names
        /// </summary>
        IList<string> DatabaseNames { get; }

        /// <summary>
        /// Gets the direction of the migration
        /// </summary>
        MigrationDirection Direction { get; }
    }
}
