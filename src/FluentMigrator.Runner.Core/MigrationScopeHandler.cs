#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner
{
    public class MigrationScopeHandler : IMigrationScopeManager
    {
        private readonly IMigrationProcessor _processor;
        private readonly bool _previewOnly;

        public MigrationScopeHandler(IMigrationProcessor processor, ProcessorOptions processorOptions)
        {
            _processor = processor;
            _previewOnly = processorOptions.PreviewOnly;
        }

        public IMigrationScope CurrentScope { get; set; }

        public IMigrationScope BeginScope()
        {
            GuardAgainstActiveMigrationScope();
            CurrentScope = new TransactionalMigrationScope(_processor, () => CurrentScope = null);
            return CurrentScope;
        }

        public IMigrationScope CreateOrWrapMigrationScope(bool transactional = true)
        {
            // Prevent connection from being opened when --no-connection is specified in preview mode
            if (_previewOnly)
            {
                return new NoOpMigrationScope();
            }

            if (HasActiveMigrationScope) return new NoOpMigrationScope();
            if (transactional) return BeginScope();
            return new NoOpMigrationScope();
        }

        private void GuardAgainstActiveMigrationScope()
        {
            if (HasActiveMigrationScope) throw new InvalidOperationException("The runner is already in an active migration scope.");
        }

        private bool HasActiveMigrationScope => CurrentScope != null && CurrentScope.IsActive;
    }
}
