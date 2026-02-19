#region License
// Copyright (c) 2024, FluentMigrator Project
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

using System;

namespace FluentMigrator.Runner.Generators.Hana
{
    /// <summary>
    /// Represents a specialized type map for SAP HANA database, providing mappings
    /// between <see cref="System.Data.DbType"/> and HANA-specific SQL types.
    /// </summary>
    /// <remarks>
    /// This interface extends <see cref="FluentMigrator.Runner.Generators.ITypeMap"/> to
    /// include HANA-specific type mapping functionality.
    /// </remarks>
    [Obsolete("Hana support will go away unless someone in the community steps up to provide support.")]
    public interface IHanaTypeMap : ITypeMap
    {
        
    }
}
