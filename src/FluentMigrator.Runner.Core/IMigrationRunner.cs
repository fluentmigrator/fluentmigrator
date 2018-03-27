#region License
// Copyright (c) 2007-2018, Sean Chambers and the FluentMigrator Project
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

using System.Reflection;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner
{
    public interface IMigrationRunner : IMigrationScopeStarter
    {
        IMigrationProcessor Processor { get; }
        IMigrationInformationLoader MigrationLoader { get; set; }
        IAssemblyCollection MigrationAssemblies { get; }
        IRunnerContext RunnerContext { get; }
        void Up(IMigration migration);
        void Down(IMigration migration);
        void MigrateUp();
        void MigrateUp(long version);
        void Rollback(int steps);
        void RollbackToVersion(long version);
        void MigrateDown(long version);
        void ValidateVersionOrder();
        void ListMigrations();
    }
}
