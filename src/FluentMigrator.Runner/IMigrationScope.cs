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

namespace FluentMigrator.Runner
{
    public interface IMigrationScope : IDisposable
    {
        bool IsActive { get; }
        void Complete();
        void Cancel();
    }

    public class TransactionalMigrationScope : IMigrationScope
    {
        private readonly IMigrationProcessor _migrationProcessor;

        public TransactionalMigrationScope(IMigrationProcessor migrationProcessor)
        {
            if (migrationProcessor == null) throw new ArgumentNullException("migrationProcessor");
            _migrationProcessor = migrationProcessor;
            _migrationProcessor.BeginTransaction();
            IsActive = true;
        }

        public void Dispose()
        {
            Cancel();
        }

        public bool IsActive { get; set; }

        public void Complete()
        {
            if (!IsActive) return;
            _migrationProcessor.CommitTransaction();
            IsActive = false;
        }

        public void Cancel()
        {
            if (!IsActive) return;
            _migrationProcessor.RollbackTransaction();
            IsActive = false;
        }
    }

    public class NullMigrationScope : IMigrationScope
    {
        public void Dispose()
        {
        }

        public bool IsActive
        {
            get { return true; }
        }

        public void Complete()
        {
        }

        public void Cancel()
        {
        }
    }
}