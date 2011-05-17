#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 20011, Grant Archibald
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
using System.Data;

namespace FluentMigrator.Builders.Execute
{
   /// <summary>
   /// Provide a No-Op implementation of an execute expression
   /// </summary>
   /// <remarks>Can be used where the SQL/Script should not be executed <seealso cref="ExecuteExpressionRoot.WithDatabaseType"/></remarks>
   public class NullExecuteExpression : IExecuteExpressionRoot
   {
      public IExecuteExpressionRoot WithDatabaseType(DatabaseType type)
      {
         return this;
      }

      public void Sql(string sqlStatement)
      {
         
      }

      public void Script(string pathToSqlScript)
      {
         
      }

      public void WithConnection(Action<IDbConnection, IDbTransaction> operation)
      {
         
      }

      public void EmbeddedScript(string EmbeddedSqlScriptName)
      {
         
      }
   }
}