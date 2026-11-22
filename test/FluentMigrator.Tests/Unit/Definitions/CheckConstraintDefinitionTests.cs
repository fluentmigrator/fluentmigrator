#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Definitions;

[TestFixture]
public class CheckConstraintDefinitionTests
{
    [Test]
    public void CloneCopiesNamingColumnsInOrderWithoutSharingThem()
    {
        var definition = new CheckConstraintDefinition { Columns = { "StartDate", "EndDate" } };

        var clone = definition.Clone();
        definition.Columns.Clear();

        Assert.That(clone.Columns, Is.EqualTo(new[] { "StartDate", "EndDate" }));
    }

    [Test]
    public void CloneCopiesEachSettingToAnIndependentDefinition()
    {
        var definition = new CheckConstraintDefinition
        {
            Name = "ck_quantity",
            SchemaName = "stock",
            TableName = "items",
            Condition = "quantity > 0",
            NoCheck = true,
            Enabled = false
        };

        var clone = definition.Clone();

        Assert.That(clone, Is.Not.SameAs(definition));
        Assert.That(clone, Is.EqualTo(definition).UsingPropertiesComparer());
    }

    [Test]
    public void CloneCopiesAdditionalFeaturesWithoutSharingTheDictionaryOrCloneableValues()
    {
        const string feature = "Example:Options";
        var definition = new CheckConstraintDefinition();
        var featureValue = new[] { "first" };
        var features = definition.ShouldBeAssignableTo<ISupportAdditionalFeatures>();
        features.SetAdditionalFeature(feature, featureValue);

        var cloneFeatures = definition.Clone().ShouldBeAssignableTo<ISupportAdditionalFeatures>();

        Assert.Multiple(() =>
        {
            cloneFeatures.AdditionalFeatures.ShouldNotBeSameAs(features.AdditionalFeatures);
            var cloneValue = cloneFeatures.GetAdditionalFeature<string[]>(feature);
            cloneValue.ShouldBe(featureValue);
            cloneValue.ShouldNotBeSameAs(featureValue);
        });
    }
}
