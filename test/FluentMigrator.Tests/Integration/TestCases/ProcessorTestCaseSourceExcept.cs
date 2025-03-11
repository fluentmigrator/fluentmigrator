#region License
// Copyright (c) 2024, Fluent Migrator Project
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

using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Tests.Integration.TestCases;

public class ProcessorTestCaseSourceExcept<TIgnore1>() : ProcessorTestCaseSource(true, typeof(TIgnore1))
    where TIgnore1 : ProcessorBase;

public class ProcessorTestCaseSourceExcept<TIgnore1, TIgnore2>() : ProcessorTestCaseSource(true, typeof(TIgnore1), typeof(TIgnore2))
    where TIgnore1 : ProcessorBase
    where TIgnore2 : ProcessorBase;

public class ProcessorTestCaseSourceExcept<TIgnore1, TIgnore2, TIgnore3>() : ProcessorTestCaseSource(true, typeof(TIgnore1), typeof(TIgnore2), typeof(TIgnore3))
    where TIgnore1 : ProcessorBase
    where TIgnore2 : ProcessorBase
    where TIgnore3 : ProcessorBase;

public class ProcessorTestCaseSourceExcept<TIgnore1, TIgnore2, TIgnore3, TIgnore4>() : ProcessorTestCaseSource(true, typeof(TIgnore1), typeof(TIgnore2), typeof(TIgnore3), typeof(TIgnore4))
    where TIgnore1 : ProcessorBase
    where TIgnore2 : ProcessorBase
    where TIgnore3 : ProcessorBase
    where TIgnore4 : ProcessorBase;

