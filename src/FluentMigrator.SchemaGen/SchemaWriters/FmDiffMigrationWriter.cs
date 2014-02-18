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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentMigrator.Runner;
using FluentMigrator.SchemaGen.Extensions;
using FluentMigrator.SchemaGen.Model;
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
        private readonly SqlFileWriter sqlFileWriter;

        private int step = 1;

        private readonly IList<string> classPaths = new List<string>();

        public FmDiffMigrationWriter(IOptions options, IAnnouncer announcer, IDbSchemaReader db1, IDbSchemaReader db2)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (announcer == null) throw new ArgumentNullException("announcer");
            if (db1 == null) throw new ArgumentNullException("db1");
            if (db2 == null) throw new ArgumentNullException("db2");

            this.options = options;
            this.announcer = announcer;
            this.sqlFileWriter = new SqlFileWriter(options, announcer);
            this.db1 = db1;
            this.db2 = db2;
        }

        public bool IsInstall
        {
            get { return db1 is EmptyDbSchemaReader; }
        }

        #region Helpers

        protected class Block : IDisposable
        {
            private readonly CodeLines lines;

            protected internal Block(CodeLines lines)
            {
                this.lines = lines;
                lines.WriteLine("{");
                lines.Indent();
            }

            public void Dispose()
            {
                lines.Indent(-1);
                lines.WriteLine("}");
            }
        }

        #endregion

        #region Emit Classes

        /// <summary>
        /// Writes migration classes.  Main entry point for this class.
        /// </summary>
        /// <returns>List of generated class file names</returns>
        public IEnumerable<string> WriteMigrationClasses()
        {
            step = options.StepStart;

            WriteClass("Initial", () => WriteComment("Sets initial version to " + options.MigrationVersion + "." + step));

            // TODO: Create new user defined DataTypes

            if (options.PreScripts)
            {
                WriteSqlScriptClass("01_Pre");
            }

            // Create/Update All tables/columns/indexes/foreign keys
            CreateUpdateTables(IsInstall ? "02_Install" : "03_Upgrade");

            // TODO: Drop/Create new or modified scripts (SPs/Views/Functions)
            // CreateUpdateScripts();

            if (options.PostScripts)
            {
                WriteSqlScriptClass("04_Post");
            }

            if (options.DropTables)
            {
                // Drop tables in order of their FK dependency.
                WriteClass("DropTables", DropTables, CantUndo);
            }

            if (options.DropScripts)
            {
                // Drop old SPs/Views/Functions
                WriteClass("DropScripts", DropScripts, CantUndo);
            }

            if (options.PostScripts)
            {
                WriteSqlScriptClass("05_Functions");
                WriteSqlScriptClass("06_Views");
                WriteSqlScriptClass("07_SPs");

                WriteSqlScriptClass("10_SeedData");
                WriteSqlScriptClass("11_DemoData", "Demo");
                WriteSqlScriptClass("12_TestData", "Test");
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

            WriteClass("Final", () => WriteComment("Sets final version to " + options.MigrationVersion + "." + step));

            return classPaths;
        }

        /// <summary>
        /// Writes a Migrator class.
        /// Only creates the class file if the <paramref name="upMethod"/> emits code.
        /// </summary>
        /// <param name="className">Migration class name</param>
        /// <param name="upMethod">Action to emit Up() method code</param>
        /// <param name="downMethod">
        /// Optional Action to emit Down() method code.
        /// If null, the class inherits from AutoReversingMigrationExt, otherwise it inherits from MigrationExt.
        /// These are project classes that inherit from AutoReversingMigration and Migration.
        /// </param>
        /// <param name="addTags">Additional FluentMigrator Tags applied to the class</param> 
        // Main Entry point
        protected CodeLines WriteClass(string className, Func<CodeLines> upMethod, Func<CodeLines> downMethod = null, string addTags = null)
        {
            // If no code is generated for an Up() method => No class is emitted
            var upMethodCode = upMethod();
            if (!upMethodCode.Any()) return upMethodCode;

            var codeLines = new CodeLines();

            // Prefix class with zero filled order number.
            className = string.Format("M{0,4:D4}_{1}", step, className);

            string fullDirName = options.OutputDirectory;

            new DirectoryInfo(fullDirName).Create();

            string classPath = Path.Combine(fullDirName, className + ".cs");
            announcer.Say(classPath);

            classPaths.Add(classPath);

            try
            {
                codeLines.WriteLine("using System;");
                codeLines.WriteLine("using System.Collections.Generic;");
                codeLines.WriteLine("using System.Linq;");
                codeLines.WriteLine("using System.Web;");
                codeLines.WriteLine("using FluentMigrator;");
                codeLines.WriteLine("using Migrations.FM_Extensions;");

                codeLines.WriteLine();

                string ns = options.NameSpace;
                codeLines.WriteLine("namespace {0}", ns);

                using (new Block(codeLines)) // namespace {}
                {
                    codeLines.WriteLine("[MigrationVersion({0})] // {1}",
                        options.MigrationVersion.Replace(".", ", ") + ", " + step,
                        GetMigrationNumber(options.MigrationVersion, step));

                    string tags = options.Tags ?? "" + addTags ?? "";
                    if (!string.IsNullOrEmpty(tags))
                    {
                        codeLines.WriteLine("[Tags(\"{0}\")]", tags.Replace(",", "\", \""));
                    }

                    codeLines.WriteLine("public class {0} : {1}", className, downMethod == null ? "AutoReversingMigrationExt" : "MigrationExt");
                    using (new Block(codeLines)) // class {}
                    {
                        codeLines.WriteLine();
                        codeLines.WriteLine("public override void Up()");
                        using (new Block(codeLines))
                        {
                            codeLines.WriteLines(upMethodCode);
                        }

                        if (downMethod != null)
                        {
                            codeLines.WriteLine();
                            codeLines.WriteLine("public override void Down()");
                            using (new Block(codeLines))
                            {
                                codeLines.WriteLines(downMethod());
                            }
                        }
                    }
                }

                step++;

                using (var fs = new FileStream(classPath, FileMode.Create))
                using (var writer = new StreamWriter(fs))
                {
                    foreach (string line in codeLines)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                announcer.Error(classPath + ": Failed to write class file");
                for (; ex != null; ex = ex.InnerException) announcer.Error(ex.Message);
            }

            return codeLines;
        }

        private CodeLines CantUndo()
        {
            return new CodeLines("throw new Exception(\"Cannot undo this database upgrade\");");
        }

        /// <summary>
        /// Computes the internal migration number saved in the database in VerionInfo table.
        /// </summary>
        /// <param name="major">Product major version</param>
        /// <param name="minor">Product minor version</param>
        /// <param name="patch">Product patch version</param>
        /// <param name="step">Step number generated by SchemaGen.</param>
        /// <returns>Migration number saved in the database in VerionInfo table</returns>
        private long GetMigrationNumber(int major, int minor, int patch, int step)
        {
            // This formula must match the one used in MigrationVersionAttribute class
            return ((((major * 100L) + minor) * 100L) + patch) * 1000L + step;
        }

        /// <summary>
        /// Computes the internal migration number saved in the database in VerionInfo table.
        /// </summary>
        /// <param name="version">major.minor.patch</param>
        /// <param name="step"></param>
        /// <returns>Migration number saved in the database in VerionInfo table</returns>
        private long GetMigrationNumber(string version, int step)
        {
            // Can throw exceptions if version format is invalid.
            int[] parts = version.Split('.').Select(int.Parse).ToArray();

            int major = parts.Length >= 1 ? parts[0] : 0;
            int minor = parts.Length >= 2 ? parts[1] : 0;
            int patch = parts.Length >= 3 ? parts[2] : 0;

            return GetMigrationNumber(major, minor, patch, step);
        }

        #endregion

        private CodeLines WriteComment(string comment)
        {
           return new CodeLines("// " + comment);
        }

        private void WriteSqlScriptClass(string subfolder, string tags = null)
        {
            var sqlDir = new DirectoryInfo(sqlFileWriter.GetSqlDirectoryPath(subfolder));
            if (!sqlDir.Exists)
            {
                announcer.Say(sqlDir.FullName + ": Did not find SQL script folder");
                return;
            }

            Func<CodeLines> upMethod = () =>
                {
                    var lines = new CodeLines();
                    if (options.SqlDirectory != null)
                    {
                        sqlFileWriter.ExecuteSqlDirectory(lines, sqlDir);
                    }
                    return lines;
                };

            WriteClass(subfolder.Replace(" ",""), upMethod, CantUndo, tags);
        }

        #region Drop Tables and Scripted Objects
        private CodeLines DropTables()
        {
            var lines = new CodeLines();
            
            // TODO: Currently ignoring Schema name for table objects.
            var db1FkOrder = db1.TablesInForeignKeyOrder(false); // descending order

            var removedTableNames = db1.Tables.Keys.Except(db2.Tables.Keys).ToList();
            removedTableNames = removedTableNames.OrderBy(t => -db1FkOrder[t]).ToList();

            foreach (TableDefinitionExt table in removedTableNames.Select(name => db1.Tables[name]))
            {
                foreach (ForeignKeyDefinitionExt fk in table.ForeignKeys)
                {
                    lines.WriteLine(fk.GetDeleteForeignKeyCode());
                }

                lines.WriteLine(table.GetDeleteCode());
            }

            return lines;
        }

        private CodeLines DropScripts()
        {
            var lines = new CodeLines();

            foreach (var name in db1.StoredProcedures.Except(db2.StoredProcedures))
            {
                lines.WriteLine("DeleteStoredProcedure(\"{0}\");", name);
            }

            foreach (var name in db1.Views.Except(db2.Views))
            {
                lines.WriteLine("DeleteView(\"{0}\");", name);
            }

            foreach (var name in db1.UserDefinedFunctions.Except(db2.UserDefinedFunctions))
            {
                lines.WriteLine("DeleteFunction(\"{0}\");", name);
            }

            foreach (var name in db1.UserDefinedDataTypes.Except(db2.UserDefinedDataTypes))
            {
                lines.WriteLine("DeleteType(\"{0}\");", name);
            }

            return lines;
        }
        #endregion

        #region Create / Update Tables
        private void CreateUpdateTables(string schemaSqlFolder)
        {
            var db1Tables = db1.Tables;

            // TODO: Currently ignoring Schema name for table objects.

            var db2FkOrder = db2.TablesInForeignKeyOrder(true);
            var db2TablesInFkOrder = db2.Tables.Values.OrderBy(tableDef => db2FkOrder[tableDef.Name]);

            foreach (TableDefinitionExt table in db2TablesInFkOrder)
            {
                TableDefinitionExt newTable = table;

                if (db1Tables.ContainsKey(newTable.Name))
                {
                    TableDefinitionExt oldTable = db1Tables[newTable.Name];
                    WriteClass("Update_" + table.Name, () => UpdateTable(oldTable, newTable, schemaSqlFolder));
                }
                else
                {
                    WriteClass("Create_" + table.Name, () => newTable.GetCreateCode());
                }
            }
        }

        /// <summary>
        /// Gets the set of tables indexes that are not declared as part of a table column definition
        /// </summary>
        /// <summary>
        /// Generate code based on changes to table columns, indexes and foreign keys
        /// </summary>
        /// <param name="oldTable"></param>
        /// <param name="newTable"></param>
        /// <param name="schemaSqlFolder"></param>
        private CodeLines UpdateTable(TableDefinitionExt oldTable, TableDefinitionExt newTable, string schemaSqlFolder)
        {
            var lines = new CodeLines();

            // Identify indexes containing fields that have changed type so they get included as Updated indexes.
            var colsDiff = new ModelDiff<ColumnDefinitionExt>(oldTable.Columns, newTable.Columns);
            var updatedCols = colsDiff.GetUpdatedNew();
            foreach (var index in FindTypeChangedIndexes(newTable.Indexes, updatedCols))
            {
                index.TypeChanged = true;
            }

            // GetNonColumnIndexes(): Single column Primary indexes are declared with the column and not explicitly named (so we exclude them from the comparison)
            var ixDiff = new ModelDiff<IndexDefinitionExt>(oldTable.GetNonColumnIndexes(), newTable.GetNonColumnIndexes());

            var fkDiff = new ModelDiff<ForeignKeyDefinitionExt>(oldTable.ForeignKeys, newTable.ForeignKeys);

            if (options.ShowChanges)
            {
                // Show renamed Indexes and Foreign Keys as comments
                ShowRenamedObjects(lines, "Index", ixDiff.GetRenamed());               
                ShowRenamedObjects(lines, "Foreign Key", fkDiff.GetRenamed());
            }

            // When a column becomes NOT NULL and has a DEFAULT value, this emits SQL to set the default on all NULL column values.
            SetDefaultsIfNotNull(lines, colsDiff);

            RemoveObjects(lines, fkDiff.GetRemovedOrUpdated().Cast<ICodeComparable>()); // Remove OLD / UPDATED foriegn keys.
            RemoveObjects(lines, ixDiff.GetRemovedOrUpdated().Cast<ICodeComparable>()); // Remove OLD / UPDATED indexes.

            var updatedColOldCode = colsDiff.GetUpdatedOld().Select(col => col.CreateCode); // Show old col defn as comments
            var updatedColsCode = colsDiff.GetUpdatedNew().Select(colCode => colCode.CreateCode.Replace("WithColumn", "AlterColumn")); 

            newTable.GetAlterTableCode(lines, updatedColsCode, updatedColOldCode);   // UPDATED columns (including 1 column indexes)

            var addedColsCode = colsDiff.GetAdded().Select(colCode => colCode.CreateCode.Replace("WithColumn", "AddColumn"));
            newTable.GetAlterTableCode(lines, addedColsCode);                       // Add NEW columns

            AddObjects(lines, ixDiff.GetAdded().Cast<ICodeComparable>());           // Add NEW Indexes
            AddObjects(lines, fkDiff.GetAdded().Cast<ICodeComparable>());           // Add NEW foreign keys

            // Note: The developer may inject custom data migration code here
            // We preserve old columns and indexes for this phase.
            sqlFileWriter.MigrateData(lines, schemaSqlFolder, newTable.Name);       // Run data migration SQL 

            AddObjects(lines, fkDiff.GetUpdatedNew().Cast<ICodeComparable>());      // Add UPDATED foreign keys
            AddObjects(lines, ixDiff.GetUpdatedNew().Cast<ICodeComparable>());      // Add UPDATED indexes (excluding 1 column indexes)

            RemoveObjects(lines, colsDiff.GetRemoved().Cast<ICodeComparable>());    // Remove OLD columns (kept for DataMigration).

            return lines;
        }

        private IEnumerable<IndexDefinitionExt> FindTypeChangedIndexes(IEnumerable<IndexDefinitionExt> indexes, IEnumerable<ColumnDefinitionExt> updatedCols)
        {
            var updatedColNames = updatedCols.Select(col => col.Name);

            // Find indexes containing columns where the type has column has changed
            return from index in indexes
                   let colNames = index.Columns.Select(col => col.Name)
                   where colNames.Any(updatedColNames.Contains)
                   select index;
        }

        /// <summary>
        /// Report renamed objects.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lines"></param>
        /// <param name="title"></param>
        /// <param name="renamed"></param>
        private void ShowRenamedObjects(CodeLines lines, string title, IEnumerable<KeyValuePair<string, string>> renamed)
        {
            foreach (var rename in renamed)
            {
                string msg = string.Format("Renamed {0}: {1} -> {2}", title, rename.Key, rename.Value);
                lines.WriteComment(msg);
                announcer.Emphasize(msg);
            }
        }

        private void AddObjects(CodeLines lines, IEnumerable<ICodeComparable> objs)
        {
            bool isFirst = true;
            foreach (var obj in objs)
            {
                if (isFirst)
                {
                    isFirst = false;
                    lines.WriteLine();
                }

                lines.WriteSplitLine(obj.CreateCode);
            }
        }

        private void RemoveObjects(CodeLines lines, IEnumerable<ICodeComparable> objs)
        {
            foreach (var obj in objs)
            {
                lines.WriteLine();
                if (options.ShowChanges)
                {
                    // Show old definition of object as a comment
                    lines.WriteComment(obj.CreateCode);
                }
                lines.WriteLine(obj.DeleteCode);
            }
        }

        #endregion

        #region Column

        /// <summary>
        /// When a column NULL -> NOT NULL and has a default value, this emits SQL to set the default on all NULL column values
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="colsDiff"></param>
        private void SetDefaultsIfNotNull(CodeLines lines,  ModelDiff<ColumnDefinitionExt> colsDiff)
        {
            if (options.SetNotNullDefault)
            {
                // When a column NULL -> NOT NULL and has a default value, this emits SQL to set the default on all NULL column values
                foreach (ColumnDefinitionExt newCol in colsDiff.GetUpdatedNew())
                {
                    ColumnDefinitionExt oldCol = colsDiff.GetOldObject(newCol.Name);
                    if (oldCol.IsNullable == true && newCol.IsNullable == false && newCol.DefaultValue != null)
                    {
                        lines.WriteLines(sqlFileWriter.EmbedSql(string.Format("UPDATE {0}.{1} SET {2} = {3} WHERE {2} IS NULL",
                            newCol.SchemaName, newCol.TableName, 
                            newCol.Name, newCol.GetColumnDefaultValue())));
                    }
                }
            }
        }

        #endregion
    }
}