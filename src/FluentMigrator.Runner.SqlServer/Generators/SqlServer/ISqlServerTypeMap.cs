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

namespace FluentMigrator.Runner.Generators.SqlServer
{
    /// <summary>
    /// Represents a specialized type map for SQL Server, extending the functionality of <see cref="FluentMigrator.Runner.Generators.ITypeMap"/>.
    /// </summary>
    /// <remarks>
    /// This interface provides SQL Server-specific mappings for database types, enabling precise control over type conversions
    /// and compatibility with various SQL Server versions.
    /// </remarks>
    public interface ISqlServerTypeMap : ITypeMap { }
}
