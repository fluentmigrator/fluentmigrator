#region License
// Copyright (c) 2021, Fluent Migrator Project
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

namespace FluentMigrator.Builder.Create.Index
{
    /// <summary>
    /// BRIN index options
    /// </summary>
    public interface ICreateBrinIndexOptionsSyntax : ICreateIndexMethodOptionsSyntax
    {
        /// <inheritdoc cref="ICreateIndexMethodOptionsSyntax.Fillfactor"/>
        new ICreateBrinIndexOptionsSyntax Fillfactor(int fillfactor);

        /// <summary>
        /// Defines the number of table blocks that make up one block range for each entry of a BRIN index.
        /// For more information about it see: https://www.postgresql.org/docs/current/brin-intro.html
        /// </summary>
        /// <param name="range">The page per range</param>
        /// <returns>The next step</returns>
        ICreateBrinIndexOptionsSyntax PagesPerRange(int range);

        /// <summary>
        /// Defines whether a summarization run is invoked for the previous page range whenever an insertion is detected on the next one.
        /// </summary>
        /// <returns>The next step</returns>
        ICreateBrinIndexOptionsSyntax Autosummarize();

        /// <summary>
        /// Disable the a summarization that run is invoked for the previous page range whenever an insertion is detected on the next one.
        /// </summary>
        /// <returns>The next step</returns>
        ICreateBrinIndexOptionsSyntax DisableAutosummarize();

        /// <summary>
        /// Defines whether a summarization run is invoked for the previous page range whenever an insertion is detected on the next one.
        /// </summary>
        /// <param name="autosummarize">True to enable fast autosummarize or false to disable.</param>
        /// <returns>The next step</returns>
        ICreateBrinIndexOptionsSyntax Autosummarize(bool autosummarize);
    }
}
