using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;
using FluentMigrator.Runner.Processors.Firebird;

namespace FluentMigrator.Runner.Generators.Firebird
{

    public class FirebirdGenerator : GenericGenerator
    {
        protected readonly FirebirdTruncator truncator;
        protected Processors.Firebird.FirebirdOptions FBOptions { get; private set; }

        public FirebirdGenerator(Processors.Firebird.FirebirdOptions fbOptions) : base(new FirebirdColumn(fbOptions), new FirebirdQuoter(), new EmptyDescriptionGenerator()) 
        {
            if (fbOptions == null)
                throw new ArgumentNullException("fbOptions");

            FBOptions = fbOptions;
            truncator = new FirebirdTruncator(FBOptions.TruncateLongNames, FBOptions.PackKeyNames);
        }
        
        //It's kind of a hack to mess with system tables, but this is the cleanest and time-tested method to alter the nullable constraint.
        // It's even mentioned in the firebird official FAQ.
        // Currently the only drawback is that the integrity is not checked by the engine, you have to ensure it manually
        public string AlterColumnSetNullable { get { return "UPDATE RDB$RELATION_FIELDS SET RDB$NULL_FLAG = {0} WHERE lower(rdb$relation_name) = lower({1}) AND lower(RDB$FIELD_NAME) = lower({2})"; } }
        
        public string AlterColumnSetType { get { return "ALTER TABLE {0} ALTER COLUMN {1} TYPE {2}"; } }

        #region SQL Generation overrides

        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1}"; } }
        public override string DropColumn { get { return "ALTER TABLE {0} DROP {1}"; } }
        public override string RenameColumn { get { return "ALTER TABLE {0} ALTER COLUMN {1} TO {2}"; } }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            truncator.Truncate(expression);
            return String.Format("ALTER TABLE {0} ALTER COLUMN {1} SET DEFAULT {2}",
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName),
                Quoter.QuoteValue(expression.DefaultValue)
                );
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            truncator.Truncate(expression);
            return String.Format("ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT",
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName)
                );
        }
        
        public override string Generate(CreateIndexExpression expression)
        {
            //Firebird doesn't have particular asc or desc order per column, only per the whole index
            // CREATE [UNIQUE] [ASC[ENDING] | [DESC[ENDING]] INDEX indexname
            //  ON tablename  { (<col> [, <col> ...]) | COMPUTED BY (expression) }
            //  <col>  ::=  a column not of type ARRAY, BLOB or COMPUTED BY
            // 
            // Assuming the first column's direction for the index's direction.

            truncator.Truncate(expression);

            StringBuilder indexColumns = new StringBuilder("");
            Direction indexDirection = Direction.Ascending;
            int columnCount = expression.Index.Columns.Count;
            for (int i = 0; i < columnCount; i++)
            {
                IndexColumnDefinition columnDef = expression.Index.Columns.ElementAt(i);

                if (i > 0)
                    indexColumns.Append(", ");
                else indexDirection = columnDef.Direction;

                indexColumns.Append(Quoter.QuoteColumnName(columnDef.Name));

            }

            return String.Format(CreateIndex
                , GetUniqueString(expression)
                , indexDirection == Direction.Ascending ? "ASC " : "DESC "
                , Quoter.QuoteIndexName(expression.Index.Name)
                , Quoter.QuoteTableName(expression.Index.TableName)
                , indexColumns.ToString());
        }

        public override string Generate(AlterColumnExpression expression)
        {
            truncator.Truncate(expression);
            return compatabilityMode.HandleCompatabilty("Alter column is not supported as expected");
        }


        public override string Generate(CreateSequenceExpression expression)
        {
            truncator.Truncate(expression);
            return String.Format("CREATE SEQUENCE {0}", Quoter.QuoteSequenceName(expression.Sequence.Name));
        }
        
        public override string Generate(DeleteSequenceExpression expression)
        {
            truncator.Truncate(expression);
            return String.Format("DROP SEQUENCE {0}", Quoter.QuoteSequenceName(expression.SequenceName));
        }

        public string GenerateAlterSequence(SequenceDefinition sequence)
        {
            truncator.Truncate(sequence);
            if (sequence.StartWith != null)
                return String.Format("ALTER SEQUENCE {0} RESTART WITH {1}", Quoter.QuoteSequenceName(sequence.Name), sequence.StartWith.ToString());
            
            return String.Empty;
        }

        #endregion


        #region Name truncation overrides

        public override string Generate(CreateTableExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }
        
        public override string Generate(DeleteTableExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(RenameTableExpression expression)
        {
            truncator.Truncate(expression);
            return compatabilityMode.HandleCompatabilty("Rename table is not supported");
        }

        public override string Generate(CreateColumnExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(RenameColumnExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string GenerateForeignKeyName(CreateForeignKeyExpression expression)
        {
            truncator.Truncate(expression);
            return truncator.Truncate(base.GenerateForeignKeyName(expression));
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(InsertDataExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(UpdateDataExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }

        public override string Generate(DeleteDataExpression expression)
        {
            truncator.Truncate(expression);
            return base.Generate(expression);
        }
        
        #endregion

        
        #region Alter column generators
        
        public virtual string GenerateSetNull(ColumnDefinition column)
        {
            truncator.Truncate(column);
            return String.Format(AlterColumnSetNullable,
                !column.IsNullable.HasValue || !column.IsNullable.Value  ? "1" : "NULL",
                Quoter.QuoteValue(column.TableName),
                Quoter.QuoteValue(column.Name)
                );
        }

        public virtual string GenerateSetType(ColumnDefinition column)
        {
            truncator.Truncate(column);
            return String.Format(AlterColumnSetType,
                Quoter.QuoteTableName(column.TableName),
                Quoter.QuoteColumnName(column.Name),
                (Column as FirebirdColumn).GenerateForTypeAlter(column)
                );
        }

        #endregion


        #region Helpers
        
        public static bool ColumnTypesMatch(ColumnDefinition col1, ColumnDefinition col2)
        {
            FirebirdColumn column = new FirebirdColumn(new FirebirdOptions());
            string colDef1 = column.GenerateForTypeAlter(col1);
            string colDef2 = column.GenerateForTypeAlter(col2);
            return colDef1 == colDef2;
        }

        public static bool DefaultValuesMatch(ColumnDefinition col1, ColumnDefinition col2)
        {
            if (col1.DefaultValue is ColumnDefinition.UndefinedDefaultValue && col2.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
                return true;
            if (col1.DefaultValue is ColumnDefinition.UndefinedDefaultValue || col2.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
                return true;
            FirebirdColumn column = new FirebirdColumn(new FirebirdOptions());
            string col1Value = column.GenerateForDefaultAlter(col1);
            string col2Value = column.GenerateForDefaultAlter(col2);
            return col1Value != col2Value;
        }

        #endregion

    }
}
