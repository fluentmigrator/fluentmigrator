#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Linq;
using System.Reflection;
using FluentMigrator.Runner.Initialization.AssemblyLoader;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
    public class TaskExecutor
    {
        private IMigrationRunner Runner { get; set; }
        private IRunnerContext RunnerContext { get; set; }

        public TaskExecutor(IRunnerContext runnerContext)
        {
            if (runnerContext == null)
                throw new ArgumentNullException("runnerContext", "RunnerContext cannot be null");

            RunnerContext = runnerContext;
        }

        private void Initialize()
        {
            var assembly = AssemblyLoaderFactory.GetAssemblyLoader(RunnerContext.Target).Load();

            var processor = InitializeProcessor(assembly.Location);

            Runner = new MigrationRunner(assembly, RunnerContext, processor);

            GreetAssembly(assembly, RunnerContext);
        }

        private static void GreetAssembly(Assembly assembly, IRunnerContext RunnerContext)
        {
            
            System.Console.WriteLine("Looking for OnAssemblyLoad in {0}", assembly.ToString());
            // should we check for current namespace?
            var types = assembly.GetTypes().Where(
                x => (
                    //String.Equals(x.Namespace, "Whatever", StringComparison.Ordinal) &&
                    x.GetCustomAttributes(false).Contains(new OnAssemblyLoadAttribute())));
            foreach (Type type in types)
            {                
                System.Console.WriteLine("Found: {0}", type.FullName);
                             
                if (typeof(IAssemblyInitializer).IsAssignableFrom(type))
                {
                    System.Console.WriteLine("Is IAssemblyIntializer: {0}", type.FullName);
                    var loader = (IAssemblyInitializer) type.Assembly.CreateInstance(type.FullName);                    
                    loader.Initialize(RunnerContext);
                    //type.InvokeMember("Initialize", BindingFlags.InvokeMethod, null, obj, new[] { RunnerContext });
                }
                else
                {
                    System.Console.WriteLine("Is not IAssemblyIntializer: {0}", type.FullName);
                }
            }

            foreach (Object obj in assembly.GetCustomAttributes(typeof(OnAssemblyLoadAttribute), false))
            {
                Type type = obj.GetType();
                var loader = type.Assembly.CreateInstance(type.FullName);
                
                if (typeof(IAssemblyInitializer).IsAssignableFrom(type))
                {
                    System.Console.WriteLine("Is IAssemblyIntializer: {0}", type.FullName);
                } 
                else
                {
                    System.Console.WriteLine("Is not IAssemblyIntializer: {0}", type.FullName);    
                }
                System.Console.WriteLine("Invoking Initialize");
                type.InvokeMember("Initialize", BindingFlags.InvokeMethod, null, obj, new[] { RunnerContext });
            }
            /*
            foreach (object obj in from obj in assembly.GetCustomAttributes(typeof (OnAssemblyLoadAttribute), false)
                                   from method in obj.GetType().GetMethods()
                                   select obj)
            {
                obj.GetType().InvokeMember("Initialize", BindingFlags.InvokeMethod, null, obj, new[] { RunnerContext });
            }
             */
        }


        public void Execute()
        {
            Initialize();

            switch (RunnerContext.Task)
            {
                case null:
                case "":
                case "migrate":
                case "migrate:up":
                    if (RunnerContext.Version != 0)
                        Runner.MigrateUp(RunnerContext.Version);
                    else
                        Runner.MigrateUp();
                    break;
                case "rollback":
                    if (RunnerContext.Steps == 0)
                        RunnerContext.Steps = 1;
                    Runner.Rollback(RunnerContext.Steps);
                    break;
                case "rollback:toversion":
                    Runner.RollbackToVersion(RunnerContext.Version);
                    break;
                case "rollback:all":
                    Runner.RollbackToVersion(0);
                    break;
                case "migrate:down":
                    Runner.MigrateDown(RunnerContext.Version);
                    break;
            }

            RunnerContext.Announcer.Say("Task completed.");
        }

        private IMigrationProcessor InitializeProcessor(string assemblyLocation)
        {
            var manager = new ConnectionStringManager(new NetConfigManager(), RunnerContext.Connection, RunnerContext.ConnectionStringConfigPath, assemblyLocation, RunnerContext.Database);

            manager.LoadConnectionString();

            if (RunnerContext.Timeout == 0)
            {
                RunnerContext.Timeout = 30; // Set default timeout for command
            }

            var processorFactory = ProcessorFactory.GetFactory(RunnerContext.Database);
            var processor = processorFactory.Create(manager.ConnectionString, RunnerContext.Announcer, new ProcessorOptions
            {
                PreviewOnly = RunnerContext.PreviewOnly,
                Timeout = RunnerContext.Timeout
            });

            return processor;
        }
    }
}