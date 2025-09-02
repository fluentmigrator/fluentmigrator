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
    /// <summary>
    /// An announcer that delegates to multiple <see cref="IAnnouncer"/> instances.
    /// </summary>
    [Obsolete]
    public class CompositeAnnouncer : IAnnouncer
    {
        /// <inheritdoc />
        public CompositeAnnouncer(params IAnnouncer[] announcers)
        {
            Announcers = announcers ?? new IAnnouncer[0];
        }

        /// <summary>
        /// Gets the collection of announcers.
        /// </summary>
        public IEnumerable<IAnnouncer> Announcers { get; }

        /// <inheritdoc />
        public void Heading(string message)
        {
            Each(a => a.Heading(message));
        }

        /// <inheritdoc />
        public void Say(string message)
        {
            Each(a => a.Say(message));
        }

        /// <inheritdoc />
        public void Emphasize(string message)
        {
            Each(a => a.Emphasize(message));
        }

        /// <inheritdoc />
        public void Sql(string sql)
        {
            Each(a => a.Sql(sql));
        }

        /// <inheritdoc />
        public void ElapsedTime(TimeSpan timeSpan)
        {
            Each(a => a.ElapsedTime(timeSpan));
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            Each(a => a.Error(message));
        }

        /// <inheritdoc />
        public void Error(Exception exception)
        {
            while (exception != null)
            {
                Error(exception.Message);
                exception = exception.InnerException;
            }
        }

        /// <inheritdoc />
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
