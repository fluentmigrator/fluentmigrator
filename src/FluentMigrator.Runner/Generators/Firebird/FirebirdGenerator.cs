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
        protected Processors.Firebird.FirebirdOptions FBOptions { get; private set; }

        public FirebirdGenerator(Processors.Firebird.FirebirdOptions fbOptions) : base(new FirebirdColumn(fbOptions), new FirebirdQuoter()) 
        {
            if (fbOptions == null)
                throw new ArgumentNullException("fbOptions");

            FBOptions = fbOptions;
        }
        
        //It's kind of a hack to mess with system tables, but this is the cleanest and time-tested method to alter the nullable constraint.
        // It's even mentioned in the firebird official FAQ.
        // Currently the only drawback is that the integrity is not checked by the engine, you have to ensure it manually
        public string AlterColumnSetNullable { get { return "UPDATE RDB$RELATION_FIELDS SET RDB$NULL_FLAG = {0} WHERE RDB$RELATION_NAME = {1} AND RDB$FIELD_NAME = {2}"; } }
        
        public string AlterColumnSetType { get { return "ALTER TABLE {0} ALTER COLUMN {1} TYPE {2}"; } }

        #region SQL Generation overrides

        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1}"; } }
        public override string DropColumn { get { return "ALTER TABLE {0} DROP {1}"; } }
        public override string RenameColumn { get { return "ALTER TABLE {0} ALTER COLUMN {1} TO {2}"; } }

        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            expression.TableName = TruncateToMaxLength(expression.TableName);
            expression.ColumnName = TruncateToMaxLength(expression.ColumnName);
            return String.Format("ALTER TABLE {0} ALTER COLUMN {1} SET DEFAULT {2}",
                Quoter.QuoteTableName(expression.TableName),
                Quoter.QuoteColumnName(expression.ColumnName),
                Quoter.QuoteValue(expression.DefaultValue)
                );
        }

        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            expression.TableName = TruncateToMaxLength(expression.TableName);
            expression.ColumnName = TruncateToMaxLength(expression.ColumnName);
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

            expression.Index.Name = TruncateToMaxLength(expression.Index.Name);
            expression.Index.TableName = TruncateToMaxLength(expression.Index.TableName);

            StringBuilder indexColumns = new StringBuilder("");
            Direction indexDirection = Direction.Ascending;
            int columnCount = expression.Index.Columns.Count;
            for (int i = 0; i < columnCount; i++)
            {
                IndexColumnDefinition columnDef = expression.Index.Columns.ElementAt(i);

                if (i > 0)
                    indexColumns.Append(", ");
                else indexDirection = columnDef.Direction;

                indexColumns.Append(Quoter.QuoteColumnName(TruncateToMaxLength(columnDef.Name)));

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
            return compatabilityMode.HandleCompatabilty("Alter column is not supported as expected");
        }


        public override string Generate(CreateSequenceExpression expression)
        {
            return String.Format("CREATE SEQUENCE {0}", Quoter.QuoteSequenceName(expression.Sequence.Name));
        }
        
        public override string Generate(DeleteSequenceExpression expression)
        {
            return String.Format("DROP SEQUENCE {0}", Quoter.QuoteSequenceName(expression.SequenceName));
        }

        public string GenerateAlterSequence(SequenceDefinition sequence)
        {
            if (sequence.StartWith != null)
                return String.Format("ALTER SEQUENCE {0} RESTART WITH {1}", Quoter.QuoteSequenceName(sequence.Name), sequence.StartWith.ToString());
            
            return String.Empty;
        }

        #endregion


        #region Name truncation overrides

        public override string Generate(CreateTableExpression expression)
        {
            expression.TableName = TruncateToMaxLength(expression.TableName);
            return base.Generate(expression);
        }
        
        public override string Generate(DeleteTableExpression expression)
        {
            expression.TableName = TruncateToMaxLength(expression.TableName);
            return base.Generate(expression);
        }

        public override string Generate(RenameTableExpression expression)
        {
            return compatabilityMode.HandleCompatabilty("Rename table is not supported");
        }

        public override string Generate(CreateColumnExpression expression)
        {
            expression.TableName = TruncateToMaxLength(expression.TableName);
            expression.Column.Name = TruncateToMaxLength(expression.Column.Name);
            return base.Generate(expression);
        }

        public override string Generate(DeleteColumnExpression expression)
        {
            expression.TableName = TruncateToMaxLength(expression.TableName);
            expression.ColumnNames = TruncateToMaxLength(expression.ColumnNames);
            return base.Generate(expression);
        }

        public override string Generate(RenameColumnExpression expression)
        {
            expression.OldName = TruncateToMaxLength(expression.OldName);
            expression.NewName = TruncateToMaxLength(expression.NewName);
            return base.Generate(expression);
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            expression.Index.Name = TruncateToMaxLength(expression.Index.Name);
            expression.Index.TableName = TruncateToMaxLength(expression.Index.TableName);
            return base.Generate(expression);
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            expression.Constraint.ConstraintName = TruncateToMaxLength(expression.Constraint.ConstraintName);
            expression.Constraint.TableName = TruncateToMaxLength(expression.Constraint.TableName);
            return base.Generate(expression);
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            expression.Constraint.ConstraintName = TruncateToMaxLength(expression.Constraint.ConstraintName);
            expression.Constraint.TableName = TruncateToMaxLength(expression.Constraint.TableName);
            return base.Generate(expression);
        }

        public override string Generate(CreateForeignKeyExpression expression)
        {
            expression.ForeignKey.Name = TruncateToMaxLength(expression.ForeignKey.Name);
            expression.ForeignKey.PrimaryTable = TruncateToMaxLength(expression.ForeignKey.PrimaryTable);
            expression.ForeignKey.ForeignTable = TruncateToMaxLength(expression.ForeignKey.ForeignTable);
            expression.ForeignKey.PrimaryColumns = TruncateToMaxLength(expression.ForeignKey.PrimaryColumns);
            expression.ForeignKey.ForeignColumns = TruncateToMaxLength(expression.ForeignKey.ForeignColumns);

            return base.Generate(expression);
        }

        public override string GenerateForeignKeyName(CreateForeignKeyExpression expression)
        {
            expression.ForeignKey.Name = TruncateToMaxLength(expression.ForeignKey.Name);
            return TruncateToMaxLength(base.GenerateForeignKeyName(expression));
        }

        public override string Generate(DeleteForeignKeyExpression expression)
        {
            expression.ForeignKey.Name = TruncateToMaxLength(expression.ForeignKey.Name);
            return base.Generate(expression);
        }

        public override string Generate(InsertDataExpression expression)
        {
            expression.TableName = TruncateToMaxLength(expression.TableName);
            //can't change it, force exception
            expression.Rows.ForEach(x => x.ForEach(y => TruncateToMaxLength(y.Key, true)));
            return base.Generate(expression);
        }

        public override string Generate(UpdateDataExpression expression)
        {
            expression.TableName = TruncateToMaxLength(expression.TableName);
            //can't change it, force exception
            expression.Set.ForEach(x => TruncateToMaxLength(x.Key, true));
            return base.Generate(expression);
        }

        public override string Generate(DeleteDataExpression expression)
        {
            expression.TableName = TruncateToMaxLength(expression.TableName);
            //can't change it, force exception
            expression.Rows.ForEach(x => x.ForEach(y => TruncateToMaxLength(y.Key, true)));

            return base.Generate(expression);
        }
        
        #endregion

        
        #region Alter column generators
        
        public virtual string GenerateSetNull(ColumnDefinition column)
        {
            return String.Format(AlterColumnSetNullable,
                column.IsNullable ? "1" : "NULL",
                Quoter.QuoteValue(column.TableName),
                Quoter.QuoteValue(column.Name)
                );
        }

        public virtual string GenerateSetType(ColumnDefinition column)
        {
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

        
        protected string TruncateToMaxLength(string name) { return TruncateToMaxLength(name, false); }
        protected string TruncateToMaxLength(string name, bool forceExceptionOnTooLong)
        {
            if (!String.IsNullOrEmpty(name))
            {
                if (
                    (forceExceptionOnTooLong || !FBOptions.TruncateLongNames)
                    && (name.Length > FirebirdOptions.MaxNameLength)
                    )
                    throw new ArgumentException(String.Format("Name too long: {0}", name));

                return name.Substring(0, Math.Min(FirebirdOptions.MaxNameLength, name.Length));
            }
            return name;
        }

        protected List<string> TruncateToMaxLength(ICollection<string> list)
        {
            List<string> ret = new List<string>();
            list.ToList().ForEach(x => ret.Add(TruncateToMaxLength(x)));
            return ret;
        }

        #endregion

    }
}
