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
using System.IO;
using FluentMigrator;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.SchemaDump.SchemaMigrations;
using Mono.Options;

namespace FluentSchemaMigrator.Console
{
	public class SchemaMigratorConsole
	{
		private readonly TextWriter _announcerOutput;
        internal readonly SchemaMigrationContext SchemaMigrationContext = new SchemaMigrationContext();
	    private bool _showHelp;
        private bool _generateOnly;

	    static void DisplayHelp( OptionSet p )
		{
			const string hr = "-------------------------------------------------------------------------------";
			System.Console.WriteLine( hr );
			System.Console.WriteLine( "=============================== FluentSchemaMigrator ================================" );
			System.Console.WriteLine( hr );
			System.Console.WriteLine( "Source Code:" );
			System.Console.WriteLine( "  http://github.com/schambers/fluentmigrator/network" );
			System.Console.WriteLine( "Ask For Help:" );
			System.Console.WriteLine( "  http://groups.google.com/group/fluentmigrator-google-group" );
			System.Console.WriteLine( hr );
			System.Console.WriteLine( "Usage:" );
            System.Console.WriteLine("  SchemaMigrate [OPTIONS]");
			System.Console.WriteLine( "Example:" );
            System.Console.WriteLine("  SchemaMigrate -db SqlServer2008 -conn \"SEE_BELOW\" -destdb SqlServer2008 -destconn \"SEE_BELOW\"");
			System.Console.WriteLine( hr );
			System.Console.WriteLine( "Example Connection Strings:" );
			System.Console.WriteLine( "  MySql: Data Source=172.0.0.1;Database=Foo;User Id=USERNAME;Password=BLAH" );
			System.Console.WriteLine( "  Oracle: Server=172.0.0.1;Database=Foo;Uid=USERNAME;Pwd=BLAH" );
			System.Console.WriteLine( "  SqlLite: Data Source=:memory:;Version=3;New=True" );
			System.Console.WriteLine( "  SqlServer: server=127.0.0.1;database=Foo;user id=USERNAME;password=BLAH" );
			System.Console.WriteLine( "             server=.\\SQLExpress;database=Foo;trusted_connection=true" );
			System.Console.WriteLine( hr );
            System.Console.WriteLine("Notes:");
            System.Console.WriteLine("If destdb and destconn are not specified only the C# migrations, Data and scripts will be generated");
            System.Console.WriteLine(hr);
			System.Console.WriteLine( "Options:" );
			p.WriteOptionDescriptions( System.Console.Out );
		}

		public SchemaMigratorConsole( params string[] args )
			: this( System.Console.Out, args )
		{
		}

        public SchemaMigratorConsole(TextWriter announcerOutput, params string[] args)
		{
			_announcerOutput = announcerOutput;

            
			try
			{
				var optionSet = new OptionSet
				{
					{
						"db=",
						"REQUIRED. The type of database you want to migrate the schema from.",
						v =>
						    {
						        SchemaMigrationContext.FromDatabaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), v,true);
						    }
					},
					{
						"conn=|connection=",
						"REQUIRED. The connection string to the source database",
						v => { SchemaMigrationContext.FromConnectionString = v; }
					},
                    {
						"destdb=",
						"OPTIONAL. The type of database you want to migrate the schema to.",
						v => { SchemaMigrationContext.ToDatabaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), v); }
					},
					{
						"destconn=|destconnection=",
						"OPTIONAL. The connection string to the destination database",
						v => { SchemaMigrationContext.ToConnectionString = v; }
					},
					{
						"workingdirectory=|wd=",
						"The directory that contains Scripts and Data as sub folders",
						v => { SchemaMigrationContext.WorkingDirectory = v; }
					},
					{
						"help|h|?",
						"Displays this help menu.",
						v => { _showHelp = true; }
					}
				};

				try
				{
					optionSet.Parse( args );
				}
				catch ( OptionException e )
				{
					System.Console.WriteLine( "FluentMigrator.Console:" );
					System.Console.WriteLine( e.Message );
					System.Console.WriteLine( "Try 'schemamigrate --help' for more information." );
					return;
				}

                

				if ( ! ArgumentsValid() )
				{
					DisplayHelp( optionSet );
					Environment.ExitCode = 1;
					return;
				}

				if ( _showHelp )
				{
					DisplayHelp( optionSet );
					return;
				}

			    ExecuteSchemaMigration();

			}
			catch ( Exception ex )
			{
				System.Console.WriteLine( "!! An error has occurred.  The error is:" );
				System.Console.WriteLine( ex );
				Environment.ExitCode = 1;
			}
		}

	    private bool ArgumentsValid()
	    {
            if ( SchemaMigrationContext.FromDatabaseType.Equals(DatabaseType.Unknown))
                return false;
            if ( string.IsNullOrEmpty(SchemaMigrationContext.FromConnectionString))
                return false;



	        _generateOnly = SchemaMigrationContext.ToDatabaseType.Equals(DatabaseType.Unknown)
	                       ||
	                       string.IsNullOrEmpty(SchemaMigrationContext.ToConnectionString);

	        return true;
	    }

	    private void ExecuteSchemaMigration()
	    {
	        BaseSchemaMigrator migrator;
	        switch ( SchemaMigrationContext.FromDatabaseType)
	        {
	            case DatabaseType.SqlServer:
                case DatabaseType.SqlServer2008:
                case DatabaseType.SqlServer2005:
	                migrator = new SqlServerSchemaMigrator(new TextWriterAnnouncer(_announcerOutput));
	                break;
                case DatabaseType.Oracle:
	                migrator = new OracleSchemaMigrator(new TextWriterAnnouncer(_announcerOutput));
	                break;
                default:
	                throw new NotSupportedException(string.Format("Database type {0} not supported as source database",
	                                                              SchemaMigrationContext.FromDatabaseType));
	        }

            migrator.Generate(SchemaMigrationContext);

            if ( _generateOnly )
                return;

            migrator.Migrate(SchemaMigrationContext);
	    }
	}
}