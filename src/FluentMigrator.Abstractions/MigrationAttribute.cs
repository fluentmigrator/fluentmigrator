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

namespace FluentMigrator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MigrationAttribute : Attribute
    {
        public MigrationAttribute(long version)
            : this(version, TransactionBehavior.Default)
        {
        }

        public MigrationAttribute(long version, string description)
            : this(version, TransactionBehavior.Default, description)
        {
        }

        public MigrationAttribute(long version, TransactionBehavior transactionBehavior)
            : this(version, transactionBehavior, null)
        {
        }

        public MigrationAttribute(long version, TransactionBehavior transactionBehavior, string description)
        {
            Version = version;
            TransactionBehavior = transactionBehavior;
            Description = description;
        }

        public long Version { get; private set; }
        public TransactionBehavior TransactionBehavior { get; private set; }
        public string Description { get; private set; }
    }
}