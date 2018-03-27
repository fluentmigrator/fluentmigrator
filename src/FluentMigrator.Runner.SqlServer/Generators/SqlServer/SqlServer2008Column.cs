#region License
// Copyright (c) 2007-2018, Sean Chambers and the FluentMigrator Project
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

using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    internal class SqlServer2008Column : SqlServer2005Column
    {
        public SqlServer2008Column(ITypeMap typeMap)
            : base(typeMap)
        {
            ClauseOrder.Add(FormatRowGuid);
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.CurrentDateTimeOffset:
                    return "SYSDATETIMEOFFSET()";
            }

            return base.FormatSystemMethods(systemMethod);
        }
    }
}
