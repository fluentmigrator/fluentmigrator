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

using System;

namespace FluentMigrator.Runner.VersionTableInfo
{
    /// <summary>
    /// An attribute used to mark a class as providing metadata for a version table.
    /// </summary>
    /// <remarks>
    /// Classes decorated with this attribute are expected to implement the
    /// <see cref="IVersionTableMetaData"/> interface. This attribute is utilized
    /// by the migration runner to identify and process version table metadata.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class VersionTableMetaDataAttribute : Attribute
    {
    }
}
