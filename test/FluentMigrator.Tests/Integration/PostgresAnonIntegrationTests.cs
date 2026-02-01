#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Builder.SecurityLabel.Provider;
using FluentMigrator.Postgres;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Tests.Integration.TestCases;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration
{
    /// <summary>
    /// Integration tests for PostgreSQL Anonymizer security label functionality.
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    [Category("Postgres")]
    [Category("Anon")]
    public class PostgresAnonIntegrationTests : IntegrationTestBase
    {
        private const string RootNamespace = "FluentMigrator.Tests.Integration";

        #region Base Tests (from MigrationRunnerTests)

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplySecurityLabelWithAnonExtension(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestInitializeAnonExtension());

                    try
                    {
                        runner.Up(new TestCreateTableWithSecurityLabel());

                        var hasLabel = processor.Exists(@"
                            SELECT 1
                            FROM pg_seclabels sl
                            JOIN pg_class c ON sl.objoid = c.oid
                            JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                            WHERE c.relname = 'SecureUsers'
                              AND a.attname = 'email'
                              AND sl.label = 'MASKED WITH FUNCTION anon.fake_email()'");

                        hasLabel.ShouldBeTrue();

                        runner.Down(new TestCreateTableWithSecurityLabel());
                    }
                    finally
                    {
                        runner.Down(new TestInitializeAnonExtension());
                    }
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyMultipleSecurityLabelsWithAnonExtension(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestInitializeAnonExtension());

                    try
                    {
                        runner.Up(new TestCreateTableWithMultipleSecurityLabels());

                        var hasEmailLabel = processor.Exists(@"
                            SELECT 1
                            FROM pg_seclabels sl
                            JOIN pg_class c ON sl.objoid = c.oid
                            JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                            WHERE c.relname = 'CustomerData'
                              AND a.attname = 'email'
                              AND sl.label = 'MASKED WITH FUNCTION anon.fake_email()'");
                        hasEmailLabel.ShouldBeTrue();

                        var hasNameLabel = processor.Exists(@"
                            SELECT 1
                            FROM pg_seclabels sl
                            JOIN pg_class c ON sl.objoid = c.oid
                            JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                            WHERE c.relname = 'CustomerData'
                              AND a.attname = 'full_name'
                              AND sl.label = 'MASKED WITH VALUE ''CONFIDENTIAL'''");
                        hasNameLabel.ShouldBeTrue();

                        var hasPhoneLabel = processor.Exists(@"
                            SELECT 1
                            FROM pg_seclabels sl
                            JOIN pg_class c ON sl.objoid = c.oid
                            JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                            WHERE c.relname = 'CustomerData'
                              AND a.attname = 'phone'
                              AND sl.label = 'MASKED WITH FUNCTION anon.partial(2, ''*'', 2)'");
                        hasPhoneLabel.ShouldBeTrue();

                        runner.Down(new TestCreateTableWithMultipleSecurityLabels());
                    }
                    finally
                    {
                        runner.Down(new TestInitializeAnonExtension());
                    }
                },
                serverOptions,
                true);
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanDeleteSecurityLabelWithAnonExtension(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestInitializeAnonExtension());

                    try
                    {
                        runner.Up(new TestCreateTableWithSecurityLabel());

                        var hasLabel = processor.Exists(@"
                            SELECT 1
                            FROM pg_seclabels sl
                            JOIN pg_class c ON sl.objoid = c.oid
                            JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                            WHERE c.relname = 'SecureUsers'
                              AND a.attname = 'email'");
                        hasLabel.ShouldBeTrue();

                        runner.Up(new TestDeleteSecurityLabelFromColumn());

                        hasLabel = processor.Exists(@"
                            SELECT 1
                            FROM pg_seclabels sl
                            JOIN pg_class c ON sl.objoid = c.oid
                            JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                            WHERE c.relname = 'SecureUsers'
                              AND a.attname = 'email'");
                        hasLabel.ShouldBeFalse();

                        runner.Down(new TestCreateTableWithSecurityLabel());
                    }
                    finally
                    {
                        runner.Down(new TestInitializeAnonExtension());
                    }
                },
                serverOptions,
                true);
        }

        #endregion

        #region Fake Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyFakeFirstNameMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestFakeFunctions(),
                "TestData", "first_name", "MASKED WITH FUNCTION anon.fake_first_name()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyFakeLastNameMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestFakeFunctions(),
                "TestData", "last_name", "MASKED WITH FUNCTION anon.fake_last_name()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyFakeCompanyMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestFakeFunctions(),
                "TestData", "company", "MASKED WITH FUNCTION anon.fake_company()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyFakeCityMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestFakeFunctions(),
                "TestData", "city", "MASKED WITH FUNCTION anon.fake_city()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyFakeCountryMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestFakeFunctions(),
                "TestData", "country", "MASKED WITH FUNCTION anon.fake_country()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyFakeAddressMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestFakeFunctions(),
                "TestData", "address", "MASKED WITH FUNCTION anon.fake_address()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyFakePhoneMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestFakeFunctions(),
                "TestData", "phone", "MASKED WITH FUNCTION anon.fake_phone()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyFakeIbanMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestFakeFunctions(),
                "TestData", "iban", "MASKED WITH FUNCTION anon.fake_iban()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyFakeSiretMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestFakeFunctions(),
                "TestData", "siret", "MASKED WITH FUNCTION anon.fake_siret()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyFakeSirenMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestFakeFunctions(),
                "TestData", "siren", "MASKED WITH FUNCTION anon.fake_siren()");
        }

        #endregion

        #region Dummy Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyDummyFirstNameMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestDummyFunctions(),
                "TestData", "first_name", "MASKED WITH FUNCTION anon.dummy_first_name()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyDummyFirstNameWithLocaleMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestDummyFunctions(),
                "TestData", "first_name_fr", "MASKED WITH FUNCTION anon.dummy_first_name(fr_FR)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyDummyLastNameMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestDummyFunctions(),
                "TestData", "last_name", "MASKED WITH FUNCTION anon.dummy_last_name()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyDummyEmailMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestDummyFunctions(),
                "TestData", "email", "MASKED WITH FUNCTION anon.dummy_email()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyDummyCompanyMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestDummyFunctions(),
                "TestData", "company", "MASKED WITH FUNCTION anon.dummy_company()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyDummyPhoneMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestDummyFunctions(),
                "TestData", "phone", "MASKED WITH FUNCTION anon.dummy_phone()");
        }

        #endregion

        #region Pseudo Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyPseudoFirstNameMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestPseudoFunctions(),
                "TestData", "first_name", "MASKED WITH FUNCTION anon.pseudo_first_name(id)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyPseudoLastNameMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestPseudoFunctions(),
                "TestData", "last_name", "MASKED WITH FUNCTION anon.pseudo_last_name(id)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyPseudoEmailMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestPseudoFunctions(),
                "TestData", "email", "MASKED WITH FUNCTION anon.pseudo_email(id)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyPseudoCompanyMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestPseudoFunctions(),
                "TestData", "company", "MASKED WITH FUNCTION anon.pseudo_company(id)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyPseudoPhoneMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestPseudoFunctions(),
                "TestData", "phone", "MASKED WITH FUNCTION anon.pseudo_phone(id)");
        }

        #endregion

        #region Hashing Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyHashMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestHashingFunctions(),
                "TestData", "email", "MASKED WITH FUNCTION anon.hash()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyHashWithAlgorithmMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestHashingFunctions(),
                "TestData", "password", "MASKED WITH FUNCTION anon.hash(sha256)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyHmacHashMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestHashingFunctions(),
                "TestData", "token", "MASKED WITH FUNCTION anon.hmac_hash()");
        }

        #endregion

        #region Noise Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyAddNoiseToIntMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestNoiseFunctions(),
                "TestData", "age", "MASKED WITH FUNCTION anon.add_noise_to_int(0.5)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyAddNoiseToNumericMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestNoiseFunctions(),
                "TestData", "salary", "MASKED WITH FUNCTION anon.add_noise_to_numeric(0.3)");
        }

        #endregion

        #region Generalize Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyGeneralizeIbanMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestGeneralizeFunctions(),
                "TestData", "iban", "MASKED WITH FUNCTION anon.generalize_iban()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyGeneralizePhoneNumberMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestGeneralizeFunctions(),
                "TestData", "phone", "MASKED WITH FUNCTION anon.generalize_phone_number()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyGeneralizeEmailMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestGeneralizeFunctions(),
                "TestData", "email", "MASKED WITH FUNCTION anon.generalize_email()");
        }

        #endregion

        #region Random Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyRandomStringMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestRandomFunctions(),
                "TestData", "code", "MASKED WITH FUNCTION anon.random_string(12)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyRandomIntMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestRandomFunctions(),
                "TestData", "number", "MASKED WITH FUNCTION anon.random_int()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyRandomIntBetweenMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestRandomFunctions(),
                "TestData", "age", "MASKED WITH FUNCTION anon.random_int_between(18, 99)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyRandomDateMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestRandomFunctions(),
                "TestData", "birth_date", "MASKED WITH FUNCTION anon.random_date()");
        }

        #endregion

        #region Partial Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyPartialScrambling(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestPartialFunctions(),
                "TestData", "credit_card", "MASKED WITH FUNCTION anon.partial(2, '*', 2)");
        }

        #endregion

        #region Helper Methods

        private void ExecuteAnonTest(
            Type processorType,
            Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions,
            Migration migration,
            string tableName,
            string columnName,
            string expectedLabel)
        {
            ExecuteWithProcessor(
                processorType,
                services => services.WithMigrationsIn(RootNamespace),
                (serviceProvider, processor) =>
                {
                    var runner = (MigrationRunner)serviceProvider.GetRequiredService<IMigrationRunner>();

                    runner.Up(new TestInitializeAnonExtension());

                    try
                    {
                        runner.Up(migration);

                        var hasLabel = processor.Exists($@"
                            SELECT 1
                            FROM pg_seclabels sl
                            JOIN pg_class c ON sl.objoid = c.oid
                            JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                            WHERE c.relname = '{tableName}'
                              AND a.attname = '{columnName}'
                              AND sl.label = '{expectedLabel.Replace("'", "''")}'");

                        hasLabel.ShouldBeTrue($"Expected security label '{expectedLabel}' on column '{tableName}.{columnName}'");

                        runner.Down(migration);
                    }
                    finally
                    {
                        runner.Down(new TestInitializeAnonExtension());
                    }
                },
                serverOptions,
                true);
        }

        #endregion
    }

    #region Test Migrations (from MigrationRunnerTests)

    internal class TestInitializeAnonExtension : Migration
    {
        public override void Up()
        {
            Execute.Sql("CREATE EXTENSION IF NOT EXISTS anon CASCADE;");
            Execute.Sql("SELECT anon.init();");
        }

        public override void Down()
        {
            Execute.Sql("DROP EXTENSION IF EXISTS anon CASCADE;");
        }
    }

    internal class TestCreateTableWithSecurityLabel : Migration
    {
        public override void Up()
        {
            Create.Table("SecureUsers")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("email").AsString(255).NotNullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("email")
                .OnTable("SecureUsers")
                .WithLabel(label => label.MaskedWithFakeEmail());
        }

        public override void Down()
        {
            Delete.Table("SecureUsers");
        }
    }

    internal class TestCreateTableWithMultipleSecurityLabels : Migration
    {
        public override void Up()
        {
            Create.Table("CustomerData")
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("email").AsString(255).NotNullable()
                .WithColumn("full_name").AsString(255).NotNullable()
                .WithColumn("phone").AsString(50).Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("email")
                .OnTable("CustomerData")
                .WithLabel(label => label.MaskedWithFakeEmail());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("full_name")
                .OnTable("CustomerData")
                .WithLabel(label => label.MaskedWithValue("CONFIDENTIAL"));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("phone")
                .OnTable("CustomerData")
                .WithLabel(label => label.MaskedWithPartialScrambling(2, '*', 2));
        }

        public override void Down()
        {
            Delete.Table("CustomerData");
        }
    }

    internal class TestDeleteSecurityLabelFromColumn : ForwardOnlyMigration
    {
        public override void Up()
        {
            Delete.SecurityLabel<AnonSecurityLabelBuilder>()
                .FromColumn("email")
                .OnTable("SecureUsers");
        }
    }

    #endregion

    #region Test Migrations for All Anon Functions

    internal class TestFakeFunctions : Migration
    {
        public override void Up()
        {
            Create.Table("TestData")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("first_name").AsString(255).Nullable()
                .WithColumn("last_name").AsString(255).Nullable()
                .WithColumn("email").AsString(255).Nullable()
                .WithColumn("company").AsString(255).Nullable()
                .WithColumn("city").AsString(255).Nullable()
                .WithColumn("country").AsString(255).Nullable()
                .WithColumn("address").AsString(500).Nullable()
                .WithColumn("phone").AsString(50).Nullable()
                .WithColumn("iban").AsString(50).Nullable()
                .WithColumn("siret").AsString(50).Nullable()
                .WithColumn("siren").AsString(50).Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("first_name").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeFirstName());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("last_name").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeLastName());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("email").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeEmail());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("company").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeCompany());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("city").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeCity());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("country").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeCountry());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("address").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeAddress());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("phone").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakePhone());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("iban").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeIban());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("siret").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeSiret());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("siren").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeSiren());
        }

        public override void Down()
        {
            Delete.Table("TestData");
        }
    }

    internal class TestDummyFunctions : Migration
    {
        public override void Up()
        {
            Create.Table("TestData")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("first_name").AsString(255).Nullable()
                .WithColumn("first_name_fr").AsString(255).Nullable()
                .WithColumn("last_name").AsString(255).Nullable()
                .WithColumn("email").AsString(255).Nullable()
                .WithColumn("company").AsString(255).Nullable()
                .WithColumn("phone").AsString(50).Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("first_name").OnTable("TestData")
                .WithLabel(label => label.MaskedWithDummyFirstName());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("first_name_fr").OnTable("TestData")
                .WithLabel(label => label.MaskedWithDummyFirstName("fr_FR"));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("last_name").OnTable("TestData")
                .WithLabel(label => label.MaskedWithDummyLastName());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("email").OnTable("TestData")
                .WithLabel(label => label.MaskedWithDummyEmail());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("company").OnTable("TestData")
                .WithLabel(label => label.MaskedWithDummyCompany());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("phone").OnTable("TestData")
                .WithLabel(label => label.MaskedWithDummyPhone());
        }

        public override void Down()
        {
            Delete.Table("TestData");
        }
    }

    internal class TestPseudoFunctions : Migration
    {
        public override void Up()
        {
            Create.Table("TestData")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("first_name").AsString(255).Nullable()
                .WithColumn("last_name").AsString(255).Nullable()
                .WithColumn("email").AsString(255).Nullable()
                .WithColumn("company").AsString(255).Nullable()
                .WithColumn("phone").AsString(50).Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("first_name").OnTable("TestData")
                .WithLabel(label => label.MaskedWithPseudoFirstName("id"));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("last_name").OnTable("TestData")
                .WithLabel(label => label.MaskedWithPseudoLastName("id"));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("email").OnTable("TestData")
                .WithLabel(label => label.MaskedWithPseudoEmail("id"));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("company").OnTable("TestData")
                .WithLabel(label => label.MaskedWithPseudoCompany("id"));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("phone").OnTable("TestData")
                .WithLabel(label => label.MaskedWithPseudoPhone("id"));
        }

        public override void Down()
        {
            Delete.Table("TestData");
        }
    }

    internal class TestHashingFunctions : Migration
    {
        public override void Up()
        {
            Create.Table("TestData")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("email").AsString(255).Nullable()
                .WithColumn("password").AsString(255).Nullable()
                .WithColumn("token").AsString(255).Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("email").OnTable("TestData")
                .WithLabel(label => label.MaskedWithHash());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("password").OnTable("TestData")
                .WithLabel(label => label.MaskedWithHash(AnonHashAlgorithm.Sha256));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("token").OnTable("TestData")
                .WithLabel(label => label.MaskedWithHmacHash());
        }

        public override void Down()
        {
            Delete.Table("TestData");
        }
    }

    internal class TestNoiseFunctions : Migration
    {
        public override void Up()
        {
            Create.Table("TestData")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("age").AsInt32().Nullable()
                .WithColumn("salary").AsDecimal().Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("age").OnTable("TestData")
                .WithLabel(label => label.MaskedWithAddNoiseToInt(0.5));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("salary").OnTable("TestData")
                .WithLabel(label => label.MaskedWithAddNoiseToNumeric(0.3));
        }

        public override void Down()
        {
            Delete.Table("TestData");
        }
    }

    internal class TestGeneralizeFunctions : Migration
    {
        public override void Up()
        {
            Create.Table("TestData")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("iban").AsString(50).Nullable()
                .WithColumn("phone").AsString(50).Nullable()
                .WithColumn("email").AsString(255).Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("iban").OnTable("TestData")
                .WithLabel(label => label.MaskedWithGeneralizeIban());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("phone").OnTable("TestData")
                .WithLabel(label => label.MaskedWithGeneralizePhoneNumber());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("email").OnTable("TestData")
                .WithLabel(label => label.MaskedWithGeneralizeEmail());
        }

        public override void Down()
        {
            Delete.Table("TestData");
        }
    }

    internal class TestRandomFunctions : Migration
    {
        public override void Up()
        {
            Create.Table("TestData")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("code").AsString(50).Nullable()
                .WithColumn("number").AsInt32().Nullable()
                .WithColumn("age").AsInt32().Nullable()
                .WithColumn("birth_date").AsDate().Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("code").OnTable("TestData")
                .WithLabel(label => label.MaskedWithRandomString(12));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("number").OnTable("TestData")
                .WithLabel(label => label.MaskedWithRandomInt());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("age").OnTable("TestData")
                .WithLabel(label => label.MaskedWithRandomIntBetween(18, 99));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("birth_date").OnTable("TestData")
                .WithLabel(label => label.MaskedWithRandomDate());
        }

        public override void Down()
        {
            Delete.Table("TestData");
        }
    }

    internal class TestPartialFunctions : Migration
    {
        public override void Up()
        {
            Create.Table("TestData")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("credit_card").AsString(50).Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("credit_card").OnTable("TestData")
                .WithLabel(label => label.MaskedWithPartialScrambling(2, '*', 2));
        }

        public override void Down()
        {
            Delete.Table("TestData");
        }
    }

    #endregion
}



