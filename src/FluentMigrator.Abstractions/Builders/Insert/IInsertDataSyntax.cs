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

using System.Collections.Generic;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Insert
{
    /// <summary>
    /// Specify the data to insert
    /// </summary>
    public interface IInsertDataSyntax : IFluentSyntax
    {
        /// <summary>
        /// Specify the data to insert
        /// </summary>
        /// <param name="dataAsAnonymousType">An anonymous object that is used to insert data</param>
        /// <remarks>
        /// The properties are the column names and their values are the row values.
        /// </remarks>
        /// <returns>The next step</returns>
#if NET
        [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("The properties of the anonymous type cannot be statically analyzed.")]
#endif
        IInsertDataSyntax Row(object dataAsAnonymousType);

        /// <summary>
        /// Specify the data to insert
        /// </summary>
        /// <param name="data">The dictionary containing column name/value combinations</param>
        /// <returns>The next step</returns>
        IInsertDataSyntax Row(IDictionary<string, object> data);
    }
}
