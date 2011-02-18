

namespace FluentMigrator.Runner.Generators.Generic
{
    using System;
    using System.Collections.Generic;
    using FluentMigrator.Expressions;
    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Base;

    public abstract class GenericGenerator : GeneratorBase
    {
        public GenericGenerator(IColumn column, IConstantFormatter constantFormatter) : base(column,constantFormatter)
        {}

        public virtual string CreateTable { get { return "create table "; } }
        public virtual string AlterTable { get { return "alter table "; } }
        public virtual string DropTable { get { return "drop table"; } }
        
        public virtual string AddColumn { get { return "add column"; } }
        public virtual string DropColumn { get { return "drop column"; } }
        public virtual string AlterColumn { get { return "alter column"; } }

        public virtual string CreateSchema { get { return "create schema"; } }
        public virtual string DropSchema { get { return "drop schema"; } }







        /// <summary>
        /// Outputs a create table string
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override string Generate(CreateTableExpression expression)
        {
            if (string.IsNullOrEmpty(expression.TableName)) throw new ArgumentNullException("Table name cannot be empty");
            var tableCreationString =  CreateTable + QuoteForTableName(expression.TableName) + " ({0})";
            return string.Format(tableCreationString, this.Column.Generate(expression));
        }

        


        /// <summary>
        /// Returns the opening quote identifier - " is the standard according to the specification
        /// </summary>
        public virtual string OpenQuote { get { return "\""; } }

        /// <summary>
        /// Returns the closing quote identifier - " is the standard according to the specification
        /// </summary>
        public virtual string CloseQuote { get { return "\""; } }

        public virtual string OpenQuoteEscapeString { get { return OpenQuote.PadRight(2, OpenQuote.ToCharArray()[0]); } }
        public virtual string CloseQuoteEscapeString { get { return CloseQuote.PadRight(2, CloseQuote.ToCharArray()[0]); } }

        /// <summary>
        /// Returns true is the value starts and ends with a close quote
        /// </summary>
        public virtual bool IsQuoted(string name)
        {
            //This can return true incorrectly in some cases edge cases.
            //If a string say [myname]] is passed in this is not correctly quote for MSSQL but this function will
            //return true. 
            return (name.StartsWith(OpenQuote) && name.EndsWith(CloseQuote));
        }

        /// <summary>
        /// Returns a quoted string that has been correctly escaped
        /// </summary>
        public virtual string Quote(string name)
        {
            string quotedName = name.Replace(OpenQuote, OpenQuoteEscapeString);
            //Check to see if we need to each closing quotes.
            //If closing quote is the same as the opening quote then no need to escape again
            if (OpenQuote != CloseQuote)
            {
                quotedName = quotedName.Replace(CloseQuote, CloseQuoteEscapeString);
            }

            return OpenQuote + quotedName + CloseQuote;
        }

        /// <summary>
        /// Quotes a column name
        /// </summary>
        public virtual string QuoteForColumnName(string columnName)
        {
            return IsQuoted(columnName) ? columnName : Quote(columnName);
        }

        /// <summary>
        /// Quotes a Table name
        /// </summary>
        public virtual string QuoteForTableName(string tableName)
        {
            return IsQuoted(tableName) ? tableName : Quote(tableName);
        }

        /// <summary>
        /// Quotes a Schema Name
        /// </summary>
        public virtual string QuoteForSchemaName(string schemaName)
        {
            return IsQuoted(schemaName) ? schemaName : Quote(schemaName);
        }

        /// <summary>
        /// Provides and unquoted, unescaped string
        /// </summary>
        public virtual string UnQuote(string quoted)
        {
            string unquoted;

            if (IsQuoted(quoted))
            {
                unquoted = quoted.Substring(1, quoted.Length - 2);
            }
            else
            {
                unquoted = quoted;
            }

            unquoted = unquoted.Replace(OpenQuoteEscapeString, OpenQuote);

            if (OpenQuote != CloseQuote)
            {
                unquoted = unquoted.Replace(CloseQuoteEscapeString, CloseQuote);
            }

            return unquoted;
        }
    }
}
