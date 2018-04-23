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
using System.Linq;

using FluentMigrator.Runner.VersionTableInfo;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner.Initialization
{
    public class AssemblySourceVersionTableMetaDataAccessor : IVersionTableMetaDataAccessor
    {
        private readonly Lazy<IVersionTableMetaData> _lazyValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblySourceVersionTableMetaDataAccessor"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to instantiate the found <see cref="IVersionTableMetaData"/> implementation</param>
        /// <param name="assemblySource">The assemblies used to search for the <see cref="IVersionTableMetaData"/> implementation</param>
        public AssemblySourceVersionTableMetaDataAccessor(
            [CanBeNull] IServiceProvider serviceProvider,
            [CanBeNull] IAssemblySource assemblySource = null)
        {
            _lazyValue = new Lazy<IVersionTableMetaData>(
                () =>
                {
                    if (assemblySource == null)
                        return null;

                    var matchedType = assemblySource.Assemblies.SelectMany(a => a.GetExportedTypes())
                        .FirstOrDefault(t => typeof(IVersionTableMetaData).IsAssignableFrom(t));

                    if (matchedType != null)
                    {
                        if (serviceProvider == null)
                            return (IVersionTableMetaData)Activator.CreateInstance(matchedType);
                        return (IVersionTableMetaData)ActivatorUtilities.CreateInstance(serviceProvider, matchedType);
                    }

                    return null;
                });
        }

        /// <inheritdoc />
        public IVersionTableMetaData VersionTableMetaData => _lazyValue.Value;
    }
}
