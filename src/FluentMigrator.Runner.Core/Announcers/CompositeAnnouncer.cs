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
using System.Collections.Generic;

namespace FluentMigrator.Runner.Announcers
{
    [Obsolete]
    public class CompositeAnnouncer : IAnnouncer
    {
        public CompositeAnnouncer(params IAnnouncer[] announcers)
        {
            Announcers = announcers ?? new IAnnouncer[0];
        }

        public IEnumerable<IAnnouncer> Announcers { get; }

        public void Heading(string message)
        {
            Each(a => a.Heading(message));
        }

        public void Say(string message)
        {
            Each(a => a.Say(message));
        }

        public void Emphasize(string message)
        {
            Each(a => a.Emphasize(message));
        }

        public void Sql(string sql)
        {
            Each(a => a.Sql(sql));
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
            Each(a => a.ElapsedTime(timeSpan));
        }

        public void Error(string message)
        {
            Each(a => a.Error(message));
        }

        public void Error(Exception exception)
        {
            while (exception != null)
            {
                Error(exception.Message);
                exception = exception.InnerException;
            }
        }

        [Obsolete]
        public void Write(string message, bool isNotSql)
        {
            Each(a => a.Write(message, isNotSql));
        }

        private void Each(Action<IAnnouncer> action)
        {
            foreach (var announcer in Announcers)
                action(announcer);
        }
    }
}
