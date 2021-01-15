#region License
// Copyright (c) 2021, FluentMigrator Project
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

using FluentMigrator.Builders.Create.Index;

namespace FluentMigrator.Builder.Create.Index
{

    /// <summary>
    /// B-tree index options
    /// </summary>
    public interface ICreateIndexMethodOptionsSyntax : ICreateIndexOptionsSyntax
    {
        /// <summary>
        /// The fillfactor for an index is a percentage that determines how full the index method will try to pack index pages.
        /// </summary>
        /// <param name="fillfactor">The fillfactor value from 10 to 100 can be selected</param>
        /// <returns>The next step</returns>
        /// <remarks>
        /// For B-trees, leaf pages are filled to this percentage during initial index build, and also when extending
        /// the index at the right (adding new largest key values). If pages subsequently become completely full,
        /// they will be split, leading to gradual degradation in the index's efficiency.
        /// B-trees use a default fillfactor of 90, but any integer value from 10 to 100 can be selected.
        /// If the table is static then fillfactor 100 is best to minimize the index's physical size,
        /// but for heavily updated tables a smaller fillfactor is better to minimize the need for page splits.
        /// The other index methods use fillfactor in different but roughly analogous ways; the default fillfactor varies between methods.
        /// </remarks>
        ICreateIndexMethodOptionsSyntax Fillfactor(int fillfactor);
    }
}
