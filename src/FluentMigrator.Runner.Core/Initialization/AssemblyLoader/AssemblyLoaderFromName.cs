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

using System.Reflection;

namespace FluentMigrator.Runner.Initialization.AssemblyLoader
{
    public class AssemblyLoaderFromName : IAssemblyLoader
    {
        private readonly string _name;

        public AssemblyLoaderFromName(string name)
        {
            _name = name;
        }

        public Assembly Load()
        {
            Assembly assembly = Assembly.Load(_name);
            return assembly;
        }
    }
}
