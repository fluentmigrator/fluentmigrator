using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Runner
{
    public class MigrationVersionRunner
    {
        private Assembly asm;
        private VersionInfo versionInfo;

        public VersionInfo Version 
        {
            get
            {
                if (versionInfo == null) LoadVersionInfo();
                return versionInfo;
            }
            private set { versionInfo = value; }
        }

        private SortedList<long, Migration> migrations;
        public SortedList<long, Migration> Migrations 
        {
            get
            {
                if (migrations == null) LoadAssemblyMigrations();
                return migrations;
            }
            private set { migrations = value; }
        }

        public MigrationConventions Conventions { get; private set; }
        public IMigrationProcessor Processor { get; private set; }

        public MigrationVersionRunner(MigrationConventions conventions, IMigrationProcessor processor, Assembly asm)
		{
			Conventions = conventions;
			Processor = processor;
            this.asm = asm;
            this.Version = null;
            this.Migrations = null;
		}

        public MigrationVersionRunner(MigrationConventions conventions, IMigrationProcessor processor, Type getAssemblyByType)
        {
            Conventions = conventions;
            Processor = processor;
            this.asm = getAssemblyByType.Assembly;
            this.Version = null;
            this.Migrations = null;
        }

        public void LoadVersionInfo()
        {
            //ensure table exists
            if (!Processor.TableExists(VersionInfo.TABLE_NAME))
            {
                //need to load version info
                var runner = new MigrationRunner(Conventions, Processor);
                runner.Up(new VersionMigration());
            }

            //fetch info
            var ds = Processor.ReadTableData(VersionInfo.TABLE_NAME);
            var row = ds.Tables[0].Rows[0];

            //set variable
            Version = new VersionInfo(int.Parse(row["CurrentVersion"].ToString()),
                int.Parse(row["PreviousVersion"].ToString()), DateTime.Parse(row["LastUpdated"].ToString()));
        }

        public void LoadAssemblyMigrations()
        {
            Migrations = new SortedList<long, Migration>();
            var loader = new MigrationLoader(Conventions);
            IEnumerable<MigrationMetadata> migrationList = loader.FindMigrationsIn(asm);

            var en = migrationList.GetEnumerator();
            while (en.MoveNext())
            {
                if (Migrations.ContainsKey(en.Current.Version))
                    throw new Exception(String.Format("Duplicate migration version {0}.", en.Current.Version) );

                //create instance of migration class and add to list
                var mig = en.Current.Type.Assembly.CreateInstance(en.Current.Type.FullName);
                Migrations.Add(en.Current.Version, mig as Migration);
            }
        }

        public long CurrentVersion
        {
            get { return Version.CurrentVersion; }
        }

        public long LastVersion
        {
            get { return Version.PreviousVersion; }
        }

        public void StepUp(long fromVersion, long toVersion, out long lastVersionAttempted)
        {
            //set steps
            int fromStep = Migrations.IndexOfKey(fromVersion);
            int toStep = Migrations.IndexOfKey(toVersion);

            //track last version atempted
            lastVersionAttempted = fromVersion;

            if (fromStep > toStep) throw new Exception(String.Format("Version {0} is greater than the target version {1}", fromVersion, toVersion));
            if (fromStep == toStep) return; //nothing to do

            //runer to execture schema Up()
            var runner = new MigrationRunner(Conventions, Processor);

            int step = fromStep;
            while (step < toStep)
            {
                long nextStepVersion = Migrations.Keys[step + 1];
                var nextMigration = Migrations[nextStepVersion];
                //step up to next version
                runner.Up(nextMigration);
                lastVersionAttempted = nextStepVersion;
                step++;
            }

            //save version info
            SaveVersionState(lastVersionAttempted, CurrentVersion);
        }

        public void StepDown(long fromVersion, long toVersion, out long lastVersionAttempted)
        {
            //set steps
            int fromStep = Migrations.IndexOfKey(fromVersion);
            int toStep = (toVersion==0) ? -1 : Migrations.IndexOfKey(toVersion);

            //track last version atempted
            lastVersionAttempted = fromVersion;

            if (fromStep < toStep) throw new Exception(String.Format("Version {0} is less than the target version {1}", fromVersion, toVersion));
            if (fromStep == toStep) return; //nothing to do

            //runer to execture schema Up()
            var runner = new MigrationRunner(Conventions, Processor);

            int step = fromStep;
            while (step > toStep)
            {
                long nextStepVersion = Migrations.Keys[step];
                var nextMigration = Migrations[nextStepVersion];
                //step up to next version
                runner.Down(nextMigration);
                lastVersionAttempted = nextStepVersion;
                step--;
            }

            if (step < 0) lastVersionAttempted = 0;

            //save version info
            SaveVersionState(lastVersionAttempted, CurrentVersion);
        }

        public void UpgradeToVersion(long number, bool autoRollback)
        {
            if (!Migrations.ContainsKey(number)) throw new Exception(String.Format("Version {0} is missing.", number));
            if (CurrentVersion != 0 && !Migrations.ContainsKey(CurrentVersion)) throw new Exception(String.Format("Current version {0} is not defined.", CurrentVersion));
            
            //runer to execture schema Up()
            var runner = new MigrationRunner(Conventions, Processor);

            long lastVersionRun = 0;
            try
            {
                StepUp(CurrentVersion, number, out lastVersionRun);
            }
            catch (Exception er)
            {
                //gracefully handle rollback
                if (lastVersionRun != 0 && autoRollback)
                {
                    long rollbackVersion = 0;
                    StepDown(lastVersionRun, CurrentVersion, out rollbackVersion);
                }
                throw;
            }
        }

        public void SaveVersionState(long currentVersion, long previousVersion)
        {
            //save
            Processor.UpdateTable(VersionInfo.TABLE_NAME, new List<string>() { "CurrentVersion", "PreviousVersion", "LastUpdated" },
                new List<string>() { String.Format("'{0}'", currentVersion), String.Format("'{0}'", previousVersion), String.Format("'{0}'", DateTime.UtcNow.ToString()) });

            //load versionInfo
            LoadVersionInfo();
        }
    }
}
