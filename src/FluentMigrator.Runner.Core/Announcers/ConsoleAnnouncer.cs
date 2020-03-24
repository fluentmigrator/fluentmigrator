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

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Announcers
{
    [Obsolete]
    public class ConsoleAnnouncer : Announcer
    {
        public ConsoleAnnouncer()
        {
        }

        public ConsoleAnnouncer(IOptions<AnnouncerOptions> options)
            : base(options)
        {
        }

        public void Header()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            HorizontalRule();
            Write("=============================== FluentMigrator ================================");
            HorizontalRule();
            Write("Source Code:");
            Write("  https://github.com/fluentmigrator/fluentmigrator");
            Write("Ask For Help:");
            Write("  https://gitter.im/FluentMigrator/fluentmigrator");
            HorizontalRule();
            Console.ResetColor();
        }

        public void HorizontalRule()
        {
            Write("".PadRight(79, '-'));
        }

        public override void Heading(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            HorizontalRule();
            base.Heading(message);
            HorizontalRule();
            Console.ResetColor();
        }

        public override void Say(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            base.Say(string.Format("[+] {0}", message));
            Console.ResetColor();
        }

        public override void Emphasize(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            base.Say(string.Format("[+] {0}", message));
            Console.ResetColor();
        }

        public override void ElapsedTime(TimeSpan timeSpan)
        {
            Console.ResetColor();
            base.ElapsedTime(timeSpan);
        }

        public override void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("!!! {0}", message);
            Console.ResetColor();
        }

        public override void Write(string message, bool isNotSql = true)
        {
            Console.Out.WriteLine(message);
        }
    }
}
