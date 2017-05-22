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

using FluentMigrator.Model;

namespace FluentMigrator.Builders.Delete.Index
{
    public interface IDeleteIndexOptionsSyntax
    {
        /// <summary>
        /// Specifies whether underlying tables and associated indexes are available for queries and data modification during the index operation.
        /// The ONLINE option can only be specified when you drop clustered indexes (SQL Server 2016).
        /// </summary>
        /// <param name="mode">
        /// ON
        /// Long-term table locks are not held. This allows queries or updates to the underlying table to continue.
        /// OFF
        /// Table locks are applied and the table is unavailable for the duration of the index operation.
        /// </param>
        void ApplyOnline(OnlineMode mode = OnlineMode.On);
    }
}