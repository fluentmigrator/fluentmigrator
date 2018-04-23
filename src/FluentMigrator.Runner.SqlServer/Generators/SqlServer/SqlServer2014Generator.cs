#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServer2014Generator : SqlServer2012Generator
    {
        public SqlServer2014Generator()
            : this(new SqlServer2008Quoter())
        {
        }

        public SqlServer2014Generator(SqlServer2008Quoter quoter)
            : base(quoter)
        {
        }

        protected SqlServer2014Generator(IColumn column, IQuoter quoter, IDescriptionGenerator descriptionGenerator)
            :base(column, quoter, descriptionGenerator)
        {
        }
    }
}
