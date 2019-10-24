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

using System;

using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

using Microsoft.Extensions.Options;

#pragma warning disable 3005
namespace FluentMigrator.Tests.Unit
{
    [VersionTableMetaData]
    public class TestVersionTableMetaData : IVersionTableMetaData
    {
        public const string TABLE_NAME = "testVersionTableName";
        public const string COLUMN_NAME = "testColumnName";
        public const string UNIQUE_INDEX_NAME = "testUniqueIndexName";
        public const string DESCRIPTION_COLUMN_NAME = "testDescriptionColumnName";
        public const string APPLIED_ON_COLUMN_NAME = "testAppliedOnColumnName";

        /// <summary>
        /// Initializes a new instance of the <see cref="TestVersionTableMetaData"/> class.
        /// </summary>
        /// <param name="options">The runner options</param>
        /// <remarks>
        /// This constructor must come first due to a bug in aspnet/DependencyInjection. An issue is already filed.
        /// </remarks>
        public TestVersionTableMetaData(IOptions<RunnerOptions> options)
            : this()
        {
#pragma warning disable 612
            ApplicationContext = options.Value.ApplicationContext;
#pragma warning restore 612
        }

        public TestVersionTableMetaData()
        {
            SchemaName = "testSchemaName";
            OwnsSchema = true;
        }

        [Obsolete("Use dependency injection to access 'application state'.")]
        public object ApplicationContext { get; set; }

        public string SchemaName { get; set; }

        public string TableName => TABLE_NAME;

        public string ColumnName => COLUMN_NAME;

        public string UniqueIndexName => UNIQUE_INDEX_NAME;

        public string AppliedOnColumnName => APPLIED_ON_COLUMN_NAME;

        public string DescriptionColumnName => DESCRIPTION_COLUMN_NAME;

        public bool OwnsSchema { get; set; }
    }
}
