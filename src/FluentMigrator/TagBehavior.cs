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

using System;

namespace FluentMigrator
{
    /// <summary>
    /// Specifies the behavior of a runner when evaluating a <see cref="TagsAttribute" />.
    /// </summary>
    public enum TagBehavior : int
    {
        /// <summary>
        /// The behavior is not specified.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// The runner will execute the tagged migration if all tag names are matched.
        /// </summary>
        RequireAll = 1,

        /// <summary>
        /// The runner will execute the tagged migration if any tag names are matched.
        /// </summary>
        RequireAny = 2
    }
}
