using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Runner
{
    public class VersionOrderInvalidException : Exception
    {
        public IEnumerable<KeyValuePair<long, IMigration>> InvalidMigrations { get; set; }

        public IEnumerable<long> InvalidVersions { get; private set; }

        public VersionOrderInvalidException(IEnumerable<long> invalidVersions)
        {
            InvalidVersions = invalidVersions;
        }

        public VersionOrderInvalidException(IEnumerable<KeyValuePair<long, IMigration>> invalidMigrations)
        {
            InvalidMigrations = invalidMigrations;
        }

        public override string Message
        {
            get
            {
                return InvalidMigrations.Aggregate(
                    "Unapplied migrations have version numbers that are less than the greatest version number of applied migrations:",
                    (current, kvp) => current + string.Format("{0}{1} - {2}", Environment.NewLine, kvp.Key, kvp.Value.GetType().Name));
            }
        }
    }
}