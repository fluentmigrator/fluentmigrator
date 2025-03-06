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

public class ProcessorTestCaseSourceOnly<TOnly1>() : ProcessorTestCaseSource(false, typeof(TOnly1))
    where TOnly1 : ProcessorBase;

public class ProcessorTestCaseSourceOnly<TOnly1, TOnly2>() : ProcessorTestCaseSource(false, typeof(TOnly1), typeof(TOnly2))
    where TOnly1 : ProcessorBase
    where TOnly2 : ProcessorBase;

public class ProcessorTestCaseSourceOnly<TOnly1, TOnly2, TOnly3>() : ProcessorTestCaseSource(false, typeof(TOnly1), typeof(TOnly2), typeof(TOnly3))
    where TOnly1 : ProcessorBase
    where TOnly2 : ProcessorBase
    where TOnly3 : ProcessorBase;

public class ProcessorTestCaseSourceOnly<TOnly1, TOnly2, TOnly3, TOnly4>() : ProcessorTestCaseSource(false, typeof(TOnly1), typeof(TOnly2), typeof(TOnly3), typeof(TOnly4))
    where TOnly1 : ProcessorBase
    where TOnly2 : ProcessorBase
    where TOnly3 : ProcessorBase
    where TOnly4 : ProcessorBase;
