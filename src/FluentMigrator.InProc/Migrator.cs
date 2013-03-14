#region Copyright (c) 2011, Agile Utilities New Zealand Ltd.
// Copyright (c) 2011, Agile Utilities New Zealand Ltd., http://www.agileutilities.com
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
#endregion

using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.InProc {
   public class Migrator {
      private class InProcRunnerContext : RunnerContext {
         public InProcRunnerContext(IMigratorContext migratorContext) : base(migratorContext.Announcer){
            Timeout = migratorContext.Timeout;
            Connection = migratorContext.Connection;
            Database = migratorContext.Database;
            PreviewOnly = migratorContext.PreviewOnly;
         }
      }

      public Migrator(IMigratorContext runnerContext) {
         RunnerContext = new InProcRunnerContext(runnerContext);
         MigrationsAssembly = runnerContext.MigrationsAssembly;
      }

      public void MigrateUp() {
         Initialize();
         Runner.MigrateUp();
         RunnerContext.Announcer.Say("FluentMigrator Task Completed.");
      }

      public void MigrateUp(long version) {
         Initialize();
         Runner.MigrateUp(version);
         RunnerContext.Announcer.Say("FluentMigrator Task Completed.");
      }

      public void Rollback(int steps) {
         Initialize();
         if (steps <= 0) {
            steps = 1;
         }
         Runner.Rollback(steps);
         RunnerContext.Announcer.Say("FluentMigrator Task Completed.");
      }

      public void RollbackToVersion(long version) {
         Initialize();
         Runner.RollbackToVersion(version);
         RunnerContext.Announcer.Say("FluentMigrator Task Completed.");
      }

      public void RollbackAll() {
         Initialize();
         Runner.RollbackToVersion(0);
         RunnerContext.Announcer.Say("FluentMigrator Task Completed.");
      }

      public void MigrateDown(long version) {
         Initialize();
         Runner.MigrateDown(version);
         RunnerContext.Announcer.Say("FluentMigrator Task Completed.");
      }

      private void Initialize() {
         var processor = InitializeProcessor(MigrationsAssembly.Location);
         Runner = new MigrationRunner(MigrationsAssembly, RunnerContext, processor);
      }

      private IMigrationProcessor InitializeProcessor(string assemblyLocation) {
         var manager = new ConnectionStringManager(new NetConfigManager(), RunnerContext.Connection, RunnerContext.ConnectionStringConfigPath, assemblyLocation, RunnerContext.Database);

         manager.LoadConnectionString();

         if (RunnerContext.Timeout == 0) {
            RunnerContext.Timeout = 30; // Set default timeout for command
         }

         var processorFactory = ProcessorFactory.GetFactory(RunnerContext.Database);
         var processor = processorFactory.Create(manager.ConnectionString, RunnerContext.Announcer, new ProcessorOptions {
            PreviewOnly = RunnerContext.PreviewOnly,
            Timeout = RunnerContext.Timeout
         });

         return processor;
      }
      
      private IMigrationRunner Runner { get; set; }
      private IRunnerContext RunnerContext { get; set; }
      private Assembly MigrationsAssembly { get; set; }
   }
}
