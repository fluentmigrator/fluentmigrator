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

using System.Collections.Generic;

using FluentMigrator.Generation;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Processors.Firebird
{
    /// <summary>
    /// Tracks virtual locks caused by DDL statements
    /// </summary>
    public class FirebirdDDLLockTracker
    {
        private readonly ISet<string> _ddlTables;
        private readonly IDictionary<string, ISet<string>> _ddlColumns;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebirdDDLLockTracker"/> class.
        /// </summary>
        /// <param name="quoter">The Firebird quoter</param>
        public FirebirdDDLLockTracker(IQuoter quoter)
        {
            var comparer = new FirebirdIdentifierComparer(quoter);
            _ddlTables = new HashSet<string>(comparer);
            _ddlColumns = new Dictionary<string, ISet<string>>(comparer);
        }

        /// <summary>
        /// Clears all virtual locks
        /// </summary>
        public void Clear()
        {
            _ddlTables.Clear();
            _ddlColumns.Clear();
        }

        /// <summary>
        /// Registers a table as locked
        /// </summary>
        /// <param name="quotedTableName">The quoted table name</param>
        /// <returns><c>true</c> when the table wasn't locked</returns>
        public bool TryRegisterTable([NotNull] string quotedTableName)
        {
            return _ddlTables.Add(quotedTableName);
        }

        /// <summary>
        /// Returns a value indicating whether the table was locked
        /// </summary>
        /// <param name="quotedTableName">The quoted table name</param>
        /// <returns><c>true</c> when the table is locked</returns>
        public bool IsTableRegistered([NotNull] string quotedTableName)
        {
            return _ddlTables.Contains(quotedTableName);
        }

        /// <summary>
        /// Registers a column as locked
        /// </summary>
        /// <param name="quotedTableName">The quoted table name</param>
        /// <param name="quotedColumnName">The quoted column name</param>
        /// <returns><c>true</c> when the column wasn't locked</returns>
        public bool TryRegisterColumn([NotNull] string quotedTableName, [NotNull] string quotedColumnName)
        {
            if (_ddlColumns.TryGetValue(quotedTableName, out var columns))
            {
                return columns.Add(quotedColumnName);
            }

            _ddlColumns.Add(quotedTableName, new HashSet<string>() { quotedColumnName });
            return true;
        }

        /// <summary>
        /// Returns a value indicating whether the column was locked
        /// </summary>
        /// <param name="quotedTableName">The quoted table name</param>
        /// <param name="quotedColumnName">The quoted column name</param>
        /// <returns><c>true</c> when the column is locked</returns>
        public bool IsColumnRegistered([NotNull] string quotedTableName, [NotNull] string quotedColumnName)
        {
            return _ddlColumns.TryGetValue(quotedTableName, out var columns) && columns.Contains(quotedColumnName);
        }
    }
}
