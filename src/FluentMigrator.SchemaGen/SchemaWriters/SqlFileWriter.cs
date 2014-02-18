using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentMigrator.Runner;

namespace FluentMigrator.SchemaGen.SchemaWriters
{
    /// <summary>
    /// Write embedded or non embedded SQL
    /// </summary>
    public class SqlFileWriter
    {
        private readonly CodeLines lines;
        private readonly IOptions options;
        private readonly IAnnouncer announcer;

        public SqlFileWriter(IOptions options, IAnnouncer announcer)
        {
            this.options = options;
            this.announcer = announcer;
        }

        private string GetRelativePath(FileInfo file)
        {
            return file.FullName.Replace(Environment.CurrentDirectory + "\\", "");
        }

        private string GetRelativePath(DirectoryInfo dir)
        {
            return dir.FullName.Replace(Environment.CurrentDirectory + "\\", "");
        }

        public CodeLines EmbedSql(string sqlStatement)
        {
            var sqlLines = sqlStatement.Replace("\r", "").Split('\n').Where(line => line.Trim().Length > 0).ToArray();

            var codeLines = new CodeLines();
            for (int i = 0; i < sqlLines.Length; i++)
            {
                string sqlLine = sqlLines[i];
                string codeLine = "";
                if (sqlLine.Trim() == "") continue;

                sqlLine = sqlLine.Replace("\"", "\"\"");  // " -> "" since we're using @" "

                if (i == 0)
                {
                    codeLine += "Execute.Sql(@\"" + sqlLine;
                }
                else
                {
                    codeLine += "\t" + sqlLine;
                }

                if (i == sqlLines.Length - 1)
                {
                    codeLine += "\");";
                }

                codeLines.WriteLine(codeLine);
            }

            return codeLines;
        }

        private void ExecuteSqlFile(CodeLines lines, FileInfo sqlFile)
        {
            if (options.EmbedSql)
            {

                string allLines = File.ReadAllText(sqlFile.FullName);
                Regex goStatement = new Regex("\\s+GO\\s+|^GO\\s+", RegexOptions.Multiline);

                lines.WriteComment(GetRelativePath(sqlFile));

                foreach (var sqlStatment in goStatement.Split(allLines))
                {
                    lines.WriteLines(EmbedSql(sqlStatment));
                }
            }
            else
            {
                lines.WriteLine("Execute.Script(\"{0}\");", GetRelativePath(sqlFile).Replace("\\", "\\\\"));   
            }
        }

        public void ExecuteSqlDirectory(CodeLines lines, DirectoryInfo sqlDirectory)
        {
            if (options.EmbedSql)
            {
                foreach (var sqlFile in sqlDirectory.GetFiles("*.sql", SearchOption.AllDirectories).OrderBy(file => file.FullName))
                {
                    if (sqlFile.Length > 0)
                    {
                        lines.WriteComment(GetRelativePath(sqlFile));
                        ExecuteSqlFile(lines, sqlFile);
                    }
                }
            }
            else
            {
                // Runs MigrationExt.ExecuteScriptDirectory
                lines.WriteLine("ExecuteScriptDirectory(\"{0}\");", GetRelativePath(sqlDirectory).Replace("\\", "\\\\"));
            }
        }

        public string GetSqlDirectoryPath(string subfolder)
        {
            string path = Path.Combine(options.SqlDirectory ?? "SQL", options.MigrationVersion);
            return Path.Combine(path, subfolder);
        }

        public void MigrateData(CodeLines lines, string schemaSqlFolder, string tableName)
        {
            if (options.SqlDirectory != null)
            {
                string sqlDirPath = GetSqlDirectoryPath(schemaSqlFolder);
                string sqlFilePath = Path.Combine(sqlDirPath, tableName + ".sql");

                var sqlFile = new FileInfo(sqlFilePath);

                if (!sqlFile.Exists)
                {
                    announcer.Say("{0}: Data migration SQL script not found.", sqlFile.FullName);
                    return;
                }

                ExecuteSqlFile(lines, sqlFile);
            }
        }
    }
}