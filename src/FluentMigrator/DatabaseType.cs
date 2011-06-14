#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator
{
   /// <summary>
   /// List of supported database types
   /// </summary>
   [Flags]
   public enum DatabaseType
   {
      /// <summary>
      /// Microsoft Jet database e.g. Access, Excel
      /// </summary>
      Jet = 2,
      /// <summary>
      /// MySql database - http://www.mysql.com
      /// </summary>
      MySql = 4,
      /// <summary>
      /// Oracle database - http://www.oracle.com/us/products/database/index.html
      /// </summary>
      Oracle = 8,
      /// <summary>
      /// Postgres - http://www.postgresql.org
      /// </summary>
      Postgres = 16,
      /// <summary>
      /// Sqllite - http://www.sqlite.org/ 
      /// </summary>
      Sqlite = 32,
      /// <summary>
      /// Microsoft Sql Server 2000 - http://www.microsoft.com/sqlserver
      /// </summary>
      SqlServer2000 = 64,
      /// <summary>
      /// Microsoft Sql Server 2005 - http://www.microsoft.com/sqlserver
      /// </summary>
      SqlServer2005 = 128,
      /// <summary>
      /// Microsoft Sql Server 2008 - http://www.microsoft.com/sqlserver
      /// </summary>
      SqlServer2008 = 256,
      /// <summary>
      /// Microsoft Sql Compact - http://blogs.msdn.com/b/sqlservercompact/
      /// </summary>
      SqlServerCe = 512,
      /// <summary>
      /// Any Version of Microsoft SQL Server (Excluding Compact Edition)
      /// </summary>
      SqlServer = SqlServer2000 + SqlServer2005 + SqlServer2008
   }
}
