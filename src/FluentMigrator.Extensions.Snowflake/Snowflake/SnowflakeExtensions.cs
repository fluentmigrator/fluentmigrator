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

namespace FluentMigrator.Snowflake
{
    /// <summary>
    /// Provides extension methods and constants for working with Snowflake-specific features in FluentMigrator.
    /// </summary>
    /// <remarks>
    /// This class includes predefined constants for Snowflake identity column configurations, such as 
    /// <see cref="IdentitySeed"/> and <see cref="IdentityIncrement"/>, which can be used to specify the 
    /// seed and increment values for identity columns in Snowflake.
    /// </remarks>
    public static partial class SnowflakeExtensions
    {
        /// <summary>
        /// Represents the key used to specify the seed value for an identity column in Snowflake.
        /// </summary>
        /// <remarks>
        /// This constant is utilized in conjunction with the <see cref="ISupportAdditionalFeatures"/> interface
        /// to define the starting value of an identity column when creating or modifying tables in Snowflake.
        /// </remarks>
        public static readonly string IdentitySeed = "SnowflakeIdentitySeed";
        /// <summary>
        /// Represents the predefined constant for specifying the increment value of an identity column
        /// in Snowflake when using FluentMigrator.
        /// </summary>
        /// <remarks>
        /// This constant is used as a key in the <see cref="FluentMigrator.Model.ColumnDefinition.AdditionalFeatures"/> 
        /// dictionary to define the increment value for Snowflake identity columns.
        /// </remarks>
        public static readonly string IdentityIncrement = "SnowflakeIdentityIncrement";

        private static string UnsupportedMethodMessage(object methodName, string interfaceName)
        {
            var msg = string.Format(ErrorMessages.MethodXMustBeCalledOnObjectImplementingY, methodName, interfaceName);
            return msg;
        }
    }
}
