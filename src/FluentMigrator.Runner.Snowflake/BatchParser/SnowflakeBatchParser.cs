using System.Collections.Generic;

using FluentMigrator.Runner.BatchParser.RangeSearchers;
using FluentMigrator.Runner.BatchParser.SpecialTokenSearchers;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.BatchParser
{
    /// <summary>
    /// Represents a specialized SQL batch parser tailored for Snowflake SQL syntax.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="SqlBatchParser"/> to provide support for parsing Snowflake-specific SQL batches.
    /// It utilizes predefined range searchers and special token searchers to identify and process SQL statements
    /// and tokens specific to Snowflake.
    /// </remarks>
    public class SnowflakeBatchParser : SqlBatchParser
    {
        [NotNull, ItemNotNull]
        private static readonly IEnumerable<IRangeSearcher> _rangeSearchers = new IRangeSearcher[]
        {
            new MultiLineComment(),
            new DoubleDashSingleLineComment(),
            new PoundSignSingleLineComment(),
            new AnsiSqlIdentifier(),
            new SqlString(),
        };

        [NotNull, ItemNotNull]
        private static readonly IEnumerable<ISpecialTokenSearcher> _specialTokenSearchers = new ISpecialTokenSearcher[]
        {
            new SemicolonSearcher()
        };

        /// <inheritdoc />
        public SnowflakeBatchParser() : this(null) { }

        /// <inheritdoc />
        public SnowflakeBatchParser(string newLine) : base(_rangeSearchers, _specialTokenSearchers, newLine) { }
    }
}
