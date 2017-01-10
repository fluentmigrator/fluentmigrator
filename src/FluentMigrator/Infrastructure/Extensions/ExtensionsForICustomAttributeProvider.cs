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
#if COREFX
        public static T GetOneAttribute<T>(this Type member)
#else
        public static T GetOneAttribute<T>(this ICustomAttributeProvider member)
#endif
        where T : Attribute {
            return member.GetOneAttribute<T>(false);
        }

#if COREFX
        public static T GetOneAttribute<T>(this Type member, bool inherit)
#else
        public static T GetOneAttribute<T>(this ICustomAttributeProvider member, bool inherit)
#endif
            where T : Attribute {
#if COREFX
            T[] attributes = member.GetTypeInfo().GetCustomAttributes(typeof(T), inherit) as T[];
#else
            T[] attributes = member.GetCustomAttributes(typeof(T), inherit) as T[];
#endif

            if ((attributes == null) || (attributes.Length == 0))
                return null;
            else
                return attributes[0];
        }

#if COREFX
        public static T[] GetAllAttributes<T>(this Type member)
#else
        public static T[] GetAllAttributes<T>(this ICustomAttributeProvider member)
#endif
            where T : Attribute {
            return member.GetAllAttributes<T>(false);
        }

#if COREFX
        public static T[] GetAllAttributes<T>(this Type member, bool inherit)
#else
        public static T[] GetAllAttributes<T>(this ICustomAttributeProvider member, bool inherit)
#endif
            where T : Attribute {
#if COREFX
            return member.GetTypeInfo().GetCustomAttributes(typeof(T), inherit) as T[];
#else
            return member.GetCustomAttributes(typeof(T), inherit) as T[];
#endif
        }

#if COREFX
        public static bool HasAttribute<T>(this Type member)
#else
        public static bool HasAttribute<T>(this ICustomAttributeProvider member)
#endif
            where T : Attribute {
            return member.HasAttribute<T>(false);
        }

#if COREFX
        public static bool HasAttribute<T>(this Type member, bool inherit)
#else
        public static bool HasAttribute<T>(this ICustomAttributeProvider member, bool inherit)
#endif
            where T : Attribute {
#if COREFX
            return member.GetTypeInfo().IsDefined(typeof(T), inherit);
#else
            return member.IsDefined(typeof(T), inherit);
#endif
        }
    }
}