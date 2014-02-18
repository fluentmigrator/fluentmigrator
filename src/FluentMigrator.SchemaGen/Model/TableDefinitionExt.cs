using System.Collections.Generic;
using System.Linq;
using FluentMigrator.SchemaGen.SchemaWriters;

namespace FluentMigrator.SchemaGen.Model
{
    public class TableDefinitionExt
    {
        public virtual string Name { get; set; }
        public virtual string SchemaName { get; set; }

        public ICollection<ColumnDefinitionExt> Columns { get; set; }
        public ICollection<ForeignKeyDefinitionExt> ForeignKeys { get; set; }
        public ICollection<IndexDefinitionExt> Indexes { get; set; }

        internal CodeLines GetCreateCode()
        {
            var lines = new CodeLines();

            lines.WriteLine("Create.Table(\"{1}\").InSchema(\"{0}\")", SchemaName, Name);

            lines.Indent();
            foreach (ColumnDefinitionExt column in Columns)
            {
                string colCode = column.CreateCode;
                if (Columns.Last() == column) colCode += ";";

                lines.WriteLine(colCode);
            }
            lines.Indent(-1);

            var nonColIndexes = GetNonColumnIndexes();

            // Split lines to make indenting work.
            lines.WriteSplitLines(nonColIndexes.Select(index => index.CreateCode));
            lines.WriteSplitLines(ForeignKeys.Select(fk => fk.CreateCode));

            return lines;
        }

        public string GetDeleteCode()
        {
            return string.Format("Delete.Table(\"{0}\").InSchema(\"{1}\");", Name, SchemaName);
        }

        public IEnumerable<IndexDefinitionExt> GetNonColumnIndexes()
        {
            // Only single colum primary keys are declared with the table.
            return from ix in Indexes where !(ix.IsPrimary && ix.Columns.Count() == 1) select ix;
        }

        public void GetAlterTableCode(CodeLines lines, IEnumerable<string> codeChanges, IEnumerable<string> oldCode = null)
        {
            var changes = codeChanges.ToList();
            if (changes.Any())
            {
                //lines.WriteLine();
                if (Options.Instance.ShowChanges && oldCode != null)
                {
                    lines.WriteComments(oldCode);
                }

                lines.WriteLine("Alter.Table(\"{0}\").InSchema(\"{1}\")", Name, SchemaName);
                lines.Indent();
                lines.WriteLines(changes, ";");
                lines.Indent(-1);
            }
        }

        /// <summary>
        /// Indexes containing updated columns.
        /// </summary>
        /// <param name="updatedColNames"></param>
        /// <returns></returns>
        public IEnumerable<IndexDefinitionExt> FindIndexesContainingUpdatedColumnNames(IEnumerable<string> updatedColNames)
        {
            return from index in Indexes
                   let colNames = index.Columns.Select(col => col.Name)
                   where colNames.Any(updatedColNames.Contains)
                   select index;
        }

        /// <summary>
        /// Find ForeignKeys containing updated columns.
        /// </summary>
        /// <param name="updatedColNames"></param>
        /// <returns></returns>
        private IEnumerable<ForeignKeyDefinitionExt> FindFKsContainingUpdatedColumnNames(IEnumerable<string> updatedColNames)
        {
            return from fk in ForeignKeys
                   let colNames = fk.ForeignColumns
                   where colNames.Any(updatedColNames.Contains)
                   select fk;
        }

    }
}