#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator.Runner.Processors.DB2
{
    using System;
    using System.Data.Common;
    using System.Reflection;

    public class Db2DbFactory : ReflectionBasedDbFactory
    {
        #region Constructors

        public Db2DbFactory()
            : base("IBM.Data.DB2.iSeries", "IBM.Data.DB2.iSeries.iDB2Factory")
        {
        }

        #endregion Constructors

        #region Methods

        protected override DbProviderFactory CreateFactory()
        {
            var assembly = AppDomain.CurrentDomain.Load("IBM.Data.DB2.iSeries, Version=12.0.0.0, Culture=neutral, PublicKeyToken=9cdb2ebfb1f93a26");
            var type = assembly.GetType("IBM.Data.DB2.iSeries.iDB2Factory");
            var field = type.GetField("Instance", BindingFlags.Static | BindingFlags.Public);

            if (field == null)
            {
                return base.CreateFactory();
            }

            return (DbProviderFactory)field.GetValue(null);
        }

        #endregion Methods
    }
}
