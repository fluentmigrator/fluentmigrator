#region License
//
// Copyright (c) 2018, FluentMigrator Project
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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;

namespace FluentMigrator.Console
{
    class LateInitAnnouncer : IAnnouncer, IDisposable
    {
        private readonly bool _executeAgainstMsSql;
        private readonly bool _verbose;
        private readonly ConsoleAnnouncer _consoleAnnouncer;

        private StreamWriter _streamWriter;

        private IAnnouncer _innerAnnouncer;

        private IAnnouncer InnerAnnouncer => _innerAnnouncer ?? (_innerAnnouncer = InitInnerAnnouncer());

        public LateInitAnnouncer(ConsoleAnnouncer consoleAnnouncer, bool executeAgainstMsSql, bool verbose,
            string outputTo)
        {
            _executeAgainstMsSql = executeAgainstMsSql;
            _verbose = verbose;
            OutputTo = outputTo;
            _consoleAnnouncer = consoleAnnouncer;
        }

        public string OutputTo { get; set; }

        public void Heading(string message)
        {
            InnerAnnouncer.Heading(message);
        }

        public void Say(string message)
        {
            InnerAnnouncer.Say(message);
        }

        public void Emphasize(string message)
        {
            InnerAnnouncer.Emphasize(message);
        }

        public void Sql(string sql)
        {
            InnerAnnouncer.Sql(sql);
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
            InnerAnnouncer.ElapsedTime(timeSpan);
        }

        public void Error(string message)
        {
            InnerAnnouncer.Error(message);
        }

        public void Error(Exception exception)
        {
            InnerAnnouncer.Error(exception);
        }

        [Obsolete]
        public void Write(string message, bool isNotSql)
        {
            InnerAnnouncer.Write(message, isNotSql);
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
        }

        private IAnnouncer InitInnerAnnouncer()
        {
            var sw = new StreamWriter(GetOutputFileName());
            var fileAnnouncer = _executeAgainstMsSql ? new TextWriterWithGoAnnouncer(sw) : new TextWriterAnnouncer(sw);

            fileAnnouncer.ShowElapsedTime = false;
            fileAnnouncer.ShowSql = true;

            _consoleAnnouncer.ShowElapsedTime = _verbose;
            _consoleAnnouncer.ShowSql = _verbose;

            var announcer = new CompositeAnnouncer(_consoleAnnouncer, fileAnnouncer);
            _streamWriter = sw;
            return announcer;
        }

        private string GetOutputFileName()
        {
            return OutputTo;
        }
    }
}
