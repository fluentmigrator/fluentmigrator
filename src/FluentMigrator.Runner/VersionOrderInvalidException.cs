using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Infrastructure;

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
                var result = "Unapplied migrations have version numbers that are less than the greatest version number of applied migrations:";

                foreach (var migration in InvalidMigrations)
                {
                    var value = migration.Value is MigrationWithMetaDataAdapter ? ((MigrationWithMetaDataAdapter)migration.Value).Migration : migration.Value;

                    result = result + string.Format("{0}{1} - {2}", Environment.NewLine, migration.Key, value.GetType().Name);
                }

                return result;
            }
        }
    }
}