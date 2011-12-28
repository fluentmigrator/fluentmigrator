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
using System.Reflection;

namespace FluentMigrator.Infrastructure.Extensions
{
    public static class ExtensionsForICustomAttributeProvider
    {
        public static T GetOneAttribute<T>(this ICustomAttributeProvider member)
            where T : Attribute
        {
            return member.GetOneAttribute<T>(false);
        }

        public static T GetOneAttribute<T>(this ICustomAttributeProvider member, bool inherit)
            where T : Attribute
        {
            T[] attributes = member.GetCustomAttributes(typeof(T), inherit) as T[];

            if ((attributes == null) || (attributes.Length == 0))
                return null;
            else
                return attributes[0];
        }

        public static T[] GetAllAttributes<T>(this ICustomAttributeProvider member)
            where T : Attribute
        {
            return member.GetAllAttributes<T>(false);
        }

        public static T[] GetAllAttributes<T>(this ICustomAttributeProvider member, bool inherit)
            where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), inherit) as T[];
        }

        public static bool HasAttribute<T>(this ICustomAttributeProvider member)
            where T : Attribute
        {
            return member.HasAttribute<T>(false);
        }

        public static bool HasAttribute<T>(this ICustomAttributeProvider member, bool inherit)
            where T : Attribute
        {
            return member.IsDefined(typeof(T), inherit);
        }
    }
}