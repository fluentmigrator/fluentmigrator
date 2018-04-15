#region License
// Copyright (c) 2007-2018, Sean Chambers and the FluentMigrator Project
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

using FluentMigrator.Runner;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.Extensions.Options;

namespace FluentMigrator.DotNet.Cli.CustomAnnouncers
{
    public class ParserConsoleAnnouncer : IAnnouncer
    {
        private readonly IConsole _console;

        private readonly CustomAnnouncerOptions _options;

        public ParserConsoleAnnouncer(IConsole console, IOptions<CustomAnnouncerOptions> options)
        {
            _console = console;
            _options = options.Value;
        }

        public bool ShowElapsedTime => _options.ShowElapsedTime;

        public bool ShowSql => _options.ShowSql;

        public void Heading(string message)
        {
            _console.ForegroundColor = ConsoleColor.Green;
            HorizontalRule();
            Write(message);
            HorizontalRule();
            _console.ResetColor();
        }

        public void Say(string message)
        {
            _console.ForegroundColor = ConsoleColor.White;
            Write(message);
            _console.ResetColor();
        }

        public void Emphasize(string message)
        {
            _console.ForegroundColor = ConsoleColor.Yellow;
            Say(string.Format("[+] {0}", message));
            _console.ResetColor();
        }

        public void Sql(string sql)
        {
            if (!_options.ShowSql)
                return;

            if (string.IsNullOrEmpty(sql))
            {
                Write("No SQL statement executed.");
            }
            else
            {
                Write(sql, false);
            }
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
            if (!_options.ShowElapsedTime)
                return;

            _console.ResetColor();
            Write(string.Format("=> {0}s", timeSpan.TotalSeconds));
        }

        public void Error(string message)
        {
            _console.ForegroundColor = ConsoleColor.Red;
            Write(message);
            _console.ResetColor();
        }

        public void Error(Exception exception)
        {
            while (exception != null)
            {
                Error(exception.Message);
                exception = exception.InnerException;
            }
        }

        public void Write(string message, bool isNotSql = true)
        {
            _console.WriteLine(message);
        }

        private void HorizontalRule()
        {
            Write("".PadRight(79, '-'));
        }
    }
}
