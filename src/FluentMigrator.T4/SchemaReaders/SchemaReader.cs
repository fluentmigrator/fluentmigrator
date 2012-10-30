using System.Data.Common;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace FluentMigrator.T4
{
    public abstract class SchemaReader
    {
        public abstract Tables ReadSchema(DbConnection connection, DbProviderFactory factory);
        public TextWriter outer;
        public void WriteLine(string o)
        {
            this.outer.WriteLine(o);
        }

        private static readonly Regex rxCleanUp = new Regex(@"[^\w\d_]", RegexOptions.Compiled);


        public static Func<string, string> CleanUp = (str) =>
        {
            str = rxCleanUp.Replace(str, "_");
            if (Char.IsDigit(str[0]))
            {
                str = "_" + str;
            }

            return str;
        };

        public static int GetDatatypePrecision(string type)
        {
            int startPos = type.IndexOf(",");
            if (startPos < 0)
            {
                return -1;
            }
            int endPos = type.IndexOf(")");
            if (endPos < 0)
            {
                return -1;
            }
            string typePrecisionStr = type.Substring(startPos + 1, endPos - startPos - 1);
            int result = -1;
            if (Int32.TryParse(typePrecisionStr, out result))
            {
                return result;
            }
            else
            {
                return -1;
            }
        }

        public static int GetDatatypeSize(string type)
        {
            int startPos = type.IndexOf("(");
            if (startPos < 0)
            {
                return -1;
            }
            int endPos = type.IndexOf(",");
            if (endPos < 0)
            {
                endPos = type.IndexOf(")");
            }
            string typeSizeStr = type.Substring(startPos + 1, endPos - startPos - 1);
            int result = -1;
            if (Int32.TryParse(typeSizeStr, out result))
            {
                return result;
            }
            else
            {
                return -1;
            }
        }
    }
}