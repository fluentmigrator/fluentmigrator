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
using System.Data;

using FluentMigrator.Builder.SecurityLabel.Provider;
using FluentMigrator.Postgres;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
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
                        // Enable debug logging for better error diagnostics
                        EnableDebugLogging(processor);

                        runner.Up(new TestCreateTableWithSecurityLabel());

                        // Validate using both pg_seclabels and anon.pg_masking_rules
                        ValidateMaskingRuleExists(processor, "SecureUsers", "email", "MASKED WITH FUNCTION anon.fake_email()");
                        ValidateSecurityLabelExists(processor, "SecureUsers", "email", "MASKED WITH FUNCTION anon.fake_email()");

                        // Validate that the anonymization actually works
                        ValidateAnonymizationWorks(processor);

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
                        // Enable debug logging for better error diagnostics
                        EnableDebugLogging(processor);

                        runner.Up(new TestCreateTableWithMultipleSecurityLabels());

                        // Validate email masking rule using both methods
                        ValidateMaskingRuleExists(processor, "CustomerData", "email", "MASKED WITH FUNCTION anon.fake_email()");
                        ValidateSecurityLabelExists(processor, "CustomerData", "email", "MASKED WITH FUNCTION anon.fake_email()");

                        // // Validate full_name masking rule using both methods
                        // TODO FIXME
                        // ValidateMaskingRuleExists(processor, "CustomerData", "full_name", "MASKED WITH VALUE 'CONFIDENTIAL'");
                        // ValidateSecurityLabelExists(processor, "CustomerData", "full_name", "MASKED WITH VALUE 'CONFIDENTIAL'");

                        // Validate phone masking rule using both methods
                        ValidateMaskingRuleExists(processor, "CustomerData", "phone", "MASKED WITH FUNCTION anon.partial(phone, 2, '*', 2)");
                        ValidateSecurityLabelExists(processor, "CustomerData", "phone", "MASKED WITH FUNCTION anon.partial(phone, 2, '*', 2)");

                        // Validate that the anonymization actually works
                        ValidateAnonymizationWorks(processor);

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
                        // Enable debug logging for better error diagnostics
                        EnableDebugLogging(processor);

                        runner.Up(new TestCreateTableWithSecurityLabel());

                        // Verify masking rule exists before deletion
                        var hasLabel = processor.Exists(@"
                            SELECT 1
                            FROM pg_seclabels sl
                            JOIN pg_class c ON sl.objoid = c.oid
                            JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                            WHERE c.relname = 'SecureUsers'
                              AND a.attname = 'email'");
                        hasLabel.ShouldBeTrue();

                        var hasMaskingRule = processor.Exists(@"
                            SELECT 1
                            FROM anon.pg_masking_rules
                            WHERE relname = 'SecureUsers'
                              AND attname = 'email'");
                        hasMaskingRule.ShouldBeTrue();

                        runner.Up(new TestDeleteSecurityLabelFromColumn());

                        // Verify masking rule is removed from pg_seclabels
                        hasLabel = processor.Exists(@"
                            SELECT 1
                            FROM pg_seclabels sl
                            JOIN pg_class c ON sl.objoid = c.oid
                            JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                            WHERE c.relname = 'SecureUsers'
                              AND a.attname = 'email'");
                        hasLabel.ShouldBeFalse();

                        // Verify masking rule is removed from anon.pg_masking_rules
                        hasMaskingRule = processor.Exists(@"
                            SELECT 1
                            FROM anon.pg_masking_rules
                            WHERE relname = 'SecureUsers'
                              AND attname = 'email'");
                        hasMaskingRule.ShouldBeFalse();

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
                "TestData", "first_name_fr", "MASKED WITH FUNCTION anon.dummy_first_name_locale('fr_FR')");
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
        public void CanApplyDummyFreeEmailMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestDummyFunctions(),
                "TestData", "email", "MASKED WITH FUNCTION anon.dummy_free_email()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyDummyCompanyMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestDummyFunctions(),
                "TestData", "company", "MASKED WITH FUNCTION anon.dummy_company_name()");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyDummyPhoneMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestDummyFunctions(),
                "TestData", "phone", "MASKED WITH FUNCTION anon.dummy_phone_number()");
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

        #endregion

        #region Hashing Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyHashMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestHashingFunctions(),
                "TestData", "email", "MASKED WITH FUNCTION anon.hash(email)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyHashWithAlgorithmMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestHashingFunctions(),
                "TestData", "password", "MASKED WITH FUNCTION anon.digest(password, 'foo', 'sha256')");
        }

        #endregion

        #region Noise Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyAddNoiseToIntMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestNoiseFunctions(),
                "TestData", "age", "MASKED WITH FUNCTION anon.noise(age, 0.5)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyAddNoiseToDateMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestNoiseFunctions(),
                "TestData", "custom_date", "MASKED WITH FUNCTION anon.dnoise(custom_date, '6 months')");
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
        public void CanApplyRandomIntBetweenMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestRandomFunctions(),
                "TestData", "age", "MASKED WITH FUNCTION anon.random_int_between(18, 99)");
        }

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyRandomDateBetweenMasking(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestRandomFunctions(),
                "TestData", "birth_date", "MASKED WITH FUNCTION anon.random_date_between('1950-01-01', '2005-12-31')");
        }

        #endregion

        #region Partial Functions Tests

        [Test]
        [TestCaseSource(typeof(ProcessorTestCaseSourceOnly<PostgresProcessor>))]
        public void CanApplyPartialScrambling(Type processorType, Func<IntegrationTestOptions.DatabaseServerOptions> serverOptions)
        {
            ExecuteAnonTest(processorType, serverOptions,
                new TestPartialFunctions(),
                "TestData", "credit_card", "MASKED WITH FUNCTION anon.partial(credit_card, 2, '*', 2)");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Executes an Anon test with enhanced validation using anon.pg_masking_rules
        /// and debug logging for better error diagnostics.
        /// </summary>
        /// <param name="processorType">The processor type.</param>
        /// <param name="serverOptions">The server options.</param>
        /// <param name="migration">The migration to test.</param>
        /// <param name="tableName">The table name to check.</param>
        /// <param name="columnName">The column name to check.</param>
        /// <param name="expectedLabel">The expected masking label.</param>
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
                        // Enable debug logging for better error diagnostics
                        EnableDebugLogging(processor);

                        runner.Up(migration);

                        // Validate using both pg_seclabels and anon.pg_masking_rules
                        ValidateMaskingRuleExists(processor, tableName, columnName, expectedLabel);
                        ValidateSecurityLabelExists(processor, tableName, columnName, expectedLabel);

                        // Validate that the anonymization actually works by calling anonymize_database()
                        // This is the ultimate test - if the rule is invalid, this will throw an error
                        ValidateAnonymizationWorks(processor);

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

        /// <summary>
        /// Enables debug logging for better error diagnostics when running anon operations.
        /// Sets client_min_messages=DEBUG to get detailed error information.
        /// </summary>
        /// <param name="processor">The processor to execute the command on.</param>
        private static void EnableDebugLogging(ProcessorBase processor)
        {
            processor.Execute("SET client_min_messages=DEBUG;");
        }

        /// <summary>
        /// Validates that a masking rule exists in anon.pg_masking_rules.
        /// This is the recommended way to check masking rules according to PostgreSQL Anonymizer documentation.
        /// </summary>
        /// <param name="processor">The processor to query.</param>
        /// <param name="tableName">The table name to check.</param>
        /// <param name="columnName">The column name to check.</param>
        /// <param name="expectedLabel">The expected masking label.</param>
        private static void ValidateMaskingRuleExists(
            ProcessorBase processor,
            string tableName,
            string columnName,
            string expectedLabel)
        {
            // Remove the "MASKED WITH FUNCTION " and "MASKED WITH VALUE " prefix for comparison
            expectedLabel = expectedLabel
                .Replace("MASKED WITH FUNCTION ", string.Empty)
                .Replace("MASKED WITH VALUE ", string.Empty);

            DataSet allRules = processor.Read($@"
                SELECT masking_function
                FROM anon.pg_masking_rules
                WHERE relname = '{tableName}'
                  AND attname = '{columnName}';");

            var results = allRules.Tables[0].Rows;
            var dbRule = results[0].ItemArray[0].ToString();

            dbRule.ShouldBe(
                expectedLabel,
                $"Expected masking rule '{expectedLabel}' on column '{tableName}.{columnName}' in anon.pg_masking_rules");
        }

        /// <summary>
        /// Validates that a security label exists in pg_seclabels.
        /// This is the traditional way to check security labels.
        /// </summary>
        /// <param name="processor">The processor to query.</param>
        /// <param name="tableName">The table name to check.</param>
        /// <param name="columnName">The column name to check.</param>
        /// <param name="expectedLabel">The expected security label.</param>
        private static void ValidateSecurityLabelExists(
            ProcessorBase processor,
            string tableName,
            string columnName,
            string expectedLabel)
        {
            var hasLabel = processor.Exists($@"
                SELECT 1
                FROM pg_seclabels sl
                JOIN pg_class c ON sl.objoid = c.oid
                JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = sl.objsubid
                WHERE c.relname = '{tableName}'
                  AND a.attname = '{columnName}'
                  AND sl.label = '{expectedLabel.Replace("'", "''")}'");

            hasLabel.ShouldBeTrue($"Expected security label '{expectedLabel}' on column '{tableName}.{columnName}' in pg_seclabels");
        }

        /// <summary>
        /// Validates that all masking rules are valid by calling anon.anonymize_database().
        /// This is the ultimate test to ensure that the masking rules we created are actually functional.
        /// If any rule is invalid (e.g., trying to mask a NOT NULL column with NULL), this will throw an error.
        /// </summary>
        /// <param name="processor">The processor to execute the anonymization.</param>
        /// <remarks>
        /// According to PostgreSQL Anonymizer documentation, calling anonymize_database() will:
        /// - Process all masking rules in the database
        /// - Throw errors for invalid rules (e.g., masking NOT NULL columns with NULL)
        /// - With client_min_messages=DEBUG, show detailed information about each rule being processed
        /// </remarks>
        private static void ValidateAnonymizationWorks(ProcessorBase processor)
        {
            processor.Execute("SELECT anon.anonymize_database();");
        }

        /// <summary>
        /// Dumps all masking rules for debugging purposes.
        /// Useful for troubleshooting when tests fail.
        /// Call this method when debugging test failures to see all configured masking rules.
        /// </summary>
        /// <param name="processor">The processor to query.</param>
        /// <remarks>
        /// The query returns: attrelid, relname, attname, format_type, masking_function, priority.
        /// This method is intentionally kept for debugging purposes even if not called directly in tests.
        /// To use: add a call to DumpMaskingRulesForDebugging(processor) in a failing test.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Kept for debugging purposes")]
        private static void DumpMaskingRulesForDebugging(ProcessorBase processor)
        {
            // Execute the query to list all masking rules
            // This can be called when debugging failing tests
            processor.Execute("SELECT relname, attname, masking_function FROM anon.pg_masking_rules;");
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
                .WithLabel(label => label.MaskedWithPartialScrambling("phone", 2, '*', 2));
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
                .WithColumn("siret").AsString(50).Nullable();

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
                .OnColumn("iban").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeIban());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("siret").OnTable("TestData")
                .WithLabel(label => label.MaskedWithFakeSiret());
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
                .WithLabel(label => label.MaskedWithDummyFreeEmail());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("company").OnTable("TestData")
                .WithLabel(label => label.MaskedWithDummyCompanyName());

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("phone").OnTable("TestData")
                .WithLabel(label => label.MaskedWithDummyPhoneNumber());
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
                .WithColumn("company").AsString(255).Nullable();

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
                .WithColumn("password").AsString(255).Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("email").OnTable("TestData")
                .WithLabel(label => label.MaskedWithHash("email"));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("password").OnTable("TestData")
                .WithLabel(label => label.MaskedWithDigest("password", "'foo'", AnonHashAlgorithm.Sha256));
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
                .WithColumn("custom_date").AsDateTime().Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("age").OnTable("TestData")
                .WithLabel(label => label.MaskedWithNoise("age", 0.5));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("custom_date").OnTable("TestData")
                .WithLabel(label => label.MaskedWithDateNoise("custom_date", "6 months"));
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
                .WithColumn("age").AsInt32().Nullable()
                .WithColumn("birth_date").AsDate().Nullable();

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("code").OnTable("TestData")
                .WithLabel(label => label.MaskedWithRandomString(12));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("age").OnTable("TestData")
                .WithLabel(label => label.MaskedWithRandomIntBetween(18, 99));

            Create.SecurityLabel<AnonSecurityLabelBuilder>()
                .OnColumn("birth_date").OnTable("TestData")
                .WithLabel(label => label.MaskedWithRandomDateBetween("'1950-01-01'", "'2005-12-31'"));
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
                .WithLabel(label => label.MaskedWithPartialScrambling("credit_card", 2, '*', 2));
        }

        public override void Down()
        {
            Delete.Table("TestData");
        }
    }

    #endregion
}



