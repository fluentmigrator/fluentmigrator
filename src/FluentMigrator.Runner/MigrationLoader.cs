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
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Runner
{
	public class MigrationLoader : IMigrationLoader
	{
		public IMigrationConventions Conventions { get; private set; }

		public MigrationLoader(IMigrationConventions conventions)
		{
			Conventions = conventions;
		}

		public IEnumerable<MigrationMetadata> FindMigrationsIn(Assembly assembly, string @namespace)
		{
			IEnumerable<Type> matchedTypes = assembly.GetExportedTypes().Where(t => Conventions.TypeIsMigration(t));

			if (!string.IsNullOrEmpty(@namespace))
				matchedTypes = assembly.GetExportedTypes().Where(t => t.Namespace == @namespace && Conventions.TypeIsMigration(t));

			foreach (Type type in matchedTypes)
				yield return Conventions.GetMetadataForMigration(type);
		}

		public IEnumerable<IMigration> FindProfilesIn(Assembly assembly, string profile)
		{
			IEnumerable<Type> matchedTypes = assembly.GetExportedTypes().Where(t => Conventions.TypeIsProfile(t));

			foreach (Type type in matchedTypes)
			{
				string profileName = type.GetOneAttribute<ProfileAttribute>().ProfileName;
				if (profileName.ToLower() == profile.ToLower())
					yield return type.Assembly.CreateInstance(type.FullName) as IMigration;
			}
		}

		public IVersionTableMetaData GetVersionTableMetaData(Assembly assembly)
		{
			Type matchedType = assembly.GetExportedTypes().Where(t => Conventions.TypeIsVersionTableMetaData(t)).FirstOrDefault();

			if (matchedType == null)
			{
				return new DefaultVersionTableMetaData();
			}

			return (IVersionTableMetaData)Activator.CreateInstance(matchedType);
		}
	}
}