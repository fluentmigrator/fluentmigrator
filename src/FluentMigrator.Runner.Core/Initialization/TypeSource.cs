#region License
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Reflection;
using System.Text;

using FluentMigrator.Infrastructure;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Provides access to delay-loaded assemblies
    /// </summary>
#if NET
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This type gets the exported types from assemblies, which may not be preserved in trimmed applications.")]
#endif
    public class AssemblyTypeSource : ITypeSource
    {
        private readonly IAssemblySource _source;

        public AssemblyTypeSource(IAssemblySource source)
        {
            _source = source;
        }

        /// <inheritdoc />
        public IEnumerable<Type> GetTypes()
        {
            return _source.Assemblies.SelectMany(a => a.GetExportedTypes());
        }
    }
}
