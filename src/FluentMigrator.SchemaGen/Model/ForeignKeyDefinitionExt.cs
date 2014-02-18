using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FluentMigrator.Model;

namespace FluentMigrator.SchemaGen.Model
{
    public class ForeignKeyDefinitionExt: ForeignKeyDefinition, ICodeComparable
    {
        private string codeDef;

        private string ToStringArray(IEnumerable<string> cols)
        {
            string strCols = String.Join(", ", cols.Select(col => '"' + col + '"').ToArray());
            return '{' + strCols + '}';
        }

        public string FQName
        {
            get { return Name; }
        }

        public string CreateCode
        {
            get { return GetCreateForeignKeyCode(); }
        }

        public string DeleteCode
        {
            get { return GetRemoveFKCode(); }
        }

        public string DefinitionCode
        {
            get { return codeDef ?? (codeDef = GetForeignKeyDefCode()); }
        }

        public bool TypeChanged { get; set; }

        public string GetCreateForeignKeyCode()
        {
            return string.Format("Create.ForeignKey(\"{0}\")\r\n\t{1};", Name, GetForeignKeyDefCode());
        }

        public string GetRemoveFKCode()
        {
            return string.Format("Delete.ForeignKey(\"{0}\").OnTable(\"{1}\").InSchema(\"{2}\");", Name, ForeignTable, ForeignTableSchema);
        }

        public string GetForeignKeyDefCode()
        {
            //Create.ForeignKey("fk_TestTable2_TestTableId_TestTable_Id")
            //    .FromTable("TestTable2").ForeignColumn("TestTableId")
            //    .ToTable("TestTable").PrimaryColumn("Id");

            var sb = new StringBuilder();

            // From Table
            string fromTable = string.Format(".FromTable(\"{0}\")", ForeignTable);

            if (ForeignColumns.Count == 1)
            {
                fromTable += string.Format(".ForeignColumn(\"{0}\")", ForeignColumns.First());
            }
            else
            {
                fromTable += string.Format("ForeignColumns({0})", ToStringArray(ForeignColumns));
            }

            sb.AppendLine("\t" + fromTable);

            // To Table
            string toTable = string.Format(".ToTable(\"{0}\")", PrimaryTable);

            if (PrimaryColumns.Count == 1)
            {
                toTable += string.Format(".PrimaryColumn(\"{0}\")", PrimaryColumns.First());
            }
            else
            {
                toTable += string.Format(".PrimaryColumns({0})", ToStringArray(PrimaryColumns));
            }

            sb.AppendLine("\t" + toTable);

            string rule = "";

            if (OnDelete != Rule.None && OnDelete == OnUpdate)
            {
                rule += string.Format(".OnDeleteOrUpdate(System.Data.Rule.{0})", OnDelete);
            }
            else
            {
                if (OnDelete != Rule.None)
                {
                    rule += string.Format(".OnDelete(System.Data.Rule.{0})", OnDelete);
                }

                if (OnUpdate != Rule.None)
                {
                    rule += string.Format(".OnUpdate(System.Data.Rule.{0})", OnUpdate);
                }
            }

            if (rule.Length > 0) sb.AppendLine("\t" + rule);

            return sb.ToString().Trim();
        }

        public string GetDeleteForeignKeyCode()
        {
            return string.Format("Delete.ForeignKey(\"{0}\").OnTable(\"{1}\").InSchema(\"{2}\");", Name, PrimaryTable, PrimaryTableSchema);
        }
    }
}