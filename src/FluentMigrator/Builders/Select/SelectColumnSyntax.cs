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

using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Select
{
    public class SelectColumnSyntax : ISelectColumnSyntax
    {
        private readonly IMigrationContext _context;
        private readonly List<string> _columns;
        private readonly string _table;

        public SelectColumnSyntax(IMigrationContext context, string table)
        {
            _context = context;
            _table = table;

            _columns = new List<string>();
        }

        public ISelectColumnSyntax Column(string columnName)
        {
            _columns.Add(columnName);
            return this;
        }

        public ISelectColumnSyntax AllColumns
        {
            get
            {
                _columns.AddRange(_context.QuerySchema.GetColumnInfos(null, _table).Select(x => x.Name));
                return this;
            }
        }

        public DataSet Read()
        {
            var sql = ((IMigrationProcessor) _context.QuerySchema).BuildSelect(_table, _columns);
            return ((IMigrationProcessor)_context.QuerySchema).Read(sql);
        }
    }
}
