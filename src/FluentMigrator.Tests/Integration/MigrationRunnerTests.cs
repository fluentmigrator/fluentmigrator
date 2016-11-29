#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2010, Nathan Brown
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion


using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Tests.Integration.Migrations;
using FluentMigrator.Tests.Integration.Migrations.Tagged;
using FluentMigrator.Tests.Unit;
using FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Integration
{
    [TestFixture]
    [Category("Integration")]
    public class MigrationRunnerTests : IntegrationTestBase
    {
        private IRunnerContext _runnerContext;

        [SetUp]
        public void SetUp()
        {
            _runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
                                        {
                                            Namespace = "FluentMigrator.Tests.Integration.Migrations"
                                        };
        }

        [Test]
        public void CanRunMigration()
        {
            ExecuteWithSupportedProcessors(processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigration());

                    processor.TableExists(null, "TestTable").ShouldBeTrue();

                    // This is a hack until MigrationVersionRunner and MigrationRunner are refactored and merged together
                    //processor.CommitTransaction();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.TableExists(null, "TestTable").ShouldBeFalse();
                });
        }

        [Test]
        public void CanSilentlyFail()
        {
            try
            {
                var processorOptions = new Mock<IMigrationProcessorOptions>();
                processorOptions.SetupGet(x => x.PreviewOnly).Returns(false);

                var processor = new Mock<IMigrationProcessor>();
                processor.Setup(x => x.Process(It.IsAny<CreateForeignKeyExpression>())).Throws(new Exception("Error"));
                processor.Setup(x => x.Process(It.IsAny<DeleteForeignKeyExpression>())).Throws(new Exception("Error"));
                processor.Setup(x => x.Options).Returns(processorOptions.Object);

                var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor.Object) { SilentlyFail = true };

                runner.Up(new TestForeignKeySilentFailure());

                runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);

                runner.Down(new TestForeignKeySilentFailure());
                runner.CaughtExceptions.Count.ShouldBeGreaterThan(0);
            }
            finally
            {
                ExecuteWithSupportedProcessors(processor =>
                {
                    MigrationRunner testRunner = SetupMigrationRunner(processor);
                    testRunner.RollbackToVersion(0);
                }, false);
            }
        }

        [Test]
        public void CanApplyForeignKeyConvention()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestForeignKeyNamingConvention());
                    processor.ConstraintExists(null, "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeTrue();

                    runner.Down(new TestForeignKeyNamingConvention());
                    processor.ConstraintExists(null, "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeFalse();
                }, false, typeof(SQLiteProcessor));
        }

        [Test]
        public void CanApplyForeignKeyConventionWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestForeignKeyNamingConventionWithSchema());

                    processor.ConstraintExists("TestSchema", "Users", "FK_Users_GroupId_Groups_GroupId").ShouldBeTrue();
                    runner.Down(new TestForeignKeyNamingConventionWithSchema());
                }, false, new []{typeof(SQLiteProcessor), typeof(FirebirdProcessor)});
        }

        [Test]
        public void CanApplyIndexConvention()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestIndexNamingConvention());
                    processor.IndexExists(null, "Users", "IX_Users_GroupId").ShouldBeTrue();
                    processor.TableExists(null, "Users").ShouldBeTrue();

                    runner.Down(new TestIndexNamingConvention());
                    processor.IndexExists(null, "Users", "IX_Users_GroupId").ShouldBeFalse();
                    processor.TableExists(null, "Users").ShouldBeFalse();
                });
        }

        [Test]
        public void CanApplyIndexConventionWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestIndexNamingConventionWithSchema());
                    processor.IndexExists("TestSchema", "Users", "IX_Users_GroupId").ShouldBeTrue();
                    processor.TableExists("TestSchema", "Users").ShouldBeTrue();

                    runner.Down(new TestIndexNamingConventionWithSchema());
                    processor.IndexExists("TestSchema", "Users", "IX_Users_GroupId").ShouldBeFalse();
                    processor.TableExists("TestSchema", "Users").ShouldBeFalse();
                });
        }

        [Test]
        public void CanCreateAndDropIndex()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigration());
                    processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Up(new TestCreateAndDropIndexMigration());
                    processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropIndexMigration());
                    processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.IndexExists(null, "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    //processor.CommitTransaction();
                });
        }

        [Test]
        public void CanCreateAndDropIndexWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Up(new TestCreateAndDropIndexMigrationWithSchema());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropIndexMigrationWithSchema());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());
                    processor.IndexExists("TestSchema", "TestTable", "IX_TestTable_Name").ShouldBeFalse();

                    runner.Down(new TestCreateSchema());
                    //processor.CommitTransaction();
                }, false, new[] { typeof(SQLiteProcessor), typeof(FirebirdProcessor) });
        }

        [Test]
        public void CanRenameTable()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigration());
                    processor.TableExists(null, "TestTable2").ShouldBeTrue();

                    runner.Up(new TestRenameTableMigration());
                    processor.TableExists(null, "TestTable2").ShouldBeFalse();
                    processor.TableExists(null, "TestTable'3").ShouldBeTrue();

                    runner.Down(new TestRenameTableMigration());
                    processor.TableExists(null, "TestTable'3").ShouldBeFalse();
                    processor.TableExists(null, "TestTable2").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.TableExists(null, "TestTable2").ShouldBeFalse();

                    //processor.CommitTransaction();
                });
        }


        [Test]
        public void CanRenameTableWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeTrue();

                    runner.Up(new TestRenameTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeFalse();
                    processor.TableExists("TestSchema", "TestTable'3").ShouldBeTrue();

                    runner.Down(new TestRenameTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable'3").ShouldBeFalse();
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable2").ShouldBeFalse();

                    runner.Down(new TestCreateSchema());

                    //processor.CommitTransaction();
                });
        }

        [Test]
        public void CanRenameColumn()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigration());
                    processor.ColumnExists(null, "TestTable2", "Name").ShouldBeTrue();

                    runner.Up(new TestRenameColumnMigration());
                    processor.ColumnExists(null, "TestTable2", "Name").ShouldBeFalse();
                    processor.ColumnExists(null, "TestTable2", "Name'3").ShouldBeTrue();

                    runner.Down(new TestRenameColumnMigration());
                    processor.ColumnExists(null, "TestTable2", "Name'3").ShouldBeFalse();
                    processor.ColumnExists(null, "TestTable2", "Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigration());
                    processor.ColumnExists(null, "TestTable2", "Name").ShouldBeFalse();
                }, true, typeof(SQLiteProcessor));
        }

        [Test]
        public void CanRenameColumnWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeTrue();

                    runner.Up(new TestRenameColumnMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeFalse();
                    processor.ColumnExists("TestSchema", "TestTable2", "Name'3").ShouldBeTrue();

                    runner.Down(new TestRenameColumnMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name'3").ShouldBeFalse();
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name").ShouldBeFalse();

                    runner.Down(new TestCreateSchema());
                }, true, typeof(SQLiteProcessor), typeof(FirebirdProcessor));
        }

        [Test]
        public void CanLoadMigrations()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
                {
                    Namespace = typeof(TestMigration).Namespace,
                };

                var runner = new MigrationRunner(typeof(MigrationRunnerTests).Assembly, runnerContext, processor);

                //runner.Processor.CommitTransaction();

                runner.MigrationLoader.LoadMigrations().ShouldNotBeNull();
            });
        }

        [Test]
        public void CanLoadVersion()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
                {
                    Namespace = typeof(TestMigration).Namespace,
                };

                var runner = new MigrationRunner(typeof(TestMigration).Assembly, runnerContext, processor);

                //runner.Processor.CommitTransaction();
                runner.VersionLoader.VersionInfo.ShouldNotBeNull();
            });
        }

        [Test]
        public void CanRunMigrations()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                MigrationRunner runner = SetupMigrationRunner(processor);

                runner.MigrateUp(false);

                runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
                runner.VersionLoader.VersionInfo.HasAppliedMigration(2).ShouldBeTrue();
                runner.VersionLoader.VersionInfo.HasAppliedMigration(3).ShouldBeTrue();
                runner.VersionLoader.VersionInfo.HasAppliedMigration(4).ShouldBeTrue();
                runner.VersionLoader.VersionInfo.HasAppliedMigration(5).ShouldBeTrue();
                runner.VersionLoader.VersionInfo.Latest().ShouldBe(5);

                runner.RollbackToVersion(0, false);
            });
        }

        [Test]
        public void CanMigrateASpecificVersion()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                MigrationRunner runner = SetupMigrationRunner(processor);
                try
                {
                    runner.MigrateUp(1, false);

                    runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
                    processor.TableExists(null, "Users").ShouldBeTrue();
                }
                finally
                {
                    runner.RollbackToVersion(0, false);
                }
            });
        }

        [Test]
        public void CanMigrateASpecificVersionDown()
        {
            try
            {
                ExecuteWithSupportedProcessors(processor =>
                {
                    MigrationRunner runner = SetupMigrationRunner(processor);

                    runner.MigrateUp(1, false);

                    runner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeTrue();
                    processor.TableExists(null, "Users").ShouldBeTrue();

                    MigrationRunner testRunner = SetupMigrationRunner(processor);
                    testRunner.MigrateDown(0, false);
                    testRunner.VersionLoader.VersionInfo.HasAppliedMigration(1).ShouldBeFalse();
                    processor.TableExists(null, "Users").ShouldBeFalse();
                }, false, typeof(SQLiteProcessor));
            }
            finally
            {
                ExecuteWithSupportedProcessors(processor =>
                {
                    MigrationRunner testRunner = SetupMigrationRunner(processor);
                    testRunner.RollbackToVersion(0, false);
                }, false);
            }
        }

        [Test]
        public void RollbackAllShouldRemoveVersionInfoTable()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                MigrationRunner runner = SetupMigrationRunner(processor);

                runner.MigrateUp(2);

                processor.TableExists(runner.VersionLoader.VersionTableMetaData.SchemaName, runner.VersionLoader.VersionTableMetaData.TableName).ShouldBeTrue();
            });

            ExecuteWithSupportedProcessors(processor =>
            {
                MigrationRunner runner = SetupMigrationRunner(processor);
                runner.RollbackToVersion(0);

                processor.TableExists(runner.VersionLoader.VersionTableMetaData.SchemaName, runner.VersionLoader.VersionTableMetaData.TableName).ShouldBeFalse();
            });
        }

        [Test]
        public void MigrateUpWithSqlServerProcessorShouldCommitItsTransaction()
        {
            if (!IntegrationTestOptions.SqlServer2008.IsEnabled)
                return;

            var connection = new SqlConnection(IntegrationTestOptions.SqlServer2008.ConnectionString);
            var processor = new SqlServerProcessor(connection, new SqlServer2008Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerDbFactory());

            MigrationRunner runner = SetupMigrationRunner(processor);
            runner.MigrateUp();

            try
            {
                processor.WasCommitted.ShouldBeTrue();

            }
            finally
            {
                CleanupTestSqlServerDatabase(connection, processor);
            }
        }

        [Test]
        public void MigrateUpSpecificVersionWithSqlServerProcessorShouldCommitItsTransaction()
        {
            if (!IntegrationTestOptions.SqlServer2008.IsEnabled)
                return;

            var connection = new SqlConnection(IntegrationTestOptions.SqlServer2008.ConnectionString);
            var processor = new SqlServerProcessor(connection, new SqlServer2008Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerDbFactory());

            MigrationRunner runner = SetupMigrationRunner(processor);
            runner.MigrateUp(1);

            try
            {
                processor.WasCommitted.ShouldBeTrue();

            }
            finally
            {
                CleanupTestSqlServerDatabase(connection, processor);
            }
        }

        [Test]
        public void MigrateUpWithTaggedMigrationsShouldOnlyApplyMatchedMigrations()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                var assembly = typeof(TenantATable).Assembly;

                var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
                {
                    Namespace = typeof(TenantATable).Namespace,
                    Tags = new[] { "TenantA" }
                };

                var runner = new MigrationRunner(assembly, runnerContext, processor);

                try
                {
                    runner.MigrateUp(false);

                    processor.TableExists(null, "TenantATable").ShouldBeTrue();
                    processor.TableExists(null, "NormalTable").ShouldBeTrue();
                    processor.TableExists(null, "TenantBTable").ShouldBeFalse();
                    processor.TableExists(null, "TenantAandBTable").ShouldBeTrue();
                }
                finally
                {
                    runner.RollbackToVersion(0);
                }
            });
        }

        [Test]
        public void MigrateUpWithTaggedMigrationsAndUsingMultipleTagsShouldOnlyApplyMatchedMigrations()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                var assembly = typeof(TenantATable).Assembly;

                var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
                {
                    Namespace = typeof(TenantATable).Namespace,
                    Tags = new[] { "TenantA", "TenantB" }
                };

                var runner = new MigrationRunner(assembly, runnerContext, processor);
                
                try
                {
                    runner.MigrateUp(false);

                    processor.TableExists(null, "TenantATable").ShouldBeFalse();
                    processor.TableExists(null, "NormalTable").ShouldBeTrue();
                    processor.TableExists(null, "TenantBTable").ShouldBeFalse();
                    processor.TableExists(null, "TenantAandBTable").ShouldBeTrue();
                }
                finally
                {
                    new MigrationRunner(assembly, runnerContext, processor).RollbackToVersion(0);
                }
            });
        }

        [Test]
        public void MigrateUpWithDifferentTaggedShouldIgnoreConcreteOfTagged()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                var assembly = typeof(TenantATable).Assembly;

                var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
                {
                    Namespace = typeof(TenantATable).Namespace,
                    Tags = new[] { "TenantB" }
                };

                var runner = new MigrationRunner(assembly, runnerContext, processor);

                try
                {
                    runner.MigrateUp(false);

                    processor.TableExists(null, "TenantATable").ShouldBeFalse();
                    processor.TableExists(null, "NormalTable").ShouldBeTrue();
                    processor.TableExists(null, "TenantBTable").ShouldBeTrue();
                }
                finally
                {
                    new MigrationRunner(assembly, runnerContext, processor).RollbackToVersion(0);
                }
            });
        }

        [Test]
        public void MigrateDownWithDifferentTagsToMigrateUpShouldApplyMatchedMigrations()
        {
            var assembly = typeof(TenantATable).Assembly;
            var migrationsNamespace = typeof(TenantATable).Namespace;

            var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
            {
                Namespace = migrationsNamespace,
            };

            // Excluded SqliteProcessor as it errors on DB cleanup (RollbackToVersion).
            ExecuteWithSupportedProcessors(processor =>
            {
                try
                {
                    runnerContext.Tags = new[] { "TenantA" };
                    
                    new MigrationRunner(assembly, runnerContext, processor).MigrateUp(false);

                    processor.TableExists(null, "TenantATable").ShouldBeTrue();
                    processor.TableExists(null, "NormalTable").ShouldBeTrue();
                    processor.TableExists(null, "TenantBTable").ShouldBeFalse();
                    processor.TableExists(null, "TenantAandBTable").ShouldBeTrue();

                    runnerContext.Tags = new[] { "TenantB" };

                    new MigrationRunner(assembly, runnerContext, processor).MigrateDown(0, false);

                    processor.TableExists(null, "TenantATable").ShouldBeTrue();
                    processor.TableExists(null, "NormalTable").ShouldBeFalse();
                    processor.TableExists(null, "TenantBTable").ShouldBeFalse();
                    processor.TableExists(null, "TenantAandBTable").ShouldBeFalse();
                }
                finally
                {
                    runnerContext.Tags = new[] { "TenantA" };

                    new MigrationRunner(assembly, runnerContext, processor).RollbackToVersion(0, false);
                }
            }, true, typeof(SQLiteProcessor));
        }

        [Test]
        public void VersionInfoCreationScriptsOnlyGeneratedOnceInPreviewMode()
        {
            if (!IntegrationTestOptions.SqlServer2008.IsEnabled)
                return;

            var connection = new SqlConnection(IntegrationTestOptions.SqlServer2008.ConnectionString);
            var processorOptions = new ProcessorOptions { PreviewOnly = true };

            var outputSql = new StringWriter();
            var announcer = new TextWriterAnnouncer(outputSql){ ShowSql = true };

            var processor = new SqlServerProcessor(connection, new SqlServer2008Generator(), announcer, processorOptions, new SqlServerDbFactory());

            try
            {
                var asm = typeof(MigrationRunnerTests).Assembly;
                var runnerContext = new RunnerContext(announcer)
                {
                    Namespace = "FluentMigrator.Tests.Integration.Migrations",
                    PreviewOnly = true
                };
                
                var runner = new MigrationRunner(asm, runnerContext, processor);
                runner.MigrateUp(1, false);

                processor.CommitTransaction();

                string schemaName = new TestVersionTableMetaData().SchemaName;
                var schemaAndTableName = string.Format("\\[{0}\\]\\.\\[{1}\\]", schemaName, TestVersionTableMetaData.TABLENAME);

                var outputSqlString = outputSql.ToString();

                var createSchemaMatches = new Regex(string.Format("CREATE SCHEMA \\[{0}\\]", schemaName)).Matches(outputSqlString).Count;
                var createTableMatches = new Regex("CREATE TABLE " + schemaAndTableName).Matches(outputSqlString).Count;
                var createIndexMatches = new Regex("CREATE UNIQUE CLUSTERED INDEX \\[" + TestVersionTableMetaData.UNIQUEINDEXNAME + "\\] ON " + schemaAndTableName).Matches(outputSqlString).Count;
                var alterTableMatches = new Regex("ALTER TABLE " + schemaAndTableName).Matches(outputSqlString).Count;

                System.Console.WriteLine(outputSqlString);

                createSchemaMatches.ShouldBe(1);
                createTableMatches.ShouldBe(1);
                alterTableMatches.ShouldBe(1);
                createIndexMatches.ShouldBe(1);
                
            }
            finally
            {
                CleanupTestSqlServerDatabase(connection, processor);
            }
        }

        [Test]
        public void MigrateUpWithTaggedMigrationsShouldNotApplyAnyMigrationsIfNoTagsParameterIsPassedIntoTheRunner()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                var assembly = typeof(TenantATable).Assembly;

                var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
                {
                    Namespace = typeof(TenantATable).Namespace
                };

                var runner = new MigrationRunner(assembly, runnerContext, processor);

                try
                {
                    runner.MigrateUp(false);

                    processor.TableExists(null, "TenantATable").ShouldBeFalse();
                    processor.TableExists(null, "NormalTable").ShouldBeTrue();
                    processor.TableExists(null, "TenantBTable").ShouldBeFalse();
                    processor.TableExists(null, "TenantAandBTable").ShouldBeFalse();
                }
                finally
                {
                    runner.RollbackToVersion(0);
                }
            });
        }

        [Test]
        public void ValidateVersionOrderShouldDoNothingIfUnappliedMigrationVersionIsGreaterThanLatestAppliedMigration()
        {

            // Using SqlServer instead of SqlLite as versions not deleted from VersionInfo table when using Sqlite.
            var excludedProcessors = new[] { typeof(SQLiteProcessor), typeof(MySqlProcessor), typeof(PostgresProcessor) };

            var assembly = typeof(User).Assembly;
            
            var runnerContext1 = new RunnerContext(new TextWriterAnnouncer(System.Console.Out)) { Namespace = typeof(Migrations.Interleaved.Pass2.User).Namespace };
            var runnerContext2 = new RunnerContext(new TextWriterAnnouncer(System.Console.Out)) { Namespace = typeof(Migrations.Interleaved.Pass3.User).Namespace };

            try
            {
                ExecuteWithSupportedProcessors(processor =>
                {
                    var migrationRunner = new MigrationRunner(assembly, runnerContext1, processor);

                    migrationRunner.MigrateUp(3);
                }, false, excludedProcessors);

                ExecuteWithSupportedProcessors(processor =>
                {
                    var migrationRunner = new MigrationRunner(assembly, runnerContext2, processor);

                    Assert.DoesNotThrow(migrationRunner.ValidateVersionOrder);
                }, false, excludedProcessors);
            }
            finally
            {
                ExecuteWithSupportedProcessors(processor =>
                {
                    var migrationRunner = new MigrationRunner(assembly, runnerContext2, processor);
                    migrationRunner.RollbackToVersion(0);
                }, true, excludedProcessors);
            }
        }

        [Test]
        public void ValidateVersionOrderShouldThrowExceptionIfUnappliedMigrationVersionIsLessThanLatestAppliedMigration()
        {

            // Using SqlServer instead of SqlLite as versions not deleted from VersionInfo table when using Sqlite.
            var excludedProcessors = new[] { typeof(SQLiteProcessor), typeof(MySqlProcessor), typeof(PostgresProcessor) };

            var assembly = typeof(User).Assembly;

            var runnerContext1 = new RunnerContext(new TextWriterAnnouncer(System.Console.Out)) { Namespace = typeof(Migrations.Interleaved.Pass2.User).Namespace };
            var runnerContext2 = new RunnerContext(new TextWriterAnnouncer(System.Console.Out)) { Namespace = typeof(Migrations.Interleaved.Pass3.User).Namespace };

            VersionOrderInvalidException caughtException = null;

            try
            {
                ExecuteWithSupportedProcessors(processor =>
                {
                    var migrationRunner = new MigrationRunner(assembly, runnerContext1, processor);
                    migrationRunner.MigrateUp();
                }, false, excludedProcessors);

                ExecuteWithSupportedProcessors(processor =>
                {
                    var migrationRunner = new MigrationRunner(assembly, runnerContext2, processor);
                    migrationRunner.ValidateVersionOrder();
                }, false, excludedProcessors);
            }
            catch (VersionOrderInvalidException ex)
            {
                caughtException = ex;
            }
            finally
            {
                ExecuteWithSupportedProcessors(processor =>
                {
                    var migrationRunner = new MigrationRunner(assembly, runnerContext2, processor);
                    migrationRunner.RollbackToVersion(0);
                }, true, excludedProcessors);
            }

            caughtException.ShouldNotBeNull();


            caughtException.InvalidMigrations.Count().ShouldBe(1);
            var keyValuePair = caughtException.InvalidMigrations.First();
            keyValuePair.Key.ShouldBe(200909060935);
            keyValuePair.Value.Migration.ShouldBeOfType<UserEmail>();
        }

        [Test]
        public void CanCreateSequence()
        {
            ExecuteWithSqlServer2012(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSequence());
                    processor.SequenceExists(null, "TestSequence");

                    runner.Down(new TestCreateSequence());
                    processor.SequenceExists(null, "TestSequence").ShouldBeFalse();
                }, true);
        }

        [Test]
        public void CanCreateSequenceWithSchema()
        {
            Action<IMigrationProcessor> action = processor =>
                                {
                                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                                    runner.Up(new TestCreateSequence());
                                    processor.SequenceExists("TestSchema", "TestSequence");

                                    runner.Down(new TestCreateSequence());
                                    processor.SequenceExists("TestSchema", "TestSequence").ShouldBeFalse();
                                };

            ExecuteWithSqlServer2012(
                action,true);

            ExecuteWithPostgres(action, IntegrationTestOptions.Postgres, true);
        }

        [Test]
        public void CanAlterColumnWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name2").ShouldBeTrue();
                    processor.DefaultValueExists("TestSchema", "TestTable", "Name", "Anonymous").ShouldBeTrue();


                    runner.Up(new TestAlterColumnWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name2").ShouldBeTrue();

                    runner.Down(new TestAlterColumnWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "Name2").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                }, true, new[] { typeof(SQLiteProcessor), typeof(FirebirdProcessor) });
        }

        [Test]
        public void CanAlterTableWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "NewColumn").ShouldBeFalse();

                    runner.Up(new TestAlterTableWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "NewColumn").ShouldBeTrue();

                    runner.Down(new TestAlterTableWithSchema());
                    processor.ColumnExists("TestSchema", "TestTable2", "NewColumn").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                }, true, new[] { typeof(SQLiteProcessor), typeof(FirebirdProcessor) });
        }

        [Test]
        public void CanAlterTablesSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.TableExists("TestSchema", "TestTable").ShouldBeTrue();

                    runner.Up(new TestAlterSchema());
                    processor.TableExists("NewSchema", "TestTable").ShouldBeTrue();

                    runner.Down(new TestAlterSchema());
                    processor.TableExists("TestSchema", "TestTable").ShouldBeTrue();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                }, true, new[] { typeof(SQLiteProcessor), typeof(FirebirdProcessor) });
        }

        [Test]
        public void CanCreateUniqueConstraint()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigration());
                    processor.ConstraintExists(null, "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Up(new TestCreateUniqueConstraint());
                    processor.ConstraintExists(null, "TestTable2", "TestUnique").ShouldBeTrue();

                    runner.Down(new TestCreateUniqueConstraint());
                    processor.ConstraintExists(null, "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigration());

                }, true, typeof(SQLiteProcessor));
        }

        [Test]
        public void CanCreateUniqueConstraintWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    processor.ConstraintExists("TestSchema", "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Up(new TestCreateUniqueConstraintWithSchema());
                    processor.ConstraintExists("TestSchema", "TestTable2", "TestUnique").ShouldBeTrue();

                    runner.Down(new TestCreateUniqueConstraintWithSchema());
                    processor.ConstraintExists("TestSchema", "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                }, true, new[] { typeof(SQLiteProcessor), typeof(FirebirdProcessor) });
        }

        [Test]
        public void CanInsertData()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigration());
                    DataSet ds = processor.ReadTableData(null, "TestTable");
                    ds.Tables[0].Rows.Count.ShouldBe(1);
                    ds.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigration());

                }, true, new[] { typeof(SQLiteProcessor) });
        }

        [Test]
        public void CanInsertDataWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());
                    DataSet ds = processor.ReadTableData("TestSchema", "TestTable");
                    ds.Tables[0].Rows.Count.ShouldBe(1);
                    ds.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                }, true, new[] { typeof(SQLiteProcessor), typeof(FirebirdProcessor) });
        }

        [Test]
        public void CanUpdateData()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Up(new TestUpdateData());
                    DataSet upDs = processor.ReadTableData("TestSchema", "TestTable");
                    upDs.Tables[0].Rows.Count.ShouldBe(1);
                    upDs.Tables[0].Rows[0][1].ShouldBe("Updated");

                    runner.Down(new TestUpdateData());
                    DataSet downDs = processor.ReadTableData("TestSchema", "TestTable");
                    downDs.Tables[0].Rows.Count.ShouldBe(1);
                    downDs.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                }, true, new[] { typeof(SQLiteProcessor), typeof(FirebirdProcessor) });
        }

        [Test]
        public void CanDeleteData()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigration());

                    runner.Up(new TestDeleteData());
                    DataSet upDs = processor.ReadTableData(null, "TestTable");
                    upDs.Tables[0].Rows.Count.ShouldBe(0);

                    runner.Down(new TestDeleteData());
                    DataSet downDs = processor.ReadTableData(null, "TestTable");
                    downDs.Tables[0].Rows.Count.ShouldBe(1);
                    downDs.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigration());

                }, true, new[] { typeof(SQLiteProcessor) });
        }

        [Test]
        public void CanDeleteDataWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Up(new TestDeleteDataWithSchema());
                    DataSet upDs = processor.ReadTableData("TestSchema", "TestTable");
                    upDs.Tables[0].Rows.Count.ShouldBe(0);

                    runner.Down(new TestDeleteDataWithSchema());
                    DataSet downDs = processor.ReadTableData("TestSchema", "TestTable");
                    downDs.Tables[0].Rows.Count.ShouldBe(1);
                    downDs.Tables[0].Rows[0][1].ShouldBe("Test");

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                }, true, new[] { typeof(SQLiteProcessor), typeof(FirebirdProcessor) });
        }

        [Test]
        public void CanReverseCreateIndex()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Up(new TestCreateIndexWithReversing());
                    processor.IndexExists("TestSchema", "TestTable2", "IX_TestTable2_Name2").ShouldBeTrue();

                    runner.Down(new TestCreateIndexWithReversing());
                    processor.IndexExists("TestSchema", "TestTable2", "IX_TestTable2_Name2").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());
                }, true, new[] { typeof(SQLiteProcessor) });
        }

        [Test]
        public void CanReverseCreateUniqueConstraint()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateAndDropTableMigration());

                    runner.Up(new TestCreateUniqueConstraintWithReversing());
                    processor.ConstraintExists(null, "TestTable2", "TestUnique").ShouldBeTrue();

                    runner.Down(new TestCreateUniqueConstraintWithReversing());
                    processor.ConstraintExists(null, "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigration());

                }, true, new[] { typeof(SQLiteProcessor) });
        }

        [Test]
        public void CanReverseCreateUniqueConstraintWithSchema()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestCreateSchema());

                    runner.Up(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Up(new TestCreateUniqueConstraintWithSchemaWithReversing());
                    processor.ConstraintExists("TestSchema", "TestTable2", "TestUnique").ShouldBeTrue();

                    runner.Down(new TestCreateUniqueConstraintWithSchemaWithReversing());
                    processor.ConstraintExists("TestSchema", "TestTable2", "TestUnique").ShouldBeFalse();

                    runner.Down(new TestCreateAndDropTableMigrationWithSchema());

                    runner.Down(new TestCreateSchema());

                }, true, new[] { typeof(SQLiteProcessor) });
        }

        [Test]
        public void CanExecuteSql()
        {
            ExecuteWithSupportedProcessors(
                processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), _runnerContext, processor);

                    runner.Up(new TestExecuteSql());
                    runner.Down(new TestExecuteSql());

                }, true, new[] { typeof(FirebirdProcessor) });
        }

        private static MigrationRunner SetupMigrationRunner(IMigrationProcessor processor)
        {
            Assembly asm = typeof(MigrationRunnerTests).Assembly;
            var runnerContext = new RunnerContext(new TextWriterAnnouncer(System.Console.Out))
            {
                Namespace = "FluentMigrator.Tests.Integration.Migrations"
            };

            return new MigrationRunner(asm, runnerContext, processor);
        }

        private static void CleanupTestSqlServerDatabase(SqlConnection connection, SqlServerProcessor origProcessor)
        {
            if (origProcessor.WasCommitted)
            {
                connection.Close();

                var cleanupProcessor = new SqlServerProcessor(connection, new SqlServer2008Generator(), new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions(), new SqlServerDbFactory());
                MigrationRunner cleanupRunner = SetupMigrationRunner(cleanupProcessor);
                cleanupRunner.RollbackToVersion(0);

            }
            else
            {
                origProcessor.RollbackTransaction();
            }
        }
    }


    internal class TestForeignKeyNamingConvention : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Table("Groups")
                .WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(32).NotNullable();

            Create.ForeignKey().FromTable("Users").ForeignColumn("GroupId").ToTable("Groups").PrimaryColumn("GroupId");
        }

        public override void Down()
        {
            Delete.Table("Users");
            Delete.Table("Groups");
        }
    }

    internal class TestIndexNamingConvention : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Index().OnTable("Users").OnColumn("GroupId").Ascending();
        }

        public override void Down()
        {
            Delete.Index("IX_Users_GroupId").OnTable("Users").OnColumn("GroupId");
            Delete.Table("Users");
        }
    }

    internal class TestForeignKeySilentFailure : Migration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Table("Groups")
                .WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(32).NotNullable();

            Create.ForeignKey("FK_Foo").FromTable("Users").ForeignColumn("GroupId").ToTable("Groups").PrimaryColumn("GroupId");
        }

        public override void Down()
        {
            Delete.ForeignKey("FK_Foo").OnTable("Users");
            Delete.Table("Users");
            Delete.Table("Groups");
        }
    }

    internal class TestCreateAndDropTableMigration : Migration
    {
        public override void Up()
        {
            Create.Table("TestTable")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable().WithDefaultValue("Anonymous");

            Create.Table("TestTable2")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).Nullable()
                .WithColumn("TestTableId").AsInt32().NotNullable();

            Create.Index("ix_Name").OnTable("TestTable2").OnColumn("Name").Ascending()
                .WithOptions().NonClustered();

            Create.Column("Name2").OnTable("TestTable2").AsBoolean().Nullable();

            Create.ForeignKey("fk_TestTable2_TestTableId_TestTable_Id")
                .FromTable("TestTable2").ForeignColumn("TestTableId")
                .ToTable("TestTable").PrimaryColumn("Id");

            Insert.IntoTable("TestTable").Row(new { Name = "Test" });
        }

        public override void Down()
        {
            Delete.Table("TestTable2");
            Delete.Table("TestTable");
        }
    }

    internal class TestRenameTableMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Table("TestTable2").To("TestTable'3");
        }
    }

    internal class TestRenameColumnMigration : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Column("Name").OnTable("TestTable2").To("Name'3");
        }
    }

    internal class TestCreateAndDropIndexMigration : Migration
    {
        public override void Up()
        {
            Create.Index("IX_TestTable_Name").OnTable("TestTable").OnColumn("Name");
        }

        public override void Down()
        {
            Delete.Index("IX_TestTable_Name").OnTable("TestTable");
        }
    }


    internal class TestForeignKeyNamingConventionWithSchema : Migration
    {
        public override void Up()
        {
            Create.Schema("TestSchema");
            Create.Table("Users")
                .InSchema("TestSchema")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Table("Groups")
                .InSchema("TestSchema")
                .WithColumn("GroupId").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(32).NotNullable();

            Create.ForeignKey().FromTable("Users").InSchema("TestSchema").ForeignColumn("GroupId").ToTable("Groups").InSchema("TestSchema").PrimaryColumn("GroupId");
        }

        public override void Down()
        {
            Delete.Table("Users").InSchema("TestSchema");
            Delete.Table("Groups").InSchema("TestSchema");
            Delete.Schema("TestSchema");
        }
    }

    internal class TestIndexNamingConventionWithSchema : Migration
    {
        public override void Up()
        {
            Create.Schema("TestSchema");

            Create.Table("Users")
                .InSchema("TestSchema")
                .WithColumn("UserId").AsInt32().Identity().PrimaryKey()
                .WithColumn("GroupId").AsInt32().NotNullable()
                .WithColumn("UserName").AsString(32).NotNullable()
                .WithColumn("Password").AsString(32).NotNullable();

            Create.Index().OnTable("Users").InSchema("TestSchema").OnColumn("GroupId").Ascending();
        }

        public override void Down()
        {
            Delete.Index("IX_Users_GroupId").OnTable("Users").InSchema("TestSchema").OnColumn("GroupId");
            Delete.Table("Users").InSchema("TestSchema");
            Delete.Schema("TestSchema");
        }
    }

    internal class TestCreateAndDropTableMigrationWithSchema : Migration
    {
        public override void Up()
        {
            Create.Table("TestTable")
                .InSchema("TestSchema")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable().WithDefaultValue("Anonymous");

            Create.Table("TestTable2")
                .InSchema("TestSchema")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).Nullable()
                .WithColumn("TestTableId").AsInt32().NotNullable();

            Create.Index("ix_Name").OnTable("TestTable2").InSchema("TestSchema").OnColumn("Name").Ascending()
                .WithOptions().NonClustered();

            Create.Column("Name2").OnTable("TestTable2").InSchema("TestSchema").AsString(10).Nullable();

            Create.ForeignKey("fk_TestTable2_TestTableId_TestTable_Id")
                .FromTable("TestTable2").InSchema("TestSchema").ForeignColumn("TestTableId")
                .ToTable("TestTable").InSchema("TestSchema").PrimaryColumn("Id");

            Insert.IntoTable("TestTable").InSchema("TestSchema").Row(new { Name = "Test" });
        }

        public override void Down()
        {
            Delete.Table("TestTable2").InSchema("TestSchema");
            Delete.Table("TestTable").InSchema("TestSchema");
        }
    }

    internal class TestRenameTableMigrationWithSchema : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Table("TestTable2").InSchema("TestSchema").To("TestTable'3");
        }
    }

    internal class TestRenameColumnMigrationWithSchema : AutoReversingMigration
    {
        public override void Up()
        {
            Rename.Column("Name").OnTable("TestTable2").InSchema("TestSchema").To("Name'3");
        }
    }

    internal class TestCreateAndDropIndexMigrationWithSchema : Migration
    {
        public override void Up()
        {
            Create.Index("IX_TestTable_Name").OnTable("TestTable").InSchema("TestSchema").OnColumn("Name");
        }

        public override void Down()
        {
            Delete.Index("IX_TestTable_Name").OnTable("TestTable").InSchema("TestSchema");
        }
    }

    internal class TestCreateSchema : Migration
    {
        public override void Up()
        {
            Create.Schema("TestSchema");
        }

        public override void Down()
        {
            Delete.Schema("TestSchema");
        }
    }

    internal class TestCreateSequence : Migration
    {
        public override void Up()
        {
            Create.Sequence("TestSequence").StartWith(1).IncrementBy(1).MinValue(0).MaxValue(1000).Cycle().Cache(10);
        }

        public override void Down()
        {
            Delete.Sequence("TestSequence");
        }
    }

    internal class TestCreateSequenceWithSchema : Migration
    {
        public override void Up()
        {
            Create.Sequence("TestSequence").InSchema("TestSchema").StartWith(1).IncrementBy(1).MinValue(0).MaxValue(1000).Cycle().Cache(10);
        }

        public override void Down()
        {
            Delete.Sequence("TestSequence").InSchema("TestSchema");
        }
    }

    internal class TestAlterColumnWithSchema: Migration
    {
        public override void Up()
        {
            Alter.Column("Name2").OnTable("TestTable2").InSchema("TestSchema").AsAnsiString(100).Nullable();
        }

        public override void Down()
        {
            Alter.Column("Name2").OnTable("TestTable2").InSchema("TestSchema").AsString(10).Nullable();
        }
    }

    internal class TestAlterTableWithSchema : Migration
    {
        public override void Up()
        {
            Alter.Table("TestTable2").InSchema("TestSchema").AddColumn("NewColumn").AsInt32().WithDefaultValue(1);
        }

        public override void Down()
        {
            Delete.Column("NewColumn").FromTable("TestTable2").InSchema("TestSchema");
        }
    }

    internal class TestAlterSchema : Migration
    {
        public override void Up()
        {
            Create.Schema("NewSchema");
            Alter.Table("TestTable").InSchema("TestSchema").ToSchema("NewSchema");
        }

        public override void Down()
        {
            Alter.Table("TestTable").InSchema("NewSchema").ToSchema("TestSchema");
            Delete.Schema("NewSchema");
        }
    }

    internal class TestCreateUniqueConstraint : Migration
    {
        public override void Up()
        {
            Create.UniqueConstraint("TestUnique").OnTable("TestTable2").Column("Name");
        }

        public override void Down()
        {
            Delete.UniqueConstraint("TestUnique").FromTable("TestTable2");
        }
    }

    internal class TestCreateUniqueConstraintWithSchema : Migration
    {
        public override void Up()
        {
            Create.UniqueConstraint("TestUnique").OnTable("TestTable2").WithSchema("TestSchema").Column("Name");
        }

        public override void Down()
        {
            Delete.UniqueConstraint("TestUnique").FromTable("TestTable2").InSchema("TestSchema");
        }
    }

    internal class TestUpdateData : Migration
    {
        public override void Up()
        {
            Update.Table("TestTable").InSchema("TestSchema").Set(new { Name = "Updated" }).AllRows();
        }

        public override void Down()
        {
            Update.Table("TestTable").InSchema("TestSchema").Set(new { Name = "Test" }).AllRows();
        }
    }

    internal class TestDeleteData : Migration
    {
        public override void Up()
        {
            Delete.FromTable("TestTable").Row(new { Name = "Test" });
        }

        public override void Down()
        {
            Insert.IntoTable("TestTable").Row(new { Name = "Test" });
        }
    }

    internal class TestDeleteDataWithSchema :Migration
    {
        public override void Up()
        {
            Delete.FromTable("TestTable").InSchema("TestSchema").Row(new { Name = "Test"});
        }

        public override void Down()
        {
            Insert.IntoTable("TestTable").InSchema("TestSchema").Row(new { Name = "Test" });
        }
    }

    internal class TestCreateIndexWithReversing : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Index().OnTable("TestTable2").InSchema("TestSchema").OnColumn("Name2").Ascending();
        }
    }

    internal class TestCreateUniqueConstraintWithReversing : AutoReversingMigration
    {
        public override void Up()
        {
            Create.UniqueConstraint("TestUnique").OnTable("TestTable2").Column("Name");
        }
    }

    internal class TestCreateUniqueConstraintWithSchemaWithReversing : AutoReversingMigration
    {
        public override void Up()
        {
            Create.UniqueConstraint("TestUnique").OnTable("TestTable2").WithSchema("TestSchema").Column("Name");
        }
    }

    internal class TestExecuteSql : Migration
    {
        public override void Up()
        {
            Execute.Sql("select 1");
        }

        public override void Down()
        {
            Execute.Sql("select 2");
        }
    }
}