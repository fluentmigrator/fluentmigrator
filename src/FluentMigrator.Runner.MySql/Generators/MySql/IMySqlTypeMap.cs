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

namespace FluentMigrator.Runner.Generators.MySql
{
    /// <summary>
    /// Represents a specialized type map for MySQL databases, providing mappings between
    /// <see cref="System.Data.DbType"/> and MySQL-specific SQL types.
    /// </summary>
    /// <remarks>
    /// This interface extends <see cref="FluentMigrator.Runner.Generators.ITypeMap"/> to include
    /// MySQL-specific type mapping functionality. Implementations of this interface are used
    /// to define and retrieve the appropriate MySQL SQL type for a given <see cref="System.Data.DbType"/>.
    /// </remarks>
    public interface IMySqlTypeMap : ITypeMap
    {

    }
}
