#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.ComponentModel.DataAnnotations;
using System.IO;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to execute SQL scripts
    /// </summary>
    public class ExecuteSqlScriptExpression : ExecuteSqlScriptExpressionBase, IFileSystemExpression
    {
        private string _rootPath;
        private string _sqlScript;
        private string _unchangedSqlScript;

        /// <summary>
        /// Gets or sets the SQL script to be executed
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.SqlScriptCannotBeNullOrEmpty))]
        public string SqlScript
        {
            get => _sqlScript;
            set
            {
                _unchangedSqlScript =  value;
                UpdateSqlScript();
            }
        }

        /// <summary>
        /// Gets or sets the root path where the SQL script file should be loaded from
        /// </summary>
        public string RootPath
        {
            get => _rootPath;
            set
            {
                _rootPath = value;
                UpdateSqlScript();
            }
        }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            string sqlText;
            using (var reader = File.OpenText(SqlScript))
            {
                sqlText = reader.ReadToEnd();
            }

            Execute(processor, sqlText);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + SqlScript;
        }

        private void UpdateSqlScript()
        {
            if (!string.IsNullOrEmpty(_rootPath) && !string.IsNullOrEmpty(_unchangedSqlScript))
            {
                _sqlScript = Path.Combine(_rootPath, _unchangedSqlScript);
            }
            else
            {
                _sqlScript = _unchangedSqlScript;
            }
        }
    }
}
