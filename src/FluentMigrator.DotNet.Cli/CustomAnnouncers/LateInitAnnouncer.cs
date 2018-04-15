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
using System.IO;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;

using Microsoft.Extensions.Options;

namespace FluentMigrator.DotNet.Cli.CustomAnnouncers
{
    public class LateInitAnnouncer : IAnnouncer, IDisposable
    {
        private Lazy<IAnnouncer> _innerAnnouncer;
        private TextWriter _writer;

        public LateInitAnnouncer(
            IOptions<MigratorOptions> migratorOptions)
        {
            var opt = migratorOptions.Value;
            OutputFileName = opt.OutputFileName;

            _innerAnnouncer = new Lazy<IAnnouncer>(() =>
            {
                if (!opt.Output)
                {
                    return new NullAnnouncer();
                }

                _writer = new StreamWriter(OutputFileName);
                var result = opt.ExecutingAgainstMsSql
                    ? new TextWriterWithGoAnnouncer(_writer)
                    : new TextWriterAnnouncer(_writer);
                result.ShowElapsedTime = false;
                result.ShowSql = true;
                return result;
            });
        }

        public string OutputFileName { get; set; }

        public void Heading(string message)
        {
            _innerAnnouncer.Value.Heading(message);
        }

        public void Say(string message)
        {
            _innerAnnouncer.Value.Say(message);
        }

        public void Emphasize(string message)
        {
            _innerAnnouncer.Value.Emphasize(message);
        }

        public void Sql(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                Say(sql);
            }
            else
            {
                _innerAnnouncer.Value.Sql(sql);
            }
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
            _innerAnnouncer.Value.ElapsedTime(timeSpan);
        }

        public void Error(string message)
        {
            _innerAnnouncer.Value.Error(message);
        }

        public void Error(Exception exception)
        {
            _innerAnnouncer.Value.Error(exception);
        }

        [Obsolete]
        public void Write(string message, bool isNotSql = true)
        {
            _innerAnnouncer.Value.Write(message, isNotSql);
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
