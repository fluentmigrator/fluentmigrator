using System.Collections.Generic;

using FluentMigrator.Runner.BatchParser.RangeSearchers;
using FluentMigrator.Runner.BatchParser.SpecialTokenSearchers;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.BatchParser
{
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
