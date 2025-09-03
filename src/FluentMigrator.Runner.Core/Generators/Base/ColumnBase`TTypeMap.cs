#region License
// Copyright (c) 2007-2018, Sean Chambers and the FluentMigrator Project
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

using FluentMigrator.Generation;

namespace FluentMigrator.Runner.Generators.Base
{
    /// <summary>
    /// The base class for column definitions
    /// </summary>
    public abstract class ColumnBase<TTypeMap> : ColumnBase
        where TTypeMap : ITypeMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase{TTypeMap}"/> class.
        /// </summary>
        /// <param name="typeMap">The type map</param>
        /// <param name="quoter">The quoter</param>
        protected ColumnBase(TTypeMap typeMap, IQuoter quoter) : base(typeMap, quoter)
        {
        }
    }
}
