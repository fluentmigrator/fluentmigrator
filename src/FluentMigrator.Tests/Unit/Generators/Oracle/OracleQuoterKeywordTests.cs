using System;
using FluentMigrator.Runner.Generators.Oracle;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Generators.Oracle
{
    [TestFixture]
    public class OracleQuoterKeywordTests
    {
        private static readonly string[] KeywordProbeSource =
        {
            "ACCESS", "BACKUP", "CACHE", "DECIMAL", "EXTERNALLY", "FAILED_LOGIN_ATTEMPTS", "INITIAL", "PRIVILEGE", "SQL_TRACE", "UB2", "ZONE"
        };

        private OracleQuoter Quoter { get; set; }

        [SetUp]
        public void Setup()
        {
            Quoter = new OracleQuoter();
        }

        [Test, TestCaseSource("KeywordProbeSource")]
        public void when_quoting_column_names_it_quotes_keyword(string keyword)
        {
            var quoted = Quoter.QuoteColumnName(keyword);

            var expected = String.Format("\"{0}\"", keyword);

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_column_names_it_quotes_keyword_even_if_casing_differs()
        {
            var quoted = Quoter.QuoteColumnName("accouNt");

            var expected = String.Format("\"{0}\"", "accouNt");

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_column_names_it_does_not_quote_unknown_name()
        {
            var quoted = Quoter.QuoteColumnName("ACTOR");

            quoted.ShouldBe("ACTOR");
        }

        [Test, TestCaseSource("KeywordProbeSource")]
        public void when_quoting_constraint_names_it_quotes_keyword(string keyword)
        {
            var quoted = Quoter.QuoteConstraintName(keyword);

            var expected = String.Format("\"{0}\"", keyword);

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_constraint_names_it_quotes_keyword_even_if_casing_differs()
        {
            var quoted = Quoter.QuoteConstraintName("accouNt");

            var expected = String.Format("\"{0}\"", "accouNt");

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_constraint_names_it_does_not_quote_unknown_name()
        {
            var quoted = Quoter.QuoteConstraintName("ACTOR");

            quoted.ShouldBe("ACTOR");
        }

        [Test, TestCaseSource("KeywordProbeSource")]
        public void when_quoting_index_names_it_quotes_keyword(string keyword)
        {
            var quoted = Quoter.QuoteIndexName(keyword);

            var expected = String.Format("\"{0}\"", keyword);

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_index_names_it_quotes_keyword_even_if_casing_differs()
        {
            var quoted = Quoter.QuoteIndexName("accouNt");

            var expected = String.Format("\"{0}\"", "accouNt");

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_index_names_it_does_not_quote_unknown_name()
        {
            var quoted = Quoter.QuoteIndexName("ACTOR");

            quoted.ShouldBe("ACTOR");
        }

        [Test, TestCaseSource("KeywordProbeSource")]
        public void when_quoting_schema_names_it_quotes_keyword(string keyword)
        {
            var quoted = Quoter.QuoteSchemaName(keyword);

            var expected = String.Format("\"{0}\"", keyword);

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_schema_names_it_quotes_keyword_even_if_casing_differs()
        {
            var quoted = Quoter.QuoteSchemaName("accouNt");

            var expected = String.Format("\"{0}\"", "accouNt");

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_schema_names_it_does_not_quote_unknown_name()
        {
            var quoted = Quoter.QuoteSchemaName("ACTOR");

            quoted.ShouldBe("ACTOR");
        }

        [Test, TestCaseSource("KeywordProbeSource")]
        public void when_quoting_table_names_it_quotes_keyword(string keyword)
        {
            var quoted = Quoter.QuoteTableName(keyword);

            var expected = String.Format("\"{0}\"", keyword);

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_table_names_it_quotes_keyword_even_if_casing_differs()
        {
            var quoted = Quoter.QuoteTableName("accouNt");

            var expected = String.Format("\"{0}\"", "accouNt");

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_table_names_it_does_not_quote_unknown_name()
        {
            var quoted = Quoter.QuoteTableName("ACTOR");

            quoted.ShouldBe("ACTOR");
        }

        [Test, TestCaseSource("KeywordProbeSource")]
        public void when_quoting_sequence_names_it_quotes_keyword(string keyword)
        {
            var quoted = Quoter.QuoteSequenceName(keyword);

            var expected = String.Format("\"{0}\"", keyword);

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_sequence_names_it_quotes_keyword_even_if_casing_differs()
        {
            var quoted = Quoter.QuoteSequenceName("accouNt");

            var expected = String.Format("\"{0}\"", "accouNt");

            quoted.ShouldBe(expected);
        }

        [Test]
        public void when_quoting_sequence_names_it_does_not_quote_unknown_name()
        {
            var quoted = Quoter.QuoteSequenceName("ACTOR");

            quoted.ShouldBe("ACTOR");
        }
    }
}