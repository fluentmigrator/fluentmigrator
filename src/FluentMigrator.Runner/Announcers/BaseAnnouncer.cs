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

namespace FluentMigrator.Runner.Announcers
{
    public class BaseAnnouncer : IAnnouncer, IFormattingAnnouncer
    {
        protected readonly Action<string> Write;

        public BaseAnnouncer(Action<string> write)
        {
            Write = write;
            ShowSql = false;
            ShowElapsedTime = false;
        }

        public bool ShowSql { get; set; }
        public bool ShowElapsedTime { get; set; }

        #region IAnnouncer Members

        public virtual void Heading(string message)
        {
            Write(message);
        }

        public virtual void Say(string message)
        {
            Write(message);
        }

        public virtual void Heading(string message, params object[] args)
        {
            Heading(string.Format(message, args));
        }

        public virtual void Say(string message, params object[] args)
        {
            Say(string.Format(message, args));
        }

        public virtual void Sql(string sql)
        {
            if (!ShowSql)
                return;

            if (!string.IsNullOrEmpty(sql))
                Write(sql);
        }

        public virtual void ElapsedTime(TimeSpan timeSpan)
        {
            if (!ShowElapsedTime)
                return;

            Write(timeSpan.TotalSeconds.ToString());
        }

        public virtual void Error(string message)
        {
            Write(message);
        }

        public virtual void Error(string message, params object[] args)
        {
            Error(string.Format(message, args));
        }

        public virtual void Dispose()
        {
        }

        #endregion
    }
}