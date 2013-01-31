using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner
{
    public class VersionOrderInvalidException : Exception
    {
        public IEnumerable<KeyValuePair<long, IMigrationInfo>> InvalidMigrations { get; set; }

        public IEnumerable<long> InvalidVersions { get; private set; }

        public VersionOrderInvalidException(IEnumerable<KeyValuePair<long, IMigrationInfo>> invalidMigrations)
        {
            InvalidMigrations = invalidMigrations;
        }

        public override string Message
        {
            get
            {
                var result = "Unapplied migrations have version numbers that are less than the greatest version number of applied migrations:";

                foreach (var pair in InvalidMigrations)
                {
                    result = result + string.Format("{0}{1} - {2}", Environment.NewLine, pair.Key, pair.Value.Migration.GetType().Name);
                }

                return result;
            }
        }
    }
}