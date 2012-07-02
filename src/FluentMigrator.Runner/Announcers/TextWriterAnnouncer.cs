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
using System.IO;

namespace FluentMigrator.Runner.Announcers
{
    public class TextWriterAnnouncer : Announcer
    {
        private readonly Action<string> write;

        public TextWriterAnnouncer(TextWriter writer)
            : this(writer.Write)
        {
        }

        public TextWriterAnnouncer(Action<string> write)
        {
            this.write = write;
        }

        public override void Heading(string message)
        {
            base.Heading(string.Format("{0} ", message).PadRight(75, '='));
            write(Environment.NewLine);
        }

        public override void ElapsedTime(TimeSpan timeSpan)
        {
            base.ElapsedTime(timeSpan);
            write(Environment.NewLine);
        }

        public override void Write(string message, bool escaped)
        {
            write(escaped ? string.Format("/* {0} */", message) : message);
            write(Environment.NewLine);
        }
    }
}