#region License

// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
    [Obsolete("Use DependencyInjection extension method chain instead: .AddLogging(lb => lb.AddDebug().AddFluentMigratorConsole())")]
    public class TextWriterAnnouncer : Announcer
    {
        private readonly Action<string> _write;

        public TextWriterAnnouncer(TextWriter writer)
            : this(writer.Write)
        {
        }

        public TextWriterAnnouncer(Action<string> write)
        {
            _write = write;
        }

        public TextWriterAnnouncer(IOptions<TextWriterAnnouncerOptions> options)
            : base(options)
        {
            _write = options.Value.WriteDelegate;
        }

        public override void Heading(string message)
        {
            base.Heading(string.Format("{0} ", message).PadRight(75, '='));
            _write(Environment.NewLine);
        }

        public override void ElapsedTime(TimeSpan timeSpan)
        {
            base.ElapsedTime(timeSpan);
            _write(Environment.NewLine);
        }

        public override void Write(string message, bool isNotSql = true)
        {
            _write(isNotSql ? string.Format("/* {0} */", message) : message);
            _write(Environment.NewLine);
        }
    }
}
