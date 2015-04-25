using System;
using FluentMigrator.Runner.Generators.PostgresBase;
using FluentMigrator.Expressions;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators.Postgres {
    public class PostgresGenerator : PostgresBaseGenerator 
    {
        public PostgresGenerator () : base(new PostgresColumn(), new PostgresQuoter(), new PostgresDescriptionGenerator())
        {
        }
        public override string Generate(CreateIndexExpression expression)
        {
            var result = new StringBuilder("CREATE");
            if (expression.Index.IsUnique)
                result.Append(" UNIQUE");

            result.Append(" INDEX {0} ON {1}.{2} (");

            var first = true;
            foreach (var column in expression.Index.Columns)
            {
                if (first)
                    first = false;
                else
                    result.Append(",");

                result.Append("\"" + column.Name + "\"");
                result.Append(column.Direction == Direction.Ascending ? " ASC" : " DESC");
            }
            result.Append(")");

            return String.Format(result.ToString(), Quoter.QuoteIndexName(expression.Index.Name), Quoter.QuoteSchemaName(expression.Index.SchemaName), Quoter.QuoteTableName(expression.Index.TableName));

            /*
            var idx = String.Format(result.ToString(), expression.Index.Name, Quoter.QuoteSchemaName(expression.Index.SchemaName), expression.Index.TableName); 
            if (!expression.Index.IsClustered)
                return idx;
              
             // Clustered indexes in Postgres do not cluster updates/inserts to the table after the initial cluster operation is applied.
             // To keep the clustered index up to date run CLUSTER TableName periodically
             
            return string.Format("{0}; CLUSTER {1}\"{2}\" ON \"{3}\"", idx, Quoter.QuoteSchemaName(expression.Index.SchemaName), expression.Index.TableName, expression.Index.Name);
             */
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            return string.Format("DROP INDEX {0}.{1}", Quoter.QuoteSchemaName(expression.Index.SchemaName), Quoter.QuoteIndexName(expression.Index.Name));
        }
    }
}

