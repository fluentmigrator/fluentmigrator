#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Infrastructure;

namespace FluentMigrator.MySql
{
    /// <summary>
    /// Feature extensions for MySql
    /// </summary>
    public static partial class MySqlExtensions
    {
        /// <summary>
        /// Unique key for additional feature (<see cref="ISupportAdditionalFeatures"/>) to customize MySQL indexes with an index type.
        /// </summary>
        public const string IndexType = "MySqlIndexType";

        private static string UnsupportedMethodMessage(string methodName, string interfaceName)
            => string.Format(ErrorMessages.MethodXMustBeCalledOnObjectImplementingY, methodName, interfaceName);
    }
}
