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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete
{
    /// <summary>
    /// Specify the condition of the data to delete
    /// </summary>
    public interface IDeleteDataSyntax : IFluentSyntax
    {
        /// <summary>
        /// Define the condition of a row/multiple rows to delete
        /// </summary>
        /// <param name="dataAsAnonymousType">An anonymous type whose member names will be trated as column names and their values as values for the condition</param>
        /// <returns>The next step</returns>
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
        IDeleteDataSyntax Row(object dataAsAnonymousType);

        /// <summary>
        /// Specify that all rows should be deleted
        /// </summary>
        void AllRows();

        /// <summary>
        /// Specify that all rows having a <c>null</c> value in the given column should be deleted
        /// </summary>
        /// <param name="columnName">The column name</param>
        void IsNull(string columnName);
    }
}
