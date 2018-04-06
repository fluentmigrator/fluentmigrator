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

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.BatchParser
{
    /// <summary>
    /// Represents the context for the <see cref="SearchStatus"/> operation
    /// </summary>
    internal sealed class SearchContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchContext"/> class.
        /// </summary>
        /// <param name="rangeSearchers">The range searchers</param>
        /// <param name="specialTokenSearchers">The special token searchers</param>
        /// <param name="stripComments">Should the comments be stripped</param>
        public SearchContext(
            [NotNull, ItemNotNull] IEnumerable<IRangeSearcher> rangeSearchers,
            [NotNull, ItemNotNull] IEnumerable<ISpecialTokenSearcher> specialTokenSearchers,
            bool stripComments)
        {
            StripComments = stripComments;
            SpecialTokenSearchers = specialTokenSearchers as IList<ISpecialTokenSearcher> ?? specialTokenSearchers.ToList().AsReadOnly();
            RangeSearchers = rangeSearchers as IList<IRangeSearcher> ?? rangeSearchers.ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets the special token searchers
        /// </summary>
        [NotNull, ItemNotNull]
        public IList<ISpecialTokenSearcher> SpecialTokenSearchers { get; }

        /// <summary>
        /// Gets the range searchers
        /// </summary>
        [NotNull, ItemNotNull]
        public IList<IRangeSearcher> RangeSearchers { get; }

        /// <summary>
        /// Gets a value indicating whether the comments should be stripped
        /// </summary>
        public bool StripComments { get; }

        /// <summary>
        /// Event handler that is called when SQL statements should be collected
        /// </summary>
        public event EventHandler<SqlBatchCollectorEventArgs> BatchSql;

        /// <summary>
        /// Event handler that is called when a special token was found
        /// </summary>
        public event EventHandler<SpecialTokenEventArgs> SpecialToken;

        internal void OnBatchSql([NotNull] SqlBatchCollectorEventArgs e)
        {
            BatchSql?.Invoke(this, e);
        }

        internal void OnSpecialToken([NotNull] SpecialTokenEventArgs e)
        {
            SpecialToken?.Invoke(this, e);
        }
    }
}
