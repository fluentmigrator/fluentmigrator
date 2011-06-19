#region License
// 
// Copyright (c) 2011, Grant Archibald
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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.SchemaDump.SchemaDumpers;
using Microsoft.CSharp;

namespace FluentMigrator.SchemaDump.SchemaMigrations
{
   /// <summary>
   /// Migrates the schema items from between different schema types
   /// </summary>
   public abstract class BaseSchemaMigrator
   {
      protected IAnnouncer Announcer { get; private set; }

      protected BaseSchemaMigrator(IAnnouncer announcer)
      {
         Announcer = announcer;
      }

      /// <summary>
      /// Generates schema migrations from the datasource as C# source code
      /// </summary>
      /// <param name="context">Define data required to generate the C# migrations</param>
      public void Generate(SchemaMigrationContext context)
      {
          Announcer.AnnounceTime = context.AnnounceTime;
         Announcer.Say(string.Format("Generating schema {0} migrations from {1}", context.Type, context.FromConnectionString));
         using (var connection = GetConnection(context))
         {

            var processor = GetProcessor(connection);
            var schemaDumper = GetSchemaDumper(processor);

            if ( context.MigrationRequired(MigrationType.Tables) || context.MigrationRequired(MigrationType.Data) || context.MigrationRequired(MigrationType.ForeignKeys))
            {
               var tables = new CSharpTableMigrationsWriter(Announcer);
               tables.GenerateMigrations(context, schemaDumper);   
            }

            if (context.MigrationRequired(MigrationType.Procedures))
            {
               var procedures = new CSharpProcedureMigrationsWriter(Announcer);
               procedures.GenerateMigrations(context, schemaDumper);
            }

            if (context.MigrationRequired(MigrationType.Functions))
            {
               var functions = new CSharpFunctionMigrationsWriter(Announcer);
               functions.GenerateMigrations(context, schemaDumper);
            }

            // Migrate the views last as procedures or functions may be included in them
            if (context.MigrationRequired(MigrationType.Views))
            {
               var views = new CSharpViewMigrationsWriter(Announcer);
               views.GenerateMigrations(context, schemaDumper);
            }
         }
      }

      /// <summary>
      /// Perform migration using the provided context
      /// </summary>
      /// <param name="context">The context that control how the migration should be generated/executed</param>
      public void Migrate(SchemaMigrationContext context)
      {
          Announcer.AnnounceTime = context.AnnounceTime;

         if (context.ExecuteInMemory)
         {
            Announcer.Say("Compiling migration");
            var compiledAssembly = CompileMigrations(GetSourceFiles(context), typeof (Migration).Assembly.Location);

            if ( compiledAssembly == null)
            {
               Announcer.Say("No migrations found");
            }
            else
               MigrateUp(compiledAssembly, context);
         }
      }

      /// <summary>
      /// Gets a list of source files that should be compiled into the migration assembly
      /// </summary>
      /// <param name="context">The context that contain the MigrationsDirectory</param>
      /// <returns>The C# source files to compile</returns>
      private static string[] GetSourceFiles(SchemaMigrationContext context)
      {
         var migrationsDirectory = Path.Combine(context.WorkingDirectory, context.MigrationsDirectory);
         return Directory.GetFiles(migrationsDirectory, "*.cs");
      }

      /// <summary>
      /// Generates an inmemory migration assembly using the provided source files
      /// </summary>
      /// <remarks>Throws exception if the assembly cannot be compiled</remarks>
      /// <param name="files">The files to be compiled</param>
      /// <param name="additionalAssemblies">Any referenced assemblies required by the source files</param>
      /// <returns>The compiled assembly</returns>
      private static Assembly CompileMigrations(string[] files, params string[] additionalAssemblies)
      {
         if ( files.Length == 0)
            return null;

         var csc = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v3.5" } });
         var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.dll" }, "foo.dll", true)
         {
            GenerateExecutable = false // Indicate we want dll not exe
            ,GenerateInMemory = true  // And want it in memory
            ,IncludeDebugInformation = false // And do not require debug info to be created

         };

         if (additionalAssemblies != null)
            parameters.ReferencedAssemblies.AddRange(additionalAssemblies);

         var results = csc.CompileAssemblyFromFile(parameters, files);


         if (results.Errors.Count > 0)
         {
            var exception = new StringBuilder();
            exception.Append("Unable to compile code snippet. See Errors");
            foreach (var error in results.Errors)
            {
               exception.Append("\r\n");
               exception.Append(error);
            }
            throw new Exception(exception.ToString());
         }

         return results.CompiledAssembly;
      }

      

      protected abstract ISchemaDumper GetSchemaDumper(IMigrationProcessor processor);

      protected abstract IMigrationProcessor GetProcessor(IDbConnection connection);

      protected abstract IDbConnection GetConnection(SchemaMigrationContext context);

      /// <summary>
      /// Performs the process of applying the migrations to the ToConnectionString
      /// </summary>
      /// <param name="assembly">The assembly to obatin the migrations from</param>
      /// <param name="context">The context that determine the target database</param>
      private void MigrateUp(Assembly assembly, SchemaMigrationContext context)
      {
         var runnerContext = new RunnerContext(Announcer)
                                {
                                   Database = context.ToDatabaseType.ToString().ToLower(),
                                   Connection = context.ToConnectionString
                                };

         if (!string.IsNullOrEmpty(context.WorkingDirectory))
         {
            Announcer.Say("Set working directory to " + context.WorkingDirectory);
            runnerContext.WorkingDirectory = context.WorkingDirectory;
         }
            

         Announcer.Say("Migrating to database type " + runnerContext.Database);
         Announcer.Say("With connection string " + context.ToConnectionString);

         var factory = ProcessorFactory.GetFactory(context.ToDatabaseType.ToString());
         IMigrationProcessor toProcessor = null;
         try
         {
            toProcessor = factory.Create(context.ToConnectionString, Announcer, new ProcessorOptions
                                                                                    {
                                                                                       PreviewOnly = false,
                                                                                       Timeout = 30
                                                                                    });

            if (toProcessor is OracleProcessor && context.OracleSequenceNamer != null)
               ((OracleProcessor) toProcessor).CustomSequenceNamer = context.OracleSequenceNamer;

            var runner = new MigrationRunner(assembly, runnerContext, toProcessor);
            runner.MigrateUp(true);
         }
         finally
         {
            if (toProcessor != null && toProcessor is IDisposable)
               ((IDisposable)toProcessor).Dispose();
         }
      }
   }
}