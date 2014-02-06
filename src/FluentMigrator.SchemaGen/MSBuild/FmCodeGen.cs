using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace FluentMigrator.SchemaGen.MSBuild
{
    /// <summary>
    /// Runs code generator as an MSBuild Task.
    /// </summary>
    public class FmCodeGen : Task, IOptions
    {
        #region IOptions
        public string Db { get; set; }
        public string Db1 { get; set; }
        public string Db2 { get; set; }

        [Required]
        public string BaseDirectory { get; set; }

        [Required]
        public string NameSpace { get; set; }

        [Required]
        public string MigrationVersion { get; set; }
        
        public int StepStart { get; set; }
        public int StepEnd { get; set; }
        public string Tags { get; set; }
        public bool UseDeprecatedTypes { get; set; }
        public string IncludeTables { get; set; }
        public string ExcludeTables { get; set; }
        #endregion

        [Output]
        public ITaskItem[] OutputClassFiles { get; private set; }

        public FmCodeGen()
        {
            AppDomain.CurrentDomain.ResourceResolve += new ResolveEventHandler(CurrentDomain_ResourceResolve);    
        }

        private static Assembly CurrentDomain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("Could Not Resolve {0}", args.Name);
            return null;
        }

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(Db) && (string.IsNullOrEmpty(Db1) || string.IsNullOrEmpty(Db2)))
            {
                Log.LogError("You must specify a connection string for either Db (if full schema) OR both Db1 and Db2 (schema diff).");
                return false;
            }

            try
            {
                var engine = new CodeGenFmClasses(this);
                OutputClassFiles = (from classPath in engine.GenClasses() 
                                    select new TaskItem(classPath) as ITaskItem)
                                    .ToArray(); 
                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }
        }
    }
}
