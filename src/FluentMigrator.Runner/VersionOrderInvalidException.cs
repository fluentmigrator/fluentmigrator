using System;
using System.Collections.Generic;

namespace FluentMigrator.Runner
{
	public class VersionOrderInvalidException : Exception
	{
		public VersionOrderInvalidException(IEnumerable<long> invalidVersions)
		{
			InvalidVersions = invalidVersions;
		}

		public IEnumerable<long> InvalidVersions { get; private set; }

		public override string Message
		{
			get { return "Unapplied migrations have version numbers that are less that greatest version number of applied migrations."; }
		}
	}
}