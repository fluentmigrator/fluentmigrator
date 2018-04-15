#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using FluentMigrator.Runner.BatchParser;
using FluentMigrator.Runner.BatchParser.Sources;

namespace FluentMigrator.Tests.Unit.BatchParser
{
    public class LinesSourceTests : SourceTestsBase
    {
        public override ITextSource CreateSource(string content)
        {
            string[] lines;
            if (content.Length == 0)
            {
                lines = new string[0];
            }
            else if (content.EndsWith("\n"))
            {
                lines = content.Substring(0, content.Length - 1).Split('\n');
            }
            else
            {
                lines = content.Split('\n');
            }

            return new LinesSource(lines);
        }
    }
}
