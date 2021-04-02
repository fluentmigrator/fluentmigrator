// #region License
// //
// // Copyright (c) 2018, Fluent Migrator Project
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //   http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// #endregion
//
// using FluentMigrator.Runner;
// using FluentMigrator.Runner.Initialization;
// using FluentMigrator.Runner.Processors.Hana;
// using FluentMigrator.Tests.Helpers;
//
// using Microsoft.Extensions.DependencyInjection;
//
// using NUnit.Framework;
//
// using Shouldly;
//
// namespace FluentMigrator.Tests.Integration.Processors.Hana
// {
//     [TestFixture]
//     [Category("Integration")]
//     [Category("Hana")]
//     public class HanaSequenceTests : BaseSequenceTests
//     {
//         private ServiceProvider ServiceProvider { get; set; }
//         private IServiceScope ServiceScope { get; set; }
//         private HanaProcessor Processor { get; set; }
//
//         [Test]
//         public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExist()
//         {
//             Processor.SequenceExists(null, "DoesNotExist").ShouldBeFalse();
//         }
//
//         [Test]
//         public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExistWithSchema()
//         {
//             Processor.SequenceExists("test_schema", "DoesNotExist").ShouldBeFalse();
//         }
//
//         [Test]
//         public override void CallingSequenceExistsReturnsTrueIfSequenceExists()
//         {
//             using (new HanaTestSequence(Processor, null, "test_sequence"))
//                 Processor.SequenceExists(null, "test_sequence").ShouldBeTrue();
//         }
//
//         [Test]
//         public override void CallingSequenceExistsReturnsTrueIfSequenceExistsWithSchema()
//         {
//             Assert.Ignore("Schemas aren't supported by this SAP Hana runner");
//         }
//
//         [OneTimeSetUp]
//         public void ClassSetUp()
//         {
//             if (!IntegrationTestOptions.Hana.IsEnabled)
//                 Assert.Ignore();
//
//             var serivces = ServiceCollectionExtensions.CreateServices()
//                 .ConfigureRunner(builder => builder.AddHana())
//                 .AddScoped<IConnectionStringReader>(
//                     _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Hana.ConnectionString));
//             ServiceProvider = serivces.BuildServiceProvider();
//         }
//
//         [OneTimeTearDown]
//         public void ClassTearDown()
//         {
//             ServiceProvider?.Dispose();
//         }
//
//         [SetUp]
//         public void SetUp()
//         {
//             ServiceScope = ServiceProvider.CreateScope();
//             Processor = ServiceScope.ServiceProvider.GetRequiredService<HanaProcessor>();
//         }
//
//         [TearDown]
//         public void TearDown()
//         {
//             ServiceScope?.Dispose();
//         }
//     }
// }
// #region License
// //
// // Copyright (c) 2018, Fluent Migrator Project
// //
// // Licensed under the Apache License, Version 2.0 (the "License");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //   http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// #endregion
//
// using FluentMigrator.Runner;
// using FluentMigrator.Runner.Initialization;
// using FluentMigrator.Runner.Processors.Hana;
// using FluentMigrator.Tests.Helpers;
//
// using Microsoft.Extensions.DependencyInjection;
//
// using NUnit.Framework;
//
// using Shouldly;
//
// namespace FluentMigrator.Tests.Integration.Processors.Hana
// {
//     [TestFixture]
//     [Category("Integration")]
//     [Category("Hana")]
//     public class HanaSequenceTests : BaseSequenceTests
//     {
//         private ServiceProvider ServiceProvider { get; set; }
//         private IServiceScope ServiceScope { get; set; }
//         private HanaProcessor Processor { get; set; }
//
//         [Test]
//         public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExist()
//         {
//             Processor.SequenceExists(null, "DoesNotExist").ShouldBeFalse();
//         }
//
//         [Test]
//         public override void CallingSequenceExistsReturnsFalseIfSequenceDoesNotExistWithSchema()
//         {
//             Processor.SequenceExists("test_schema", "DoesNotExist").ShouldBeFalse();
//         }
//
//         [Test]
//         public override void CallingSequenceExistsReturnsTrueIfSequenceExists()
//         {
//             using (new HanaTestSequence(Processor, null, "test_sequence"))
//                 Processor.SequenceExists(null, "test_sequence").ShouldBeTrue();
//         }
//
//         [Test]
//         public override void CallingSequenceExistsReturnsTrueIfSequenceExistsWithSchema()
//         {
//             Assert.Ignore("Schemas aren't supported by this SAP Hana runner");
//         }
//
//         [OneTimeSetUp]
//         public void ClassSetUp()
//         {
//             if (!IntegrationTestOptions.Hana.IsEnabled)
//                 Assert.Ignore();
//
//             var serivces = ServiceCollectionExtensions.CreateServices()
//                 .ConfigureRunner(builder => builder.AddHana())
//                 .AddScoped<IConnectionStringReader>(
//                     _ => new PassThroughConnectionStringReader(IntegrationTestOptions.Hana.ConnectionString));
//             ServiceProvider = serivces.BuildServiceProvider();
//         }
//
//         [OneTimeTearDown]
//         public void ClassTearDown()
//         {
//             ServiceProvider?.Dispose();
//         }
//
//         [SetUp]
//         public void SetUp()
//         {
//             ServiceScope = ServiceProvider.CreateScope();
//             Processor = ServiceScope.ServiceProvider.GetRequiredService<HanaProcessor>();
//         }
//
//         [TearDown]
//         public void TearDown()
//         {
//             ServiceScope?.Dispose();
//         }
//     }
// }
