#region License
// 
// Copyright (c) 2007-2014, Sean Chambers <schambers80@gmail.com>
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
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MigrationDescriptionAttribute : Attribute
    {
        public MigrationDescriptionAttribute(Type resourceType) :
            this(resourceType, null)
        {

        }

        public MigrationDescriptionAttribute(Type resourceType, string name)
        {
            if (resourceType == null)
                throw new ArgumentNullException("resourceType");
            ResourceType = resourceType;
            if (string.IsNullOrEmpty(name) == false && char.IsLetter(name[0]) == false)
                throw new ArgumentException("Invalid resource key name", "name");
            Name = name;
        }

        public string Name { get; private set; }
        public Type ResourceType { get; private set; }
    }
}
