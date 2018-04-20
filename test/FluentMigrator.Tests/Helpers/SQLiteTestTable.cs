#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using System.Data;
using System.Text;
using FluentMigrator.Runner.Processors.SQLite;

namespace FluentMigrator.Tests.Helpers {
    // ReSharper disable once InconsistentNaming
    public class SQLiteTestTable : IDisposable {
        private readonly string _schemaName;
        private IDbConnection Connection { get; set; }
        public string Name { get; set; }
        private IDbTransaction Transaction { get; set; }

        public SQLiteTestTable( SQLiteProcessor processor, string schemaName, params string[] columnDefinitions ) {
            _schemaName = schemaName;
            Connection = processor.Connection;
            Transaction = processor.Transaction;

            Name = "Table" + Guid.NewGuid().ToString( "N" );
            Create( columnDefinitions );
        }

        public void Dispose() {
            Drop();
        }

        public void Create( IEnumerable<string> columnDefinitions ) {
            if ( !string.IsNullOrEmpty( _schemaName ) ) {
                using ( var command = Connection.CreateCommand() ) {
                    //new DbCommand(string.Format("CREATE SCHEMA [{0}]", _schemaName), Connection, Transaction)
                    command.CommandText = string.Format( "CREATE SCHEMA [{0}]", _schemaName );
                    command.Transaction = Transaction;
                    command.ExecuteNonQuery();
                }
            }

            var sb = new StringBuilder();
            sb.Append( "CREATE TABLE " );
            if ( !string.IsNullOrEmpty( _schemaName ) )
                sb.AppendFormat( "[{0}].", _schemaName );
            sb.Append( Name );

            foreach ( string definition in columnDefinitions ) {
                sb.Append( "(" );
                sb.Append( definition );
                sb.Append( "), " );
            }

            sb.Remove( sb.Length - 2, 2 );
            using ( var command = Connection.CreateCommand() ) {
                //var command = new SqlCommand(sb.ToString(), Connection, Transaction)
                command.CommandText = sb.ToString();
                command.Transaction = Transaction;
                command.ExecuteNonQuery();
            }
            //using (var command = new SqlCommand(sb.ToString(), Connection, Transaction))
            //    command.ExecuteNonQuery();
        }

        public void Drop() {
            if ( string.IsNullOrEmpty( _schemaName ) ) {
                using ( var command = Connection.CreateCommand() ) {
                    //var command = new SqlCommand(sb.ToString(), Connection, Transaction)
                    command.CommandText = "DROP TABLE " + Name;
                    command.Transaction = Transaction;
                    command.ExecuteNonQuery();
                }
                //using (var command = new SqlCommand("DROP TABLE " + Name, Connection, Transaction))
                //    command.ExecuteNonQuery();
            } else {
                using ( var command = Connection.CreateCommand() ) {
                    //var command = new SqlCommand(sb.ToString(), Connection, Transaction)
                    command.CommandText = string.Format( "DROP TABLE [{0}].{1}", _schemaName, Name );
                    command.Transaction = Transaction;
                    command.ExecuteNonQuery();
                }
                //using (var command = new SqlCommand(string.Format("DROP TABLE [{0}].{1}", _schemaName, Name), Connection, Transaction))
                //    command.ExecuteNonQuery();

                using ( var command = Connection.CreateCommand() ) {
                    //var command = new SqlCommand(sb.ToString(), Connection, Transaction)
                    command.CommandText = string.Format( "DROP SCHEMA [{0}]", _schemaName );
                    command.Transaction = Transaction;
                    command.ExecuteNonQuery();
                }
                //using (var command = new SqlCommand(string.Format("DROP SCHEMA [{0}]", _schemaName), Connection, Transaction))
                //    command.ExecuteNonQuery();
            }
        }
    }
}
