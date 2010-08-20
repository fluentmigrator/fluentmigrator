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

using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Runner
{
	public interface IVersionLoader
	{
		VersionInfo VersionInfo { get; }
		void RemoveVersionTable();
	}

	public interface IProfileLoader
	{
		
	}

	public interface IMigrationVersionRunner
	{
		Assembly MigrationAssembly { get; }
		void MigrateUp();
		void MigrateUp(long version);
		void Rollback(int steps);
		void RollbackToVersion( long version );
		void MigrateDown(long version);
	}
}