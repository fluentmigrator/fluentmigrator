#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace FluentMigrator.SqlProj
{
    /// <summary>
    /// Main class for generating FluentMigrator migrations from SQL Server Database Projects
    /// </summary>
    public class SqlProjMigrationGenerator
    {
        private readonly SqlProjModelExtractor _modelExtractor;
        private readonly MigrationCodeGenerator _codeGenerator;

        public SqlProjMigrationGenerator(string migrationNamespace = "Migrations")
        {
            _modelExtractor = new SqlProjModelExtractor();
            _codeGenerator = new MigrationCodeGenerator(migrationNamespace);
        }

        /// <summary>
        /// Generates a migration file from a .sqlproj file
        /// </summary>
        /// <param name="sqlProjPath">Path to the .sqlproj file</param>
        /// <param name="outputPath">Path where the migration file should be created</param>
        /// <param name="migrationName">Name for the migration class</param>
        /// <param name="version">Optional version number (defaults to timestamp)</param>
        /// <returns>Path to the generated migration file</returns>
        public string GenerateMigrationFromSqlProj(
            string sqlProjPath, 
            string outputPath, 
            string migrationName,
            long? version = null)
        {
            if (!File.Exists(sqlProjPath))
            {
                throw new FileNotFoundException($"SqlProj file not found: {sqlProjPath}");
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // Build model from sqlproj
            var model = _modelExtractor.BuildModelFromSqlProj(sqlProjPath);

            // Extract table definitions
            var tables = _modelExtractor.GetTables(model);

            if (tables.Count == 0)
            {
                throw new InvalidOperationException("No tables found in the SQL project");
            }

            // Generate version if not provided
            var migrationVersion = version ?? MigrationCodeGenerator.GenerateVersionNumber();

            // Generate migration code
            var migrationCode = _codeGenerator.GenerateMigration(tables, migrationName, migrationVersion);

            // Write to file
            var fileName = $"{migrationVersion}_{migrationName}.cs";
            var fullPath = Path.Combine(outputPath, fileName);
            File.WriteAllText(fullPath, migrationCode);

            return fullPath;
        }

        /// <summary>
        /// Generates migration code from a .sqlproj file and returns it as a string
        /// </summary>
        /// <param name="sqlProjPath">Path to the .sqlproj file</param>
        /// <param name="migrationName">Name for the migration class</param>
        /// <param name="version">Optional version number (defaults to timestamp)</param>
        /// <returns>Generated migration code as string</returns>
        public string GenerateMigrationCode(
            string sqlProjPath,
            string migrationName,
            long? version = null)
        {
            if (!File.Exists(sqlProjPath))
            {
                throw new FileNotFoundException($"SqlProj file not found: {sqlProjPath}");
            }

            // Build model from sqlproj
            var model = _modelExtractor.BuildModelFromSqlProj(sqlProjPath);

            // Extract table definitions
            var tables = _modelExtractor.GetTables(model);

            if (tables.Count == 0)
            {
                throw new InvalidOperationException("No tables found in the SQL project");
            }

            // Generate version if not provided
            var migrationVersion = version ?? MigrationCodeGenerator.GenerateVersionNumber();

            // Generate and return migration code
            return _codeGenerator.GenerateMigration(tables, migrationName, migrationVersion);
        }
    }
}
