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

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Announcers
{
    /// <summary>
    /// Base class for announcers in FluentMigrator.
    /// </summary>
    [Obsolete]
    public abstract class Announcer : IAnnouncer
    {
        /// <inheritdoc />
        public virtual bool ShowSql { get; set; }
        /// <inheritdoc />
        public virtual bool ShowElapsedTime { get; set; }

        /// <inheritdoc />
        protected Announcer()
        {
        }

        /// <inheritdoc />
        protected Announcer(IOptions<AnnouncerOptions> options)
        {
            // ReSharper disable VirtualMemberCallInConstructor
            ShowSql = options.Value.ShowSql;
            ShowElapsedTime = options.Value.ShowElapsedTime;
            // ReSharper restore VirtualMemberCallInConstructor
        }

        /// <inheritdoc />
        public virtual void Heading(string message)
        {
            Write(message);
        }

        /// <inheritdoc />
        public virtual void Say(string message)
        {
            Write(message);
        }

        /// <inheritdoc />
        public virtual void Emphasize(string message)
        {
            Write(message);
        }

        /// <inheritdoc />
        public virtual void Sql(string sql)
        {
            if (!ShowSql) return;

            if (string.IsNullOrEmpty(sql))
                Write("No SQL statement executed.");
            else Write(sql, false);
        }

        /// <inheritdoc />
        public virtual void ElapsedTime(TimeSpan timeSpan)
        {
            if (!ShowElapsedTime) return;

            Write(string.Format("=> {0}s", timeSpan.TotalSeconds));
        }

        /// <inheritdoc />
        public virtual void Error(Exception exception)
        {
            while (exception != null)
            {
                Error(exception.Message);
                exception = exception.InnerException;
            }
        }

        /// <inheritdoc />
        public virtual void Error(string message)
        {
            Write(string.Format("!!! {0}", message));
        }

        /// <inheritdoc />
        public abstract void Write(string message, bool isNotSql = true);
    }
}
