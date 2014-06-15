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

namespace FluentMigrator.Helpers
{
    /// <summary>
    /// A pre-.NET4 Lazy<T> implementation
    /// </summary>
    internal class LazyLoader<T> where T : class
    {
        private readonly object _lock = new object();
        private Func<T> _function;
        private bool _hasRun;
        private T _value;

        public LazyLoader(Func<T> function)
        {
            if (function == null) throw new ArgumentNullException("function");

            _function = function;
        }

        public T Value
        {
            get
            {
                lock (_lock)
                {
                    if (!_hasRun)
                    {
                        _value = _function();
                        _hasRun = true;
                        _function = null;
                    }
                }

                return _value;
            }
        }
    }
}