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
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace FluentMigrator.Runner.Processors
{
    public abstract class DbFactoryBase
    {
        private readonly object _lock = new object();
        private volatile DbProviderFactory _factory;

        protected DbFactoryBase(DbProviderFactory factory)
        {
            _factory = factory;
        }

        protected DbFactoryBase()
        {
        }

        /// <summary>
        /// Gets the DB provider factory
        /// </summary>
        public virtual DbProviderFactory Factory
        {
            get
            {
                if (_factory == null)
                {
                    lock (_lock)
                    {
                        if (_factory == null)
                        {
                            _factory = CreateFactory();
                        }
                    }
                }
                return _factory;
            }
        }

        protected abstract DbProviderFactory CreateFactory();
    }
}
