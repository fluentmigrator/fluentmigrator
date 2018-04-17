#region License
//
// Copyright (c) 2018, FluentMigrator Project
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Console
{
    class LateInitTaskExecutor : TaskExecutor
    {
        private readonly IAnnouncer _announcer;

        public LateInitTaskExecutor(IRunnerContext runnerContext)
            : base(runnerContext)
        {
            _announcer = runnerContext.Announcer;
        }

        [Obsolete]
        protected override IEnumerable<Assembly> GetTargetAssemblies()
        {
            var targetAssemblies = base.GetTargetAssemblies().ToList();
            if (targetAssemblies.Count != 0 && _announcer is LateInitAnnouncer announcer && string.IsNullOrEmpty(announcer.OutputTo))
            {
                var targetAssembly = targetAssemblies.First();
                announcer.OutputTo = targetAssembly.Location + ".sql";
            }

            return targetAssemblies;
        }
    }
}
