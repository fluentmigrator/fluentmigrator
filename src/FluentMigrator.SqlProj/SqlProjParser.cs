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

using System.Xml.Linq;

namespace FluentMigrator.SqlProj
{
    /// <summary>
    /// Parses SQL Server Database Project (.sqlproj) files to extract SQL script references
    /// </summary>
    public class SqlProjParser
    {
        /// <summary>
        /// Parses a .sqlproj file and returns the list of SQL script file paths
        /// </summary>
        /// <param name="sqlProjPath">Path to the .sqlproj file</param>
        /// <returns>List of SQL script file paths relative to the project directory</returns>
        public IEnumerable<string> ParseSqlProjFile(string sqlProjPath)
        {
            if (!File.Exists(sqlProjPath))
            {
                throw new FileNotFoundException($"SqlProj file not found: {sqlProjPath}");
            }

            var projectDirectory = Path.GetDirectoryName(sqlProjPath) ?? throw new InvalidOperationException($"Unable to determine project directory for {sqlProjPath}");
            var document = XDocument.Load(sqlProjPath);
            
            // Handle namespace for MSBuild project files
            var ns = document.Root?.Name.Namespace ?? XNamespace.None;
            
            var buildItems = document.Descendants(ns + "Build")
                .Concat(document.Descendants(ns + "None"))
                .Concat(document.Descendants(ns + "Content"))
                .Where(e => e.Attribute("Include") != null);

            var sqlFiles = new List<string>();
            
            foreach (var item in buildItems)
            {
                var includePath = item.Attribute("Include")?.Value;
                if (!string.IsNullOrEmpty(includePath) && includePath.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                {
                    // Normalize path separators for cross-platform compatibility
                    var normalizedPath = includePath.Replace('\\', Path.DirectorySeparatorChar);
                    var fullPath = Path.Combine(projectDirectory, normalizedPath);
                    if (File.Exists(fullPath))
                    {
                        sqlFiles.Add(fullPath);
                    }
                }
            }

            return sqlFiles;
        }

        /// <summary>
        /// Reads SQL content from multiple SQL files
        /// </summary>
        /// <param name="sqlFilePaths">Paths to SQL files</param>
        /// <returns>Dictionary mapping file paths to their SQL content</returns>
        public Dictionary<string, string> ReadSqlFiles(IEnumerable<string> sqlFilePaths)
        {
            var sqlContents = new Dictionary<string, string>();
            
            foreach (var filePath in sqlFilePaths)
            {
                if (File.Exists(filePath))
                {
                    sqlContents[filePath] = File.ReadAllText(filePath);
                }
            }

            return sqlContents;
        }
    }
}
