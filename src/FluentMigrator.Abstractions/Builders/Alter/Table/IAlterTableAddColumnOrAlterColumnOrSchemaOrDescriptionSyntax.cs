#region License
// 
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator.Builders.Alter.Table
{
    /// <summary>
    /// Interface to change the description or alter the table/column/schema
    /// </summary>
    public interface IAlterTableAddColumnOrAlterColumnOrSchemaOrDescriptionSyntax : IAlterTableAddColumnOrAlterColumnOrSchemaSyntax
    {
        /// <summary>
        /// Set the description
        /// </summary>
        /// <param name="description">The description to set</param>
        /// <returns>Interface providing ways for other modifications</returns>
        IAlterTableAddColumnOrAlterColumnOrSchemaSyntax WithDescription(string description);
    }
}
