#region License
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Diagnostics;

namespace FluentMigrator.Runner.Announcers
{
    public class TraceSourceAnnouncer : IAnnouncer
    {
        public const string TraceSourceName = "FluentMigrator";

        private const SourceLevels DefaultSourceLevels = SourceLevels.Verbose; // logging to trace source is always manually enabled, so no point in limiting levels
        // Verbose=1xxx Information=2xxx Warning=3xxx Error=4xxx Critical=5xxx
        private const int SqlMessageId = 1000;
        private const int ElapsedTimeMessageId = 1001;
        private const int GenericMessageId = 2000;
        private const int HeaderMessageId = 2001;
        private const int ImportantMessageId = 2002;
        private const int ErrorMesageId = 4000;

        private static readonly object _sync = new object();
        private static volatile TraceSource _traceSource;

        public TraceSourceAnnouncer()
        {
            ShowElapsedTime = false; // timestamps are part of trace already, no need to duplicate information
        }

        public bool ShowElapsedTime { get; set; }

        public static TraceSource TraceSource
        {
            get
            {
                if (_traceSource == null)
                    lock (_sync)
                        if (_traceSource == null)
                            _traceSource = new TraceSource(TraceSourceName, DefaultSourceLevels);
                return _traceSource;
            }
        }

        public void Heading(string message)
        {
            TraceSource.TraceEvent(TraceEventType.Information, HeaderMessageId, message);
        }

        public void Say(string message)
        {
            TraceSource.TraceEvent(TraceEventType.Information, GenericMessageId, message);
        }

        public void Emphasize(string message)
        {
            TraceSource.TraceEvent(TraceEventType.Information, ImportantMessageId, message);
        }

        public void Sql(string sql)
        {
            TraceSource.TraceEvent(TraceEventType.Verbose, SqlMessageId, string.IsNullOrEmpty(sql) ? "No SQL statement executed." : sql);
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
            if (ShowElapsedTime)
                TraceSource.TraceEvent(TraceEventType.Verbose, ElapsedTimeMessageId, string.Format("Elapsed: {0}s", timeSpan.TotalSeconds));
        }

        public void Error(string message)
        {
            TraceSource.TraceEvent(TraceEventType.Error, ErrorMesageId, message);
        }

        void IAnnouncer.Write(string message, bool escaped) // should not be in the interface
        {
            throw new NotSupportedException("Implementation detail of Announcer class, must not be used.");
        }
    }
}