#region Apache License
// 
// Copyright (c) 2014, Tony O'Hagan <tony@ohagan.name>
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
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace FluentMigrator.SchemaGen
{
    class Program
    {
        private static string GetDbConnectionString(string dbName)
        {
            if (string.IsNullOrEmpty(dbName) 
                || 
                dbName.Contains("="))   // Is it a connection string?
            {
                return dbName;
            }
            else
            {
                // Use the App.config template to generate a connection string
                string cnnTemplate = ConfigurationManager.ConnectionStrings["template"].ConnectionString;
                string cnnString = cnnTemplate.Replace("{dbName}", dbName);
                return cnnString;
            }
        }

        private static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                try
                {
                    options.Db = GetDbConnectionString(options.Db);
                    options.Db1 = GetDbConnectionString(options.Db1);
                    options.Db2 = GetDbConnectionString(options.Db2);

                    new CodeGenFmClasses(options).GenClasses();
                }
                catch (DatabaseArgumentException)
                {
                    Console.WriteLine("Specificy EITHER --db OR --db1 and --db2 options.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.InnerException != null) Console.WriteLine(ex.InnerException.Message);
                    //Console.WriteLine("Press any key to continue.");
                    //Console.ReadKey();
                    Environment.Exit(1);
                }

            }
        }
    }
}
