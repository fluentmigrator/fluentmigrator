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

namespace FluentMigrator.Model
{
   ///<summary>
   /// Wraps information on a view read from a database
   ///</summary>
   public class ViewDefinition
   {
      /// <summary>
      /// The schema that the viwe belongs to
      /// </summary>
      public string SchemaName { get; set; }

      /// <summary>
      /// The name of the view
      /// </summary>
      public string Name { get; set; }

      /// <summary>
      /// The sql statement used to create the original view
      /// </summary>
      public string CreateViewSql { get; set; }
   }
}
