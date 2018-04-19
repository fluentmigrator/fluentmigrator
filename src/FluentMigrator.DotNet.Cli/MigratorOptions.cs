using System;
using System.Collections.Generic;
using System.Linq;

using FluentMigrator.DotNet.Cli.Commands;

namespace FluentMigrator.DotNet.Cli
{
    public class MigratorOptions
    {
        public MigratorOptions()
        {
        }

        private MigratorOptions(string task)
        {
            Task = task;
        }

        public string Task { get; }
        public string ConnectionString { get; private set; }
        public string ProcessorType { get; private set; }
        public string ProcessorSwitches { get; private set; }
        public IReadOnlyCollection<string> TargetAssemblies { get; private set; }
        public long? TargetVersion { get; private set; }
        public int? Steps { get; private set; }
        public string Namespace { get; private set; }
        public bool NestedNamespaces { get; private set; }
        public long? StartVersion { get; private set; }
        public bool NoConnection { get; private set; }
        public string ConnectionStringConfigPath { get; private set; }
        public string WorkingDirectory { get; private set; }
        public IEnumerable<string> Tags { get; private set; }
        public bool Preview { get; private set; }
        public bool Verbose { get; private set; }
        public string Profile { get; private set; }
        public string Context { get; private set; }
        public int? Timeout { get; private set; }
        public TransactionMode TransactionMode { get; private set; }
        public bool Output { get; private set; }
        public string OutputFileName { get; private set; }

        public bool ExecutingAgainstMsSql
            => ProcessorType.StartsWith("SqlServer", StringComparison.InvariantCultureIgnoreCase);

        public static MigratorOptions CreateListMigrations(ListMigrations cmd)
        {
            var result = new MigratorOptions("listmigrations")
                .Init(cmd);
            return result;
        }

        public static MigratorOptions CreateMigrateUp(Migrate cmd, long? targetVersion = null)
        {
            var result = new MigratorOptions("migrate:up")
                .Init(cmd);
            result.TargetVersion = targetVersion;
            result.TransactionMode = cmd.TransactionMode;
            return result;
        }

        public static MigratorOptions CreateMigrateDown(MigrateDown cmd)
        {
            var result = new MigratorOptions("migrate:down")
                .Init(cmd.Parent);
            result.TargetVersion = cmd.TargetVersion;
            result.TransactionMode = cmd.Parent.TransactionMode;
            return result;
        }

        public static MigratorOptions CreateRollbackBy(Rollback cmd, int? steps)
        {
            var result = new MigratorOptions("rollback")
                .Init(cmd);
            result.Steps = steps;
            result.TransactionMode = cmd.TransactionMode;
            return result;
        }

        public static MigratorOptions CreateRollbackTo(RollbackTo cmd)
        {
            var result = new MigratorOptions("rollback:toversion")
                .Init(cmd.Parent);
            result.TargetVersion = cmd.Version;
            result.TransactionMode = cmd.Parent.TransactionMode;
            return result;
        }

        public static MigratorOptions CreateRollbackAll(RollbackAll cmd)
        {
            var result = new MigratorOptions("rollback:all")
                .Init(cmd.Parent);
            result.TransactionMode = cmd.Parent.TransactionMode;
            return result;
        }

        public static MigratorOptions CreateValidateVersionOrder(ValidateVersionOrder cmd)
        {
            return new MigratorOptions("validateversionorder")
                .Init(cmd);
        }

        private MigratorOptions Init(ConnectionCommand cmd)
        {
            ConnectionString = cmd.ConnectionString;
            ConnectionStringConfigPath = cmd.ConnectionStringConfigPath;
            NoConnection = cmd.NoConnection;
            ProcessorType = cmd.ProcessorType;
            ProcessorSwitches = cmd.ProcessorSwitches;
            Preview = cmd.Preview;
            Verbose = cmd.Verbose;
            Profile = cmd.Profile;
            Context = cmd.Context;
            Timeout = cmd.Timeout;
            (Output, OutputFileName) = cmd.Output;
            return Init((MigrationCommand)cmd);
        }

        private MigratorOptions Init(MigrationCommand cmd)
        {
            TargetAssemblies = cmd.TargetAssemblies.ToList();
            Namespace = cmd.Namespace;
            NestedNamespaces = cmd.NestedNamespaces;
            StartVersion = cmd.StartVersion;
            WorkingDirectory = cmd.WorkingDirectory;
            Tags = cmd.Tags?.ToList() ?? new List<string>();
            return this;
        }
    }
}
