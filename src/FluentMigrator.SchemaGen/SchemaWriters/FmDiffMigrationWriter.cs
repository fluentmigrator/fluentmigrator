#region Apache 2.0 License
// 
// Copyright (c) 2014, Tony O'Hagan <tony@ohagan.name>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.SchemaGen.Extensions;
using FluentMigrator.SchemaGen.SchemaReaders;

namespace FluentMigrator.SchemaGen.SchemaWriters
{
    /// <summary>
    /// Writes a Fluent Migrator class for a database schema
    /// </summary>
    public class FmDiffMigrationWriter : IMigrationWriter
    {
        private readonly IOptions options;
        private readonly IAnnouncer announcer;
        private readonly IDbSchemaReader db1;
        private readonly IDbSchemaReader db2;
        private int step = 1;

        private readonly IDictionary<string, bool> tablesCompleted = new Dictionary<string, bool>();

        private readonly IList<string> classPaths = new List<string>();

        private static int indent = 0;
        private static StreamWriter writer;
        private static StringBuilder sb;
        private static readonly Stack<StringBuilder> nestedBuffers = new Stack<StringBuilder>();

        public FmDiffMigrationWriter(IOptions options, IAnnouncer announcer, IDbSchemaReader db1, IDbSchemaReader db2)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (announcer == null) throw new ArgumentNullException("announcer");
            if (db1 == null) throw new ArgumentNullException("db1");
            if (db2 == null) throw new ArgumentNullException("db2");

            indent = 0;
            writer = null;
            sb = null;

            this.options = options;
            this.announcer = announcer;
            this.db1 = db1;
            this.db2 = db2;
        }

        #region WriteLine/Indent/Block/Buffer Helpers

        private static void Indent()
        {
            if (sb == null)
            {
                for (int i = 0; i < indent; i++)
                {
                    writer.Write("    ");
                }
            }
            else
            {
                for (int i = 0; i < indent; i++)
                {
                    sb.Append("    ");
                }
            }
        }

        private static void WriteLine()
        {
            if (sb == null)
            {
                writer.WriteLine();
            }
            else
            {
                sb.AppendLine();
            }
        }

        private static void WriteLine(string line)
        {
            if (line.Length > 0) Indent();

            if (sb == null)
            {
                writer.WriteLine(line);
            }
            else
            {
                sb.AppendLine(line);
            }
        }

        private static void WriteLine(string format, params object[] args)
        {
            Indent();
            if (sb == null)
            {
                writer.WriteLine(format, args);
            }
            else
            {
                sb.AppendFormat(format, args);
                sb.AppendLine();
            }
        }

