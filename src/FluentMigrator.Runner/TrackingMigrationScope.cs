#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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
    public class TrackingMigrationScope : IMigrationScope
    {
        private readonly Action _disposalAction;

        public TrackingMigrationScope(Action disposalAction)
        {
            _disposalAction = disposalAction;
            IsActive = true;
        }

        public void Dispose()
        {
            Cancel();
            _disposalAction?.Invoke();
        }

        public virtual bool IsActive { get; private set; }

        public void Complete()
        {
            if (!IsActive) return;
            DoComplete();
            IsActive = false;
        }

        public void Cancel()
        {
            if (!IsActive) return;
            DoCancel();
            IsActive = false;
        }

        protected virtual void DoComplete()
        {
        }

        protected virtual void DoCancel()
        {
        }
    }
}
