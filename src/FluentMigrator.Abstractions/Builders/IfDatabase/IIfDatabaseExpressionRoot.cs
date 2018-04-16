#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2011, Grant Archibald
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

using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Delete;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Builders.Update;

namespace FluentMigrator.Builders.IfDatabase
{
    /// <summary>
    /// Defines fluent expressions that can be conditionally executed
    /// </summary>
    public interface IIfDatabaseExpressionRoot
    {
        /// <summary>
        /// Creates an ALTER expression
        /// </summary>
        IAlterExpressionRoot Alter { get; }

        /// <summary>
        /// Creates CREATE expression
        /// </summary>
        ICreateExpressionRoot Create { get; }

        /// <summary>
        /// Creates a DELETE expression
        /// </summary>
        IDeleteExpressionRoot Delete { get; }

        /// <summary>
        /// Renames a database object
        /// </summary>
        IRenameExpressionRoot Rename { get; }

        /// <summary>
        /// Inserts data into a table
        /// </summary>
        IInsertExpressionRoot Insert { get; }

        /// <summary>
        /// Execute some SQL
        /// </summary>
        IExecuteExpressionRoot Execute { get; }

        /// <summary>
        /// Check if a database object exists
        /// </summary>
        ISchemaExpressionRoot Schema { get; }

        /// <summary>
        /// Updates data in a table
        /// </summary>
        IUpdateExpressionRoot Update { get; }

        /// <summary>
        /// Invokes a delegate that can be used to create database specific expressions
        /// </summary>
        /// <param name="delegation">The delegate to call if the database type matches</param>
        void Delegate(Action delegation);
    }
}
