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
using System.Reflection;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Assembly loader engine
    /// </summary>
    public interface IAssemblyLoadEngine
    {
        /// <summary>
        /// Try loading an assembly with the given name
        /// </summary>
        /// <param name="name">The assembly name</param>
        /// <param name="exceptions">The collected exceptions</param>
        /// <param name="assembly">The loaded assembly</param>
        /// <returns><c>true</c> if the assembly could be loaded</returns>
        bool TryLoad(string name, ICollection<Exception> exceptions, out Assembly assembly);
    }
}
