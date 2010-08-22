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
using System.Reflection;
using FluentMigrator.Infrastructure;
using System.Linq;

namespace FluentMigrator.Runner
{
	public class MigrationLoader : IMigrationLoader
	{
		public IMigrationConventions Conventions { get; private set; }
		public Assembly Assembly { get; set; }
		public string Namespace { get; set; }
		public SortedList<long, IMigration> Migrations { get; private set; }

		public MigrationLoader(IMigrationConventions conventions, Assembly assembly, string @namespace)
		{
			Conventions = conventions;
			Assembly = assembly;
			Namespace = @namespace;

			Initialize();
		}

		private void Initialize()
		{
			Migrations = new SortedList<long, IMigration>();

			IEnumerable<MigrationMetadata> migrationList = FindMigrations();

			if (migrationList == null)
				return;

			foreach (var migrationMetadata in migrationList)
			{
				if (Migrations.ContainsKey(migrationMetadata.Version))
					throw new Exception(String.Format("Duplicate migration version {0}.", migrationMetadata.Version));

				var migration = migrationMetadata.Type.Assembly.CreateInstance(migrationMetadata.Type.FullName);
				Migrations.Add(migrationMetadata.Version, migration as IMigration);
			}
		}

		public IEnumerable<MigrationMetadata> FindMigrations()
		{
			IEnumerable<Type> matchedTypes = Assembly.GetExportedTypes().Where(t => Conventions.TypeIsMigration(t));

			if (!string.IsNullOrEmpty(Namespace))
				matchedTypes = Assembly.GetExportedTypes().Where(t => t.Namespace == Namespace && Conventions.TypeIsMigration(t));

			foreach (Type type in matchedTypes)
				yield return Conventions.GetMetadataForMigration(type);
		}
	}
}