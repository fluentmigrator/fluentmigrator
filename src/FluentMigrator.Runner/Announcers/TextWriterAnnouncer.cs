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
using System.IO;

namespace FluentMigrator.Runner.Announcers
{
    public class TextWriterAnnouncer : BaseAnnouncer
    {
        public TextWriterAnnouncer(TextWriter writer)
            : this(writer.Write)
        {
        }

        public TextWriterAnnouncer(Action<string> write)
            : base(write)
        {
            NonSqlPrefix = "/* ";
			NonSqlWrapper = sql => string.Format("{0}{1} */", NonSqlPrefix, sql);
        }


        public string NonSqlPrefix { get; set; }

        public Func<string, string> NonSqlWrapper { get; set; }

        #region IAnnouncer Members

        public override void Heading(string message)
        {
            Write(NonSqlWrapper((message + " ").PadRight(75, '=')) + Environment.NewLine + Environment.NewLine);
        }

        public override void Say(string message)
        {
			Info(NonSqlWrapper(message));
        }

        public override void Sql(string sql)
        {
            if (!ShowSql)
                return;

            if (!string.IsNullOrEmpty(sql))
                Info(sql);
            else
                Say("No SQL statement executed.");
        }

        public override void ElapsedTime(TimeSpan timeSpan)
        {
            if (!ShowElapsedTime)
                return;

            Say(string.Format("-> {0}s", timeSpan.TotalSeconds));
            Write(Environment.NewLine);
        }

        public override void Error(string message)
        {
            Write(NonSqlWrapper("ERROR: " + message));
            Write(Environment.NewLine);
        }

        #endregion

        private void Info(string message)
        {
            Write(message);
            Write(Environment.NewLine);
        }
    }
}