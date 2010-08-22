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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
	public class MigrationVersionRunner
	{
		private MigrationRunner _migrationRunner;

		private IMigrationConventions _migrationConventions;
		private IMigrationProcessor _migrationProcessor;

		
		private string _namespace;
		private readonly IAnnouncer _announcer;
		
		private IVersionTableMetaData _versionTableMetaData;
		private SortedList<long, IMigration> _migrations;
		private bool _alreadyOutputPreviewOnlyModeWarning;

		private VersionInfo _versionInfo;
		private IMigration _versionMigration;
		private string _profile;
		private IEnumerable<IMigration> _profiles;
		private bool _alreadyCreatedVersionTable;

		public MigrationVersionRunner(IMigrationConventions conventions, IMigrationProcessor processor, IMigrationLoader loader, IAnnouncer announcer)
			: this(conventions, processor, loader, Assembly.GetCallingAssembly(), null, announcer, string.Empty)
		{
		}

		public MigrationVersionRunner(IMigrationConventions conventions, IMigrationProcessor processor, IMigrationLoader loader, Type getAssemblyByType, IAnnouncer announcer)
			: this(conventions, processor, loader, getAssemblyByType.Assembly, null, announcer, string.Empty)
		{
		}

		public MigrationVersionRunner(IMigrationConventions conventions, IMigrationProcessor processor, IMigrationLoader loader, Assembly assembly, string @namespace, IAnnouncer announcer, string profile)
		{
			_migrationLoader = loader;
			_profile = profile;
			_namespace = @namespace;
			_versionTableMetaData = loader.GetVersionTableMetaData(assembly);
			_versionMigration = new VersionMigration(_versionTableMetaData);

			
			LoadVersionInfo();
		}
	}
}