        private static void WriteLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                WriteLine(line);
            }
        }

        private static void WriteComment(string line)
        {
            WriteLine("/* {0} */", line);
        }

        private static void WriteComments(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                WriteComment(line);
            }
        }

        private void ShowOldCode(IEnumerable<string> oldCode)
        {
            if (options.ShowOldCode)
            {
                WriteComments(oldCode);
            }
        }

        private static void WriteLines(IEnumerable<string> lines, string appendLastLine)
        {
            var lineArr = lines.ToArray();
            for (int i = 0; i < lineArr.Length; i++)
            {
                if (i < lineArr.Length - 1)
                {
                    WriteLine(lineArr[i]);
                }
                else
                {
                    WriteLine(lineArr[i] + appendLastLine);
                }
            }
        }

        /// <summary>
        /// Returns the lines output by <paramref name="action"/>.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private string Buffer(Action action)
        {
            if (sb != null) nestedBuffers.Push(sb);
            sb = new StringBuilder();

            action();

            string result = sb.ToString();
            sb = nestedBuffers.Count == 0 ? null : nestedBuffers.Pop();

            return result;
        }

        protected class Indenter : IDisposable
        {
            protected internal Indenter()
            {
                indent++;
            }

            public void Dispose()
            {
                indent--;
            }
        }

        protected class Block : IDisposable
        {
            protected internal Block()
            {
                WriteLine("{");
                indent++;
            }

            public void Dispose()
            {
                indent--;
                WriteLine("}");
            }
        }

        #endregion

        #region Embed or Execute external SQL scripts

        private void EmbedSql(string sqlStatement)
        {
            Regex lineDelim = new Regex("$");
            var lines = lineDelim.Split(sqlStatement);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Trim() == "") continue;

                line = line.Replace("\"", "\\\"");

                if (i == 0)
                {
                    line = "Execute.Sql(@\"" + line;
                }
                else
                {
                    line = "\t" + line;
                }

                if (i == lines.Length - 1)
                {
                    line += "\");";
                }

                WriteLine(line);
            }
        }

        private void EmbedSqlFile(FileInfo sqlFile)
        {
            string allLines = File.ReadAllText(sqlFile.FullName);
            Regex goStatement = new Regex("^ *GO *$");

            foreach (var sqlStatment in goStatement.Split(allLines))
            {
                EmbedSql(sqlStatment);
            }
        }

        private void EmbedSqlDirectory(DirectoryInfo sqlDirectory)
        {
            foreach (var sqlFile in sqlDirectory.GetFiles("*.sql", SearchOption.AllDirectories).OrderBy(file => file.FullName))
            {
                if (sqlFile.Length > 0)
                {
                    WriteComment(sqlFile.FullName.Replace(Environment.CurrentDirectory + "\\", ""));
                    EmbedSqlFile(sqlFile);                    
                }
            }
        }

        private string GetSqlDirectoryPath(string subfolder)
        {
            return Path.Combine(options.SqlDirectory ?? "SQL", options.MigrationVersion);
        }

        private void MigrateData(string tableName)
        {
            if (options.SqlDirectory != null)
            {
                string sqlDirPath = GetSqlDirectoryPath("MigrateData");
                string sqlFilePath = Path.Combine(sqlDirPath, tableName + ".sql");

                var sqlFile = new FileInfo(sqlFilePath);

                if (!sqlFile.Exists)
                {
                    announcer.Say("{0}: Data migration SQL script not found.", sqlFile.FullName);
                    return;
                }

                EmbedSqlFile(sqlFile);
            }
        }

        #endregion

        #region Emit Classes

        /// <summary>
        /// Writes a Migrator class.
        /// Only creates the class file if the <paramref name="upMethod"/> emits code.
        /// </summary>
        /// <param name="dirName">Project subdirectory (included in namespace)</param>
        /// <param name="className">Migration class name</param>
        /// <param name="upMethod">Action to emit Up() method code</param>
        /// <param name="downMethod">
        /// Optional Action to emit Down() method code.
        /// If null, the class inherits from AutoReversingMigrationExt, otherwise it inherits from MigrationExt.
        /// These are project classes that inherit from AutoReversingMigration and Migration.
        /// </param>
        /// <param name="addTags">Additional FluentMigrator Tags applied to the class</param> 
        protected void WriteClass(string dirName, string className, Action upMethod, Action downMethod = null, string addTags = null)
        {
            // If no code is generated for an Up() method => No class is emitted
            string upMethodCode = Buffer(upMethod);
            if (upMethodCode.Length == 0) return;

            // Prefix class with zero filled order number.
            className = string.Format("M{0,4:D4}0_{1}", step, className);

            string fullDirName = options.OutputDirectory;
            if (!string.IsNullOrEmpty(dirName))
            {
                fullDirName = Path.Combine(options.OutputDirectory, dirName);
            }

            new DirectoryInfo(fullDirName).Create();

            string classPath = Path.Combine(fullDirName, className + ".cs");
            Console.WriteLine(classPath);

            classPaths.Add(classPath);

            try
            {
                using (var fs = new FileStream(classPath, FileMode.Create))
                using (var writer1 = new StreamWriter(fs))
                {
                    writer = writer1; // assigns class 'writer' variable

                    WriteLine("using System;");
                    WriteLine("using System.Collections.Generic;");
                    WriteLine("using System.Linq;");
                    WriteLine("using System.Web;");
                    WriteLine("using FluentMigrator;");
                    WriteLine("using Migrations.FM_Extensions;");

                    WriteLine(String.Empty);

                    string ns = options.NameSpace;
                    if (!string.IsNullOrEmpty(dirName))
                    {
                        ns = ns + "." + dirName.Replace("\\", ".");
                    }
                    WriteLine("namespace {0}", ns);

                    using (new Block()) // namespace {}
                    {
                        WriteLine("[MigrationVersion({0})]", options.MigrationVersion.Replace(".", ", ") + ", " + step);

                        string tags = options.Tags ?? "" + addTags ?? "";
                        if (!string.IsNullOrEmpty(tags))
                        {
                            WriteLine("[Tags(\"{0}\")]", tags.Replace(",", "\", \""));
                        }
                       
                        WriteLine("public class {0} : {1}", className, downMethod == null ? "AutoReversingMigrationExt" : "MigrationExt");
                        using (new Block()) // class {}
                        {
                            string[] upMethodLines = SplitCodeLines(upMethodCode);
                            WriteMethod("Up", () => WriteLines(upMethodLines));

                            if (downMethod != null)
                            {
                                WriteMethod("Down", downMethod);
                            }
                        }
                    }

                    step++;

                    writer.Flush();
                    writer = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(classPath + ": Failed to write class file");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null) Console.WriteLine(ex.InnerException.Message);
            }
        }

        private string[] SplitCodeLines(string codeText)
        {
            // Trim extra leading / trailing blank lines.
            if (codeText.StartsWith(Environment.NewLine))
            {
                codeText = codeText.Substring(Environment.NewLine.Length);
            }

            if (codeText.EndsWith(Environment.NewLine))
            {
                codeText = codeText.Substring(0, codeText.Length - Environment.NewLine.Length);
            }

            // Need to split into lines so indenting works 
            return codeText.Replace(Environment.NewLine, "\n").Split('\n');
        }

        private void WriteMethod(string name, Action body)
        {
            WriteLine();
            WriteLine("public override void {0}()", name);
            using (new Block())
            {
                body();
            }
        }

        private void CantUndo()
        {
            WriteLine("throw new Exception(\"Cannot undo this database upgrade\");");
        }

        #endregion

        // Main Entry point
        public IEnumerable<string> WriteMigrationClasses()
        {
            step = options.StepStart;

            WriteClass("", "Initial", () => WriteComment("Sets initial version to " + options.MigrationVersion + "." + step));

            // TODO: Create new user defined DataTypes

            // Create/Update All tables/columns/indexes/foreign keys
            CreateUpdateTables();

            // TODO: Drop/Create new or modified scripts (SPs/Views/Functions)
            // CreateUpdateScripts();

            WriteSqlScriptClass("Views");
            WriteSqlScriptClass("Stored Procedures");
            WriteSqlScriptClass("Seed Data");
            WriteSqlScriptClass("Demo Data", "Load demo data", "Demo");
            WriteSqlScriptClass("Test Data", "Load test data", "Test");

            if (options.DropTables)
            {
                // Drop tables in order of their FK dependency.
                WriteClass("", "DropTables", DropTables, CantUndo);
            }

            if (options.DropScripts)
            {
                // Drop old SPs/Views/Functions
                WriteClass("", "DropScripts", DropScripts, CantUndo);
            }

            // TODO: Drop old user defined DataTypes
            // WriteClass("Data", "LoadSeedData", LoadSeedData, DropSeedData);

            // TODO: Load/Update Demo Data (if tagged "Demo")
            // WriteClass("Data", "LoadDemoData", LoadDemoData, DropDemoData);

            // TODO: Load/Update Test Data (if tagged "Test")
            // WriteClass("Test", "LoadTestData", LoadTestData, DropTestData);

            if (options.StepEnd != -1)
            {
                // The assigned range of step numbers exceeded the upper limit.
                if (step > options.StepEnd) throw new Exception("Last step number exceeded the StepEnd option.");
                step = options.StepEnd;
            }

            WriteClass("", "Final", () => WriteComment("Sets final version to " + options.MigrationVersion + "." + step));

            return classPaths;
        }

        private void WriteSqlScriptClass(string subfolder, string commment = null, string tags = null)
        {
            var sqlDir = new DirectoryInfo(GetSqlDirectoryPath(subfolder));
            if (!sqlDir.Exists)
            {
                announcer.Say(sqlDir.FullName + ": Did not find SQL script folder");
                return;
            }

            Action upMethod = () =>
                {
                    if (commment != null) WriteComment(commment);

                    if (options.SqlDirectory != null)
                    {
                        EmbedSqlDirectory(sqlDir);
                    }
                };

            WriteClass("", subfolder.Replace(" ",""), upMethod, CantUndo, tags);
        }

        #region Drop Tables and Code
        private void DropTables()
        {
            // TODO: Currently ignoring Schema name for table objects.
            var db1FkOrder = db1.TablesInForeignKeyOrder(false); // descending order

            var removedTableNames = db1.Tables.Keys.Except(db2.Tables.Keys).ToList();
            removedTableNames = removedTableNames.OrderBy(t => -db1FkOrder[t]).ToList();

            foreach (TableDefinition table in removedTableNames.Select(name => db1.Tables[name]))
            {
                foreach (ForeignKeyDefinition fk in table.ForeignKeys)
                {
                    WriteLine("Delete.ForeignKey(\"{0}\").OnTable(\"{1}\").InSchema(\"{2}\");", fk.Name, fk.PrimaryTable, fk.PrimaryTableSchema);
                }

                WriteLine("Delete.Table(\"{0}\").InSchema(\"{1}\");", table.Name, table.SchemaName);
            }
        }

        private void DropScripts()
        {
            foreach (var name in db1.StoredProcedures.Except(db2.StoredProcedures))
            {
                WriteLine("DeleteStoredProcedure(\"{0}\");", name);
            }

            foreach (var name in db1.Views.Except(db2.Views))
            {
                WriteLine("DeleteView(\"{0}\");", name);
            }

            foreach (var name in db1.UserDefinedFunctions.Except(db2.UserDefinedFunctions))
            {
                WriteLine("DeleteFunction(\"{0}\");", name);
            }

            foreach (var name in db1.UserDefinedDataTypes.Except(db2.UserDefinedDataTypes))
            {
                WriteLine("DeleteType(\"{0}\");", name);
            }
        }
        #endregion

        #region Create / Update Tables
        private void CreateUpdateTables()
        {
            var db1Tables = db1.Tables;

            // TODO: Currently ignoring Schema name for table objects.

            var db2FkOrder = db2.TablesInForeignKeyOrder(true);
            var db2TablesInFkOrder = db2.Tables.Values.OrderBy(tableDef => db2FkOrder[tableDef.Name]);

            foreach (TableDefinition table in db2TablesInFkOrder)
            {
                TableDefinition newTable = table;

                if (db1Tables.ContainsKey(newTable.Name))
                {
                    TableDefinition oldTable = db1Tables[newTable.Name];
                    WriteClass("", "Update_" + table.Name, () => UpdateTable(oldTable, newTable));
                }
                else
                {
                    WriteClass("", "Create_" + table.Name, () => CreateTable(newTable));
                }
            }
        }

        private void CreateTable(TableDefinition table)
        {
            WriteLine("Create.Table(\"{1}\").InSchema(\"{0}\")", table.SchemaName, table.Name);
            
            using (new Indenter())
            {
                foreach (ColumnDefinition column in table.Columns)
                {
                    string colCode = GetColumnCode(column);
                    if (table.Columns.Last() == column) colCode += ";";

                    WriteLine(colCode);
                }
            }

            WriteLine();

            var nonColIndexes = GetNonColumnIndexes(table);

            WriteLines(nonColIndexes.Select(index => GetCreateIndexCode(table, index)));
            WriteLines(table.ForeignKeys.Select(GetCreateForeignKeyCode));
        }

        /// <summary>
        /// Gets the set of tables indexes that are not declared as part of a table column definition
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private IEnumerable<IndexDefinition> GetNonColumnIndexes(TableDefinition table)
        {
            // Names of indexes declared as as part of table column definition
            var colIndexNames = from col in table.Columns
                                where col.IsIndexed 
                                select col.IsPrimaryKey ? col.PrimaryKeyName : col.IndexName;

            // Remaining indexes undeclared
            return from index in table.Indexes where !colIndexNames.Contains(index.Name) select index;
        }

        /// <summary>
        /// Generate code based on changes to table columns, indexes and foreign keys
        /// </summary>
        /// <param name="oldTable"></param>
        /// <param name="newTable"></param>
        private void UpdateTable(TableDefinition oldTable, TableDefinition newTable)
        {
            if (tablesCompleted.ContainsKey(newTable.Name)) return;
            tablesCompleted[newTable.Name] = true;

            // Strategy is to compute generated FluentMigration API code 
            // for each table related object (columns, indexes and foreign keys) 
            // and then detect changes in EITHER the list of object names 
            // OR the code for objects of matching name.

            // We only emit code if there are actual changes so we can detect 
            // when there are NO changes and then not emit the class.

            // TODO: If a column is updated but it's part of a foreign key relationship, need to drop/create the FK and alter the columns in both tables.
            // TODO: MSAccess appears to create indexes when a FK is created => so we need to conditionally create these indexes. 

            // Columns
            IDictionary<string, string> oldCols = oldTable.Columns.ToDictionary(col => col.Name, GetColumnCode);
            IDictionary<string, string> newCols = newTable.Columns.ToDictionary(col => col.Name, GetColumnCode);

            var addedColsCode = oldCols.GetAdded(newCols).Select(colCode => colCode.Replace("WithColumn", "AddColumn"));
            var updatedColsOldCode = newCols.GetUpdated(oldCols);
            var updatedColsCode = oldCols.GetUpdated(newCols).Select(colCode => colCode.Replace("WithColumn", "AlterColumn"));
            var removedColsDefCode = oldCols.GetRemoved(newCols);
            var removedColsCode = oldCols.GetRemovedNames(newCols).Select(colName => GetRemoveColumnCode(newTable, colName));

            // Indexes 
            IDictionary<string, string> oldIndexes = GetNonColumnIndexes(oldTable).ToDictionary(index => index.Name, index => GetCreateIndexCode(oldTable, index));
            IDictionary<string, string> newIndexes = GetNonColumnIndexes(newTable).ToDictionary(index => index.Name, index => GetCreateIndexCode(newTable, index));
            
            // Updated indexes are removed and added
            var oldUpdatedIndexDefCode = newIndexes.GetUpdated(oldIndexes);
            var newUpdatedIndexCode = oldIndexes.GetUpdated(newIndexes);
            var removeUpdatedIndexesCode = oldIndexes.GetUpdatedNames(newIndexes).Select(indexName => GetRemoveIndexCode(oldTable, indexName));
 
            var addedIndexCode = oldIndexes.GetAdded(newIndexes);
            var removedIndexNames = oldIndexes.GetRemovedNames(newIndexes);
            var removedIndexOldDefCode = oldIndexes.GetRemoved(newIndexes);
            var removedIndexCode = removedIndexNames.Select(indexName => GetRemoveIndexCode(oldTable, indexName));

            // Foreign Keys
            IDictionary<string, string> oldFKs = oldTable.ForeignKeys.ToDictionary(fk => fk.Name, GetCreateForeignKeyCode);
            IDictionary<string, string> newFKs = newTable.ForeignKeys.ToDictionary(fk => fk.Name, GetCreateForeignKeyCode);

            // Updated foreign keys are removed and added
            var updatedFkNames = oldFKs.GetUpdatedNames(newFKs);
            var removeUpdatedFkCode = updatedFkNames.Select(fkName => GetRemoveFKCode(oldTable, fkName));
            var oldUpdatedFkDefCode = newFKs.GetUpdated(oldFKs);
            var newUpdatedFkCode = oldFKs.GetUpdated(newFKs);

            var addedFkCode = oldFKs.GetAdded(newFKs);
            var removedFkNames = oldFKs.GetRemovedNames(newFKs);
            var removedFkDefCode = oldFKs.GetRemoved(newFKs);
            var removedFkCode = removedFkNames.Select(fkName => GetRemoveFKCode(oldTable, fkName));

            // We keep old indexes and columns as long as possible as they may be needed 
            // when the developer adds custom code to in MigrateData() to migrate the data from old to new schema.

            // When a column becomes NOT NULL and has a DEFAULT value, this emits SQL to set the default on all NULL column values.
            SetDefaultsIfNotNull(oldTable, newTable, oldCols.GetUpdatedNames(newCols));

            AlterTable(newTable, addedColsCode);    // Add NEW columns

            WriteChanges(addedIndexCode);           // Add NEW Indexes
            WriteChanges(addedFkCode);              // Add NEW foreign keys

            ShowOldCode(oldUpdatedIndexDefCode);    // Show old version of updated indexes definitions as comments.
            WriteChanges(removeUpdatedIndexesCode); // Remove UPDATED indexes

            ShowOldCode(oldUpdatedFkDefCode);       // Show old version of UPDATED foreign keys as comments
            WriteChanges(removeUpdatedFkCode);      // Remove UPDATED foreign keys

            ShowOldCode(removedFkDefCode);          // Show old code of removed FKs
            WriteChanges(removedFkCode);            // Remove OLD foreign keys

            ShowOldCode(updatedColsOldCode);        // Show old definition of updated columns
            AlterTable(newTable, updatedColsCode);  // Updated columns (including 1 column indexes)

            MigrateData(newTable.Name);             // Run data migration SQL

            WriteChanges(newUpdatedFkCode);         // Add UPDATED foreign keys
            WriteChanges(newUpdatedIndexCode);      // Add UPDATED indexes (excluding 1 column indexes)

            // Note: The developer may add custom data migration code here

            ShowOldCode(removedIndexOldDefCode);    // Comments showing OLD index definitions.
            WriteChanges(removedIndexCode);         // Remove OLD Indexes

            ShowOldCode(removedColsDefCode);        // Comments showing OLD column definitions.
            WriteChanges(removedColsCode);          // Remove OLD columns
        }

        private void AlterTable(TableDefinition table, IEnumerable<string> codeChanges)
        {
            var changes = codeChanges.ToList();
            if (changes.Any())
            {
                WriteLine("Alter.Table(\"{0}\").InSchema(\"{1}\")", table.Name, table.SchemaName);
                using (new Indenter())
                {
                    WriteLines(changes, ";");
                }
            }
        }

        private void WriteChanges(IEnumerable<string> codeChanges)
        {
            var changes = codeChanges.ToList();
            if (changes.Any())
            {
                WriteLine();
                WriteLines(changes);
            }
        }

        #endregion

        #region Column

        /// <summary>
        /// When a column NULL -> NOT NULL and has a default value, this emits SQL to set the default on all NULL column values
        /// </summary>
        /// <param name="oldTable"></param>
        /// <param name="newTable"></param>
        /// <param name="updatedColNames"></param>
        private void SetDefaultsIfNotNull(TableDefinition oldTable, TableDefinition newTable, IEnumerable<string> updatedColNames)
        {
            if (options.SetNotNullDefault)
            {
                // When a column NULL -> NOT NULL and has a default value, this emits SQL to set the default on all NULL column values
                foreach (string colName in updatedColNames)
                {
                    ColumnDefinition oldCol = oldTable.Columns.First(col => col.Name == colName);
                    ColumnDefinition newCol = newTable.Columns.First(col => col.Name == colName);

                    if (oldCol.IsNullable == true && newCol.IsNullable == false && newCol.DefaultValue != null)
                    {
                        EmbedSql(string.Format("UPDATE {0}.{1} SET {2} = {3} WHERE {2} IS NULL",
                                                   newTable.SchemaName, newTable.Name, 
                                                   newCol.Name, GetColumnDefaultValue(newCol)));
                    }
                }
            }
        }

        private string GetRemoveColumnCode(TableDefinition table, string colName)
        {
            return string.Format("Delete.Column(\"{0}\").FromTable(\"{1}\").InSchema(\"{2}\");", colName, table.Name, table.SchemaName);
        }

        private string GetColumnCode(ColumnDefinition column)
        {
            var sb = new StringBuilder();

            sb.AppendFormat(".WithColumn(\"{0}\").{1}", column.Name, GetMigrationTypeFunctionForType(column));

            if (column.IsIdentity) 
            {
                sb.Append(".Identity()");
            }

            if (column.IsPrimaryKey)
            {
                sb.AppendFormat(".PrimaryKey(\"{0}\")", column.PrimaryKeyName);
            }
            else if (column.IsUnique)
            {
                sb.AppendFormat(".Unique(\"{0}\")", column.IndexName);
            }
            else if (column.IsIndexed)
            {
                sb.AppendFormat(".Indexed(\"{0}\")", column.IndexName);
            }

            if (column.IsNullable.HasValue)
            {
                sb.Append(column.IsNullable.Value ? ".Nullable()" : ".NotNullable()");
            }

            if (column.DefaultValue != null && !column.IsIdentity)
            {
                sb.AppendFormat(".WithDefaultValue({0})", GetColumnDefaultValue(column));
            }

            //if (lastColumn) sb.Append(";");
            return sb.ToString();
        }

        private string GetMigrationTypeSize(DbType? type, int size)
        {
            if (size == -1) return "int.MaxValue";

            if (type == DbType.Binary && size == DbTypeSizes.ImageCapacity) return "DbTypeSizes.ImageCapacity";              // IMAGE fields
            if (type == DbType.AnsiString && size == DbTypeSizes.AnsiTextCapacity) return "DbTypeSizes.AnsiTextCapacity";    // TEXT fields
            if (type == DbType.String && size == DbTypeSizes.UnicodeTextCapacity) return "DbTypeSizes.UnicodeTextCapacity";  // NTEXT fields

            return size.ToString();
        }

        public string GetMigrationTypeFunctionForType(ColumnDefinition col)
        {
            var precision = col.Precision;
            string sizeStr = GetMigrationTypeSize(col.Type, col.Size);
            string precisionStr = (precision == -1) ? "" : "," + precision.ToString();
            string sysType = "AsString(" + sizeStr + ")";

            switch (col.Type)
            {
                case DbType.AnsiString:
                    if (options.UseDeprecatedTypes && col.Size == DbTypeSizes.AnsiTextCapacity)
                    {
                        sysType = "AsCustom(\"TEXT\")";
                    }
                    else
                    {
                        sysType = string.Format("AsAnsiString({0})", sizeStr);
                    }
                    break;
                case DbType.AnsiStringFixedLength:
                    sysType = string.Format("AsFixedLengthAnsiString({0})", sizeStr);
                    break;
                case DbType.String:
                    if (options.UseDeprecatedTypes && col.Size == DbTypeSizes.UnicodeTextCapacity)
                    {
                        sysType = "AsCustom(\"NTEXT\")";
                    }
                    else
                    {
                        sysType = string.Format("AsString({0})", sizeStr);
                    }
                    break;
                case DbType.StringFixedLength:
                    sysType = string.Format("AsFixedLengthString({0})", sizeStr);
                    break;
                case DbType.Binary:
                    if (options.UseDeprecatedTypes && col.Size == DbTypeSizes.ImageCapacity)
                    {
                        sysType = "AsCustom(\"IMAGE\")";
                    }
                    else
                    {
                        sysType = string.Format("AsBinary({0})", sizeStr);
                    }
                    break;
                case DbType.Boolean:
                    sysType = "AsBoolean()";
                    break;
                case DbType.Byte:
                    sysType = "AsByte()";
                    break;
                case DbType.Currency:
                    sysType = "AsCurrency()";
                    break;
                case DbType.Date:
                    sysType = "AsDate()";
                    break;
                case DbType.DateTime:
                    sysType = "AsDateTime()";
                    break;
                case DbType.Decimal:
                    sysType = string.Format("AsDecimal({0})", sizeStr + precisionStr);
                    break;
                case DbType.Double:
                    sysType = "AsDouble()";
                    break;
                case DbType.Guid:
                    sysType = "AsGuid()";
                    break;
                case DbType.Int16:
                case DbType.UInt16:
                    sysType = "AsInt16()";
                    break;
                case DbType.Int32:
                case DbType.UInt32:
                    sysType = "AsInt32()";
                    break;
                case DbType.Int64:
                case DbType.UInt64:
                    sysType = "AsInt64()";
                    break;
                case DbType.Single:
                    sysType = "AsFloat()";
                    break;
                case null:
                    sysType = string.Format("AsCustom({0})", col.CustomType);
                    break;
                default:
                    break;
            }

            return sysType;
        }

        public string GetColumnDefaultValue(ColumnDefinition col)
        {
            string sysType = null;
            string defValue = col.DefaultValue.ToString().CleanBracket().ToUpper().Trim();

            var guid = Guid.Empty;
            switch (col.Type)
            {
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Single:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                    sysType = defValue.Replace("'", "").Replace("\"", "").CleanBracket();
                    break;

                case DbType.Guid:
                    if (defValue == "NEWID()")
                    {
                        sysType = "SystemMethods.NewGuid";
                    } else if (defValue == "NEWSEQUENTIALID()")
                    {
                        sysType = "SystemMethods.NewSequentialId";
                    }
                    else if (defValue.IsGuid(out guid))
                    {
                        if (guid == Guid.Empty)
                        {
                            sysType = "Guid.Empty";
                        }
                        else
                        {
                            sysType = string.Format("new System.Guid(\"{0}\")", guid);
                        }
                    }
                    break;

                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.Date:
                    if (defValue == "CURRENT_TIME"
                        || defValue == "CURRENT_DATE"
                        || defValue == "CURRENT_TIMESTAMP"
                        || defValue == "GETDATE()")
                    {
                        sysType = "SystemMethods.CurrentDateTime";
                    }
                    else if (defValue == "GETUTCDATE()")
                    {
                        sysType = "SystemMethods.CurrentUTCDateTime";
                    }
                    else
                    {
                        sysType = "\"" + defValue + "\"";
                    }
                    break;

                default:
                    if (defValue == "CURRENT_USER")
                    {
                        sysType = "SystemMethods.CurrentUser";
                    }
                    else
                    {
                        sysType = string.Format("\"{0}\"", col.DefaultValue);
                    }
                    break;
            }

            return sysType.Replace("'", "''");
        }
        #endregion

        #region Index 

        private string GetRemoveIndexCode(TableDefinition table, string indexName)
        {
            return string.Format("Delete.Index(\"{0}\").OnTable(\"{1}\");", indexName, table.Name);
        }

        private IEnumerable<ForeignKeyDefinition> FindForeignKeysWithColumns(IEnumerable<ForeignKeyDefinition> fks, IEnumerable<IndexColumnDefinition> cols)
        {
            string[] colNames = cols.Select(col => col.Name).ToArray();
            return fks.Where(fk => fk.ForeignColumns.SequenceEqual(colNames));
        }

        private string GetCreateIndexCode(TableDefinition table, IndexDefinition index)
        {
            var sb = new StringBuilder();

            sb.AppendLine();

            if (table.ForeignKeys.Any())
            {
                bool isFkIndex = FindForeignKeysWithColumns(table.ForeignKeys, index.Columns).Any();
                if (isFkIndex) sb.Append("IfDatabase(\"sqlserver\").");
            }

            //Create.Index("ix_Name").OnTable("TestTable2").OnColumn("Name").Ascending().WithOptions().NonClustered();
            sb.AppendFormat("Create.Index(\"{0}\").OnTable(\"{1}\")", index.Name, index.TableName);

            if (index.IsUnique)
            {
                sb.AppendFormat(".WithOptions().Unique()");
            }

            if (index.IsClustered)
            {
                sb.AppendFormat(".WithOptions().Clustered()");
            }

            foreach (var col in index.Columns)
            {
                sb.AppendFormat("\n\t.OnColumn(\"{0}\")", col.Name);
                sb.AppendFormat(".{0}()", col.Direction.ToString());
            }

            sb.Append(";");

            return sb.ToString();
        }

        #endregion

        #region Foreign Key

        private string GetRemoveFKCode(TableDefinition table, string fkName)
        {
            return string.Format("Delete.ForeignKey(\"{0}\").OnTable(\"{1}\").InSchema(\"{2}\");", fkName, table.Name, table.SchemaName);
        }

        private string ToStringArray(IEnumerable<string> cols)
        {
            string strCols = String.Join(", ", cols.Select(col => '"' + col + '"').ToArray());
            return '{' + strCols + '}';
        }

        protected string GetCreateForeignKeyCode(ForeignKeyDefinition fk)
        {
            string result = Buffer(() => DoGetCreateForeignKeyCode(fk));
            return result.Substring(0, result.Length - Environment.NewLine.Length) + ";";
        }

        private void DoGetCreateForeignKeyCode(ForeignKeyDefinition fk)
        {
            //Create.ForeignKey("fk_TestTable2_TestTableId_TestTable_Id")
            //    .FromTable("TestTable2").ForeignColumn("TestTableId")
            //    .ToTable("TestTable").PrimaryColumn("Id");

            WriteLine();
            WriteLine("Create.ForeignKey(\"{0}\")", fk.Name);
            using (new Indenter())
            {
                // From Table
                string fromTable = string.Format(".FromTable(\"{0}\")", fk.ForeignTable);

                using (new Indenter())
                {
                    if (fk.ForeignColumns.Count == 1)
                    {
                        fromTable += string.Format(".ForeignColumn(\"{0}\")", fk.ForeignColumns.First());
                    }
                    else
                    {
                        fromTable += string.Format("ForeignColumns({0})", ToStringArray(fk.ForeignColumns));
                    }
                }

                WriteLine(fromTable);

                // To Table
                string toTable = string.Format(".ToTable(\"{0}\")", fk.PrimaryTable);

                using (new Indenter())
                {
                    if (fk.PrimaryColumns.Count == 1)
                    {
                        toTable += string.Format(".PrimaryColumn(\"{0}\")", fk.PrimaryColumns.First());
                    }
                    else
                    {
                        toTable += string.Format(".PrimaryColumns({0})", ToStringArray(fk.PrimaryColumns));
                    }
                }

                WriteLine(toTable);

                if (fk.OnDelete != Rule.None && fk.OnDelete == fk.OnUpdate)
                {
                    WriteLine(".OnDeleteOrUpdate(System.Data.Rule.{0})", fk.OnDelete);
                }
                else
                {
                    if (fk.OnDelete != Rule.None)
                    {
                        WriteLine(".OnDelete(System.Data.Rule.{0})", fk.OnDelete);
                    }

                    if (fk.OnUpdate != Rule.None)
                    {
                        WriteLine(".OnUpdate(System.Data.Rule.{0})", fk.OnUpdate);
                    }
                }
            }
        }

        #endregion
    }
}