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

namespace FluentMigrator.Runner.Initialization.AssemblyLoader
{
    /// <summary>
    /// Class that creates an <see cref="IAssemblyLoader"/> for a given assembly name
    /// </summary>
#if NET
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This type uses AppDomain to load assemblies, which may not be preserved in trimmed applications.")]
#endif
    public class AssemblyLoaderFactory
    {
        /// <summary>
        /// Returns an assembly loader for an assembly with the given name
        /// </summary>
        /// <param name="name">The assembly name (can be an assembly name or file name)</param>
        /// <returns>The new assembly loader</returns>
        public virtual IAssemblyLoader GetAssemblyLoader(string name)
        {
            var nameInLowerCase = name.ToLower();

            if (nameInLowerCase.EndsWith(".dll") || nameInLowerCase.EndsWith(".exe"))
                return new AssemblyLoaderFromFile(name);

            return new AssemblyLoaderFromName(name);
        }
    }
}
