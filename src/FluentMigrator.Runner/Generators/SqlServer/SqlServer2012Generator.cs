using System.Text;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServer2012Generator : SqlServer2008Generator
    {
        public SqlServer2012Generator()
            :base(new SqlServerColumn(new SqlServer2008TypeMap()))
        {
        }

        public override string Generate(Expressions.CreateSequenceExpression expression)
        {
            var result = new StringBuilder(string.Format("CREATE SEQUENCE "));
            var seq = expression.Sequence;
            result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(seq.SchemaName), Quoter.QuoteSequenceName(seq.Name));

            if (seq.Increment.HasValue)
            {
                result.AppendFormat(" INCREMENT BY {0}", seq.Increment);
            }

            if (seq.MinValue.HasValue)
            {
                result.AppendFormat(" MINVALUE {0}", seq.MinValue);
            }

            if (seq.MaxValue.HasValue)
            {
                result.AppendFormat(" MAXVALUE {0}", seq.MaxValue);
            }

            if (seq.StartWith.HasValue)
            {
                result.AppendFormat(" START WITH {0}", seq.StartWith);
            }

            if (seq.Cache.HasValue)
            {
                result.AppendFormat(" CACHE {0}", seq.Cache);
            }

            if (seq.Cycle)
            {
                result.Append(" CYCLE");
            }

            return result.ToString();
        }

        public override string Generate(Expressions.DeleteSequenceExpression expression)
        {
            var result = new StringBuilder(string.Format("DROP SEQUENCE "));
            result.AppendFormat("{0}.{1}", Quoter.QuoteSchemaName(expression.SchemaName), Quoter.QuoteSequenceName(expression.SequenceName));

            return result.ToString();
        }
    }
}