﻿#region Copyright (c) 2011, Agile Utilities New Zealand Ltd.
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

using System.Reflection;
using FluentMigrator.Runner;

namespace FluentMigrator.InProc {
   public interface IMigratorContext {
      Assembly MigrationsAssembly { get; set; }
      int Timeout { get; set; }
      string Connection { get; set; }
      string Database { get; set; }
      IAnnouncer Announcer { get; }
      bool PreviewOnly { get; set; }
   }
}
