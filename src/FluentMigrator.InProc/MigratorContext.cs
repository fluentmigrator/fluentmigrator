#region Copyright (c) 2011, Agile Utilities New Zealand Ltd.
// Copyright (c) 2011, Agile Utilities New Zealand Ltd., http://www.agileutilities.com
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
#endregion

using System.IO;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;

namespace FluentMigrator.InProc {
   public class MigratorContext : IMigratorContext {
      public MigratorContext(TextWriter announcerOutput, bool verbose) {
         Announcer = new TextWriterAnnouncer(announcerOutput) {
            ShowElapsedTime = verbose,
            ShowSql = verbose
         };
      }

      public MigratorContext(TextWriter announcerOutput)
          : this(announcerOutput, true)
      { 
      }

      public Assembly MigrationsAssembly { get; set; }
      public int Timeout { get; set; }
      public string Connection { get; set; }
      public string Database { get; set; }
      public IAnnouncer Announcer { get; private set; }
      public bool PreviewOnly { get; set; }
      public string Profile { get; set; }
   }
}
