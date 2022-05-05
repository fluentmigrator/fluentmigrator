#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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
using System.IO;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Announcers
{
    [Obsolete]
    public class TextWriterWithGoAnnouncer : TextWriterAnnouncer
    {
        public TextWriterWithGoAnnouncer(TextWriter writer)
            : base(writer)
        { }

        public  TextWriterWithGoAnnouncer(Action<string> write)
            : base(write)
        { }

        public  TextWriterWithGoAnnouncer(IOptions<TextWriterAnnouncerOptions> options)
            : base(options)
        { }

        public override void Sql(string sql)
        {
            if (!ShowSql) return;

            base.Sql(sql);

            if (!string.IsNullOrEmpty(sql))
                Write("GO", false);
        }
    }
}
