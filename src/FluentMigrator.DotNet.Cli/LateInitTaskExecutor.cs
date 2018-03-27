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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentMigrator.DotNet.Cli.CustomAnnouncers;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.DotNet.Cli
{
    class LateInitTaskExecutor : TaskExecutor
    {
        private readonly IAnnouncer _announcer;

        public LateInitTaskExecutor(IRunnerContext runnerContext)
            : base(runnerContext)
        {
            _announcer = runnerContext.Announcer;
        }

        protected override IEnumerable<Assembly> GetTargetAssemblies()
        {
            var targetAssemblies = base.GetTargetAssemblies().ToList();
            if (targetAssemblies.Count != 0)
            {
                var targetAssembly = targetAssemblies.First();
                var outputTo = targetAssembly.Location + ".sql";
                foreach (var lia in OfType<LateInitAnnouncer>(_announcer).Where(lia => string.IsNullOrEmpty(lia.OutputFileName)))
                {
                    lia.OutputFileName = outputTo;
                }
            }

            return targetAssemblies;
        }

        private static IEnumerable<TAnnouncer> OfType<TAnnouncer>(IAnnouncer announcer)
            where TAnnouncer : IAnnouncer
        {
            if (announcer is CompositeAnnouncer compositeAnnouncer)
            {
                foreach (var childAnnouncer in compositeAnnouncer.Announcers)
                {
                    foreach (var result in OfType<TAnnouncer>(childAnnouncer))
                    {
                        yield return result;
                    }
                }
            }
            else if (announcer is TAnnouncer lateInitAnnouncer)
            {
                yield return lateInitAnnouncer;
            }
        }
    }
}
