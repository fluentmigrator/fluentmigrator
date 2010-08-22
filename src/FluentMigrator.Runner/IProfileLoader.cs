using System.Collections.Generic;
using System.Reflection;

namespace FluentMigrator.Runner
{
	public interface IProfileLoader
	{
		void ApplyProfiles();
		IEnumerable<IMigration> FindProfilesIn(Assembly assembly, string profile);
	}
}