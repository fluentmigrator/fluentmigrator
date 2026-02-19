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
using System.IO;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Announcers
{
    /// <summary>
    /// An announcer that writes output to a <see cref="TextWriter"/>.
    /// </summary>
    [Obsolete("Use DependencyInjection extension method chain instead: .AddLogging(lb => lb.AddDebug().AddFluentMigratorConsole())")]
    public class TextWriterAnnouncer : Announcer
    {
        private readonly Action<string> _write;

        /// <inheritdoc />
        public TextWriterAnnouncer(TextWriter writer)
            : this(writer.Write)
        {
        }

        /// <inheritdoc />
        public TextWriterAnnouncer(Action<string> write)
        {
            _write = write;
        }

        /// <inheritdoc />
        public TextWriterAnnouncer(IOptions<TextWriterAnnouncerOptions> options)
            : base(options)
        {
            _write = options.Value.WriteDelegate;
        }

        /// <inheritdoc />
        public override void Heading(string message)
        {
            base.Heading(string.Format("{0} ", message).PadRight(75, '='));
            _write(Environment.NewLine);
        }

        /// <inheritdoc />
        public override void ElapsedTime(TimeSpan timeSpan)
        {
            base.ElapsedTime(timeSpan);
            _write(Environment.NewLine);
        }

        /// <inheritdoc />
        public override void Write(string message, bool isNotSql = true)
        {
            _write(isNotSql ? string.Format("/* {0} */", message) : message);
            _write(Environment.NewLine);
        }
    }
}
