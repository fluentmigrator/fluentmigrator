using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Runner.Versioning
{
	public class VersionInfo
	{
        public const string TABLE_NAME = "VersionInfo";
        public const string COLUMN_NAME = "Version";

	    private IList<long> _versionsApplied = new List<long>();

	    public long Latest()
	    {
	        return _versionsApplied.OrderByDescending(x => x).FirstOrDefault();
	    }

	    public void AddAppliedMigration(long migration)
        {
            _versionsApplied.Add(migration);
        }

        public bool HasAppliedMigration(long migration)
        {
            return _versionsApplied.Contains(migration);
        }
	}
}
