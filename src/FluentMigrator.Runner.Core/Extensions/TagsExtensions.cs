#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Runner.Extensions
{
    /// <summary>
    /// Provides extension methods for handling and manipulating tags in the context of FluentMigrator.
    /// </summary>
    public static class TagsExtensions
    {
         /// <summary>
         /// Converts a comma-separated string of tags into a list of individual tag strings.
         /// </summary>
         /// <param name="tags">The comma-separated string of tags. Can be <c>null</c> or empty.</param>
         /// <returns>
         /// A list of tags as individual strings. If the input string is <c>null</c> or empty, 
         /// an empty list is returned.
         /// </returns>
         /// <remarks>
         /// This method splits the input string using a comma (',') as the delimiter and removes any empty entries.
         /// </remarks>
         public static List<string> ToTags(this string tags)
         {
             if(string.IsNullOrEmpty(tags))
                 return new List<string>();

             return tags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
         }
    }
}
