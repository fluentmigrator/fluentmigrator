#region License
// Copyright (c) 2018, FluentMigrator Project
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
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Runner.Processors;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Default implementation of <see cref="IConnectionStringAccessor"/>
    /// </summary>
    public class ConnectionStringAccessor : IConnectionStringAccessor
    {
        private readonly Lazy<string> _lazyData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringAccessor"/> class.
        /// </summary>
        /// <param name="processorOptions">The processor options containing the connection string or name</param>
        /// <param name="processorSelectorOptions">The selected processor (its ID is used as connection string name)</param>
        /// <param name="readers">The registered connection string readers</param>
        public ConnectionStringAccessor(
            IOptionsSnapshot<ProcessorOptions> processorOptions,
            IOptionsSnapshot<SelectingProcessorAccessorOptions> processorSelectorOptions,
            IEnumerable<IConnectionStringReader> readers)
        {
            _lazyData = new Lazy<string>(
                () =>
                {
                    var connectionStringOrName =
                        string.IsNullOrEmpty(processorOptions.Value.ConnectionString)
                            ? processorSelectorOptions.Value.ProcessorId
                            : processorOptions.Value.ConnectionString;
                    var result = (from accessor in readers.OrderByDescending(x => x.Priority)
                                  let cs = accessor.GetConnectionString(connectionStringOrName)
                                  where !string.IsNullOrEmpty(cs)
                                  select cs).FirstOrDefault();

                    if (string.IsNullOrEmpty(result))
                    {
                        result = connectionStringOrName;
                    }

                    return result;
                });
        }

        /// <inheritdoc />
        public string ConnectionString => _lazyData.Value;
    }
}
