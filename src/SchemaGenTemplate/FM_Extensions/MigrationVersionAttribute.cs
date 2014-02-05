using System;
using System.Diagnostics;

namespace Migrations.FM_Extensions
{
    /// <summary>
    /// Computes a migration number based on product version numbering + migration step
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MigrationVersionAttribute : FluentMigrator.MigrationAttribute
    {
        public MigrationVersionAttribute(int major, int minor, int patch = 0, int step = 0)
            : base(CalculateValue(major, minor, patch, step))
        {
        }

        private static long CalculateValue(int major, int minor, int patch, int step)
        {
            Debug.Assert(major < 100);
            Debug.Assert(minor < 100);
            Debug.Assert(patch < 100);
            Debug.Assert(step < 1000);

            return ((((major * 100L) + minor) * 100L) + patch) * 1000L + step;
        }
    }
}
