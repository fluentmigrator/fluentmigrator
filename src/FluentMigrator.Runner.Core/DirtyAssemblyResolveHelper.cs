#region License
// Copyright (c) 2024, Fluent Migrator Project
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Temporary workaround for
    /// https://github.com/fluentmigrator/fluentmigrator/issues/1406
    ///
    /// taken mostly from
    /// https://github.com/dotnet/BenchmarkDotNet/blob/852bb8cd9c2ac2530866dc53723c5f2ce3d411fa/src/BenchmarkDotNet/Helpers/DirtyAssemblyResolveHelper.cs#L18
    /// 
    /// Sometimes NuGet/VS/other tool does not generate the right assembly binding redirects
    /// or just for any other magical reasons
    /// our users get FileNotFoundException when trying to run their benchmarks
    ///
    /// We want our users to be happy and we try to help the .NET framework when it fails to load an assembly
    ///
    /// It's not recommended to copy this code OR reuse it anywhere. It's an UGLY WORKAROUND.
    ///
    /// If one day we can remove it, the person doing that should celebrate!!
    /// </summary>
    internal class DirtyAssemblyResolveHelper : IDisposable
    {
        internal static IDisposable Create() => new DirtyAssemblyResolveHelper();

        private DirtyAssemblyResolveHelper() => AppDomain.CurrentDomain.AssemblyResolve += HelpTheFrameworkToResolveTheAssembly;

        public void Dispose() => AppDomain.CurrentDomain.AssemblyResolve -= HelpTheFrameworkToResolveTheAssembly;

        /// <summary>
        /// according to https://msdn.microsoft.com/en-us/library/ff527268(v=vs.110).aspx
        /// "the handler is invoked whenever the runtime fails to bind to an assembly by name."
        /// </summary>
        /// <returns>not null when we find it manually, null when can't help</returns>
        private Assembly HelpTheFrameworkToResolveTheAssembly(object sender, ResolveEventArgs args)
        {
            var fullName = new AssemblyName(args.Name);
            string simpleName = fullName.Name;

            // Get assemblies from runtime environment
            string[] runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), $"{simpleName}.dll");

            // Create the list of assembly paths consisting of runtime assemblies
            var paths = new List<string>(runtimeAssemblies);

            // Since we search by simple name, there should hopefully be one file
            string guessedPath = paths.FirstOrDefault();

            // If there wasn't a runtime file check the app directory for the file 
            if (guessedPath == null)
            {
                guessedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{simpleName}.dll");
            }
            
            if (!File.Exists(guessedPath))
                return null; // we can't help, and we also don't call Assembly.Load which if fails comes back here, creates endless loop and causes StackOverflow

            // the file is right there, but has most probably different version and there is no assembly redirect
            // so we just load it and ignore the version mismatch
            // we can at least try because benchmarks are not executed in the Host process,
            // so even if we load some bad version of the assembly
            // we might still produce the right exe with proper references

            // we warn the user about that, in case some Super User want to be aware of that
            Console.WriteLine($"// Wrong assembly binding redirects for {simpleName}, loading it from disk anyway. {guessedPath}");

            return Assembly.LoadFrom(guessedPath);
        }
    }
}
