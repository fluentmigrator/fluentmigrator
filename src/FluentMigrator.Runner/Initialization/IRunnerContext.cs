#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator.Runner.Initialization
{
    public interface IRunnerContext
    {
        string Database { get; set; }
        string Connection { get; set; }
        string Target { get; set; }
        bool PreviewOnly { get; set; }
        string Namespace { get; set; }
        string Task { get; set; }
        long Version { get; set; }
        int Steps { get; set; }
        string WorkingDirectory { get; set; }
        string Profile { get; set; }
        IAnnouncer Announcer { get; }
        IStopWatch StopWatch { get; }
        int Timeout { get; set; }
        string ConnectionStringConfigPath { get; set; }

        /// <summary>The arbitrary application context passed to the task runner.</summary>
        object ApplicationContext { get; set; }
    }
}