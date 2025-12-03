#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using System.Collections.Generic;
using System.Text;

namespace FluentMigrator.Runner.Processors.Oracle
{
    /// <summary>
    /// Provides parsing utilities for Oracle SQL scripts.
    /// </summary>
    internal static class OracleSqlParser
    {
        /// <summary>
        /// Splits a SQL script into individual statements, taking into account Oracle-specific syntax rules.
        /// </summary>
        /// <param name="sqlScript">The SQL script to split.</param>
        /// <returns>A list of individual SQL statements.</returns>
        internal static List<string> SplitOracleSqlStatements(string sqlScript)
        {
            var statements = new List<string>();
            var currentStatement = new StringBuilder();
            var inString = false;
            var inIdentifier = false;
            var inSingleLineComment = false;
            var inMultiLineComment = false;
            var prevChar = '\0';
            var plsqlBlockDepth = 0; // Track nested BEGIN/END blocks

            for (var i = 0; i < sqlScript.Length; i++)
            {
                var c = sqlScript[i];

                if (inSingleLineComment)
                {
                    currentStatement.Append(c);
                    if (c == '\n')
                    {
                        inSingleLineComment = false;
                    }
                    prevChar = c;
                    continue;
                }

                if (inMultiLineComment)
                {
                    currentStatement.Append(c);
                    if (prevChar == '*' && c == '/')
                    {
                        inMultiLineComment = false;
                    }
                    prevChar = c;
                    continue;
                }

                if (inString)
                {
                    currentStatement.Append(c);
                    if (c == '\'' && prevChar != '\\')
                    {
                        inString = false;
                    }
                    prevChar = c;
                    continue;
                }

                if (inIdentifier)
                {
                    currentStatement.Append(c);
                    if (c == '"')
                    {
                        inIdentifier = false;
                    }
                    prevChar = c;
                    continue;
                }

                // Check for comment start
                if (c == '-' && prevChar == '-')
                {
                    inSingleLineComment = true;
                    currentStatement.Append(c);
                    prevChar = c;
                    continue;
                }

                if (c == '*' && prevChar == '/')
                {
                    inMultiLineComment = true;
                    currentStatement.Append(c);
                    prevChar = c;
                    continue;
                }

                // Check for string start
                if (c == '\'')
                {
                    inString = true;
                    currentStatement.Append(c);
                    prevChar = c;
                    continue;
                }

                // Check for identifier start
                if (c == '"')
                {
                    inIdentifier = true;
                    currentStatement.Append(c);
                    prevChar = c;
                    continue;
                }

                // Check for BEGIN, DECLARE, or END keywords (PL/SQL block markers)
                if (char.IsLetter(c) || c == '_')
                {
                    var keyword = new StringBuilder();
                    while (i < sqlScript.Length && (char.IsLetterOrDigit(sqlScript[i]) || sqlScript[i] == '_'))
                    {
                        keyword.Append(sqlScript[i]);
                        i++;
                    }
                    i--; // Step back since the outer loop will increment

                    var keywordStr = keyword.ToString().ToUpperInvariant();
                    
                    // Check if this is the start of a PL/SQL block
                    if (keywordStr == "BEGIN" || keywordStr == "DECLARE")
                    {
                        plsqlBlockDepth++;
                    }
                    else if (keywordStr == "END")
                    {
                        // Check if END is followed by a control structure keyword (IF, LOOP, CASE, etc.)
                        // If so, it's not a block terminator
                        var j = i + 1;
                        while (j < sqlScript.Length && char.IsWhiteSpace(sqlScript[j]))
                        {
                            j++;
                        }
                        
                        var isControlStructureEnd = false;
                        if (j < sqlScript.Length && (char.IsLetter(sqlScript[j]) || sqlScript[j] == '_'))
                        {
                            var nextKeyword = new StringBuilder();
                            while (j < sqlScript.Length && (char.IsLetterOrDigit(sqlScript[j]) || sqlScript[j] == '_'))
                            {
                                nextKeyword.Append(sqlScript[j]);
                                j++;
                            }
                            
                            var nextKeywordStr = nextKeyword.ToString().ToUpperInvariant();
                            // END IF, END LOOP, END CASE, END WHILE, END FOR are control structure terminators, not block terminators
                            if (nextKeywordStr == "IF" || nextKeywordStr == "LOOP" || nextKeywordStr == "CASE" || 
                                nextKeywordStr == "WHILE" || nextKeywordStr == "FOR")
                            {
                                isControlStructureEnd = true;
                            }
                        }
                        
                        if (!isControlStructureEnd && plsqlBlockDepth > 0)
                        {
                            plsqlBlockDepth--;
                            
                            // If we just closed the last block, look ahead for semicolon
                            // and include it as part of the block
                            if (plsqlBlockDepth == 0)
                            {
                                currentStatement.Append(keyword);
                                
                                // Look ahead to find the semicolon
                                // Can have: END; or END <name>; or END\n; etc.
                                j = i + 1;
                                var foundContent = new StringBuilder();
                                var foundSemicolon = false;
                                
                                // Skip whitespace and collect optional identifier (procedure/function name)
                                while (j < sqlScript.Length)
                                {
                                    var ch = sqlScript[j];
                                    
                                    if (ch == ';')
                                    {
                                        // Found the semicolon - include everything up to and including it
                                        currentStatement.Append(foundContent);
                                        currentStatement.Append(';');
                                        i = j; // Move past the semicolon
                                        foundSemicolon = true;
                                        
                                        // Add the complete PL/SQL block
                                        var statement = currentStatement.ToString().Trim();
                                        if (!string.IsNullOrWhiteSpace(statement))
                                        {
                                            statements.Add(statement);
                                        }
                                        currentStatement.Clear();
                                        prevChar = ';';
                                        break;
                                    }
                                    else if (char.IsWhiteSpace(ch) || char.IsLetterOrDigit(ch) || ch == '_')
                                    {
                                        // Collect whitespace and identifier characters
                                        foundContent.Append(ch);
                                        j++;
                                    }
                                    else
                                    {
                                        // Found something else - append what we collected and continue normally
                                        // This handles cases where END is not followed by a semicolon or identifier
                                        currentStatement.Append(foundContent);
                                        break;
                                    }
                                }
                                
                                if (foundSemicolon)
                                {
                                    continue;
                                }
                                
                                prevChar = keyword.Length > 0 ? keyword[keyword.Length - 1] : '\0';
                                continue;
                            }
                        }
                    }
                    
                    currentStatement.Append(keyword);
                    prevChar = keyword.Length > 0 ? keyword[keyword.Length - 1] : '\0';
                    continue;
                }

                // Check for statement terminator
                if (c == ';')
                {
                    currentStatement.Append(c);
                    
                    // Only split on semicolon if we're not inside a PL/SQL block
                    if (plsqlBlockDepth == 0)
                    {
                        var statement = currentStatement.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(statement))
                        {
                            // Remove trailing semicolon for regular SQL statements
                            statement = statement.TrimEnd(';').Trim();
                            if (!string.IsNullOrWhiteSpace(statement))
                            {
                                statements.Add(statement);
                            }
                        }
                        currentStatement.Clear();
                        prevChar = '\0';
                    }
                    else
                    {
                        prevChar = c;
                    }
                    continue;
                }

                // Default case: append character
                currentStatement.Append(c);
                prevChar = c;
            }

            // Add any remaining content
            var remainingStatement = currentStatement.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(remainingStatement))
            {
                statements.Add(remainingStatement);
            }

            return statements;
        }
    }
}
