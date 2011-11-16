using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner.Initialization
{
    public interface IAssemblyInitializer
    {
        void Initialize(IRunnerContext runnerContext);
    }
}
