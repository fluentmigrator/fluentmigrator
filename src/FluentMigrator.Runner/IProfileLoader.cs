using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner
{
    public interface IProfileLoader
    {
        void ApplyProfiles();
        IEnumerable<IMigration> FindProfilesIn(IAssemblyCollection assemblies, string profile);
    }
}