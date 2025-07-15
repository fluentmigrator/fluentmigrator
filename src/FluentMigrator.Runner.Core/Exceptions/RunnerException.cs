#region License
// Copyright (c) 2018, Fluent Migrator Project
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
using System.Runtime.Serialization;

using FluentMigrator.Exceptions;

namespace FluentMigrator.Runner.Exceptions
{
    public class RunnerException : FluentMigratorException
    {
        protected RunnerException()
        {
        }

        protected RunnerException(string message) : base(message)
        {
        }

        protected RunnerException(string message, Exception innerException) : base(message, innerException)
        {
        }
#if NET
        [Obsolete("Formatter-based serialization is obsolete")]
#endif
        protected RunnerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
