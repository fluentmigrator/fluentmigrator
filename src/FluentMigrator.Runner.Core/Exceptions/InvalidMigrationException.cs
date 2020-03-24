#region License
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator.Runner.Exceptions
{
    public class InvalidMigrationException : RunnerException
    {
        private readonly IMigration _migration;
        private readonly string _errors;

        public InvalidMigrationException(IMigration migration, string errors)
        {
            _migration = migration;
            _errors = errors;
        }

        public override string Message
        {
            get
            {
                return string.Format("The migration {0} contained the following Validation Error(s): {1}", _migration.GetType().Name, _errors);
            }
        }
    }
}
