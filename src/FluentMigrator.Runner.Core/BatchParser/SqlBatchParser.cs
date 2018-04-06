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
using System.IO;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.BatchParser
{
    /// <summary>
    /// This is the base implementation of the SQL batch parser
    /// </summary>
    public class SqlBatchParser
    {
        [NotNull]
        [ItemNotNull]
        private readonly IEnumerable<IRangeSearcher> _rangeSearchers;

        [NotNull]
        [ItemNotNull]
        private readonly IEnumerable<ISpecialTokenSearcher> _specialTokenSearchers;

        [NotNull]
        private readonly string _newLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlBatchParser"/> class.
        /// </summary>
        /// <param name="rangeSearchers">The range searchers to be used</param>
        /// <param name="specialTokenSearchers">The special token searchers to be used</param>
        /// <param name="newLine">The new line sequence to be used for the output</param>
        public SqlBatchParser(
            [NotNull, ItemNotNull] IEnumerable<IRangeSearcher> rangeSearchers,
            [NotNull, ItemNotNull] IEnumerable<ISpecialTokenSearcher> specialTokenSearchers,
            string newLine = null)
        {
            _rangeSearchers = rangeSearchers;
            _specialTokenSearchers = specialTokenSearchers;
            _newLine = newLine ?? Environment.NewLine;
        }

        /// <summary>
        /// An event handler that is called when a special token was found
        /// </summary>
        public event EventHandler<SpecialTokenEventArgs> SpecialToken;

        /// <summary>
        /// An event handler that is called when an SQL text was collected and is considered complete
        /// </summary>
        public event EventHandler<SqlTextEventArgs> SqlText;

        /// <summary>
        /// Process the <paramref name="source"/>
        /// </summary>
        /// <param name="source">The source to process/parse for SQL statement batches</param>
        /// <param name="stripComments"><c>true</c> when the comments should be stripped</param>
        public void Process(ITextSource source, bool stripComments = false)
        {
            var output = new StringWriter()
            {
                NewLine = _newLine,
            };

            var context = new SearchContext(_rangeSearchers, _specialTokenSearchers, stripComments);
            context.BatchSql += (sender, evt) =>
            {
                output.Write(evt.SqlContent);
                if (evt.IsEndOfLine)
                    output.WriteLine();
            };

            context.SpecialToken += (sender, evt) =>
            {
                output.Flush();
                var sqlText = output.ToString();
                OnSqlText(new SqlTextEventArgs(sqlText));
                OnSpecialToken(evt);
                output = new StringWriter();
            };

            var reader = source.CreateReader();
            if (reader == null)
                return;

            var status = new SearchStatus(context, reader);
            do
            {
                status = status.Process();
            }
            while (status != null);

            var remaining = output.ToString();
            if (!string.IsNullOrWhiteSpace(remaining))
            {
                OnSqlText(new SqlTextEventArgs(remaining));
            }
        }

        /// <summary>
        /// Invokes the <see cref="SpecialToken"/> event
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnSpecialToken(SpecialTokenEventArgs e)
        {
            SpecialToken?.Invoke(this, e);
        }

        /// <summary>
        /// Invokes the <see cref="SqlText"/> event
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected virtual void OnSqlText(SqlTextEventArgs e)
        {
            SqlText?.Invoke(this, e);
        }
    }
}
