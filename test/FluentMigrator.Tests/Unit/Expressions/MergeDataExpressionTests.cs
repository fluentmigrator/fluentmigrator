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

using System.Collections.Generic;
using System.Data;

using FluentMigrator.Expressions;
using FluentMigrator.Model;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Expressions
{
    [TestFixture]
    [Category("Expression")]
    [Category("MergeData")]
    public class MergeDataExpressionTests
    {
        [Test]
        public void ExecuteWithCallsCorrectMethods()
        {
            var expression = new MergeDataExpression
            {
                TableName = "TestTable",
                SchemaName = "TestSchema"
            };

            expression.MatchColumns.Add("Id");

            var row1 = new InsertionDataDefinition();
            row1.Add(new KeyValuePair<string, object>("Id", 1));
            row1.Add(new KeyValuePair<string, object>("Name", "John"));
            expression.Rows.Add(row1);

            var existingDataSet = new DataSet();
            var existingTable = new DataTable();
            existingTable.Columns.Add("Id", typeof(int));
            existingTable.Columns.Add("Name", typeof(string));
            
            // Add existing row with Id = 1 (should be updated)
            var existingRow = existingTable.NewRow();
            existingRow["Id"] = 1;
            existingRow["Name"] = "OldJohn";
            existingTable.Rows.Add(existingRow);
            
            existingDataSet.Tables.Add(existingTable);

            var processorMock = new Mock<IMigrationProcessor>();
            processorMock.Setup(x => x.ReadTableData("TestSchema", "TestTable"))
                        .Returns(existingDataSet);

            expression.ExecuteWith(processorMock.Object);

            // Should call Process with UpdateDataExpression (not InsertDataExpression)
            processorMock.Verify(x => x.Process(It.IsAny<UpdateDataExpression>()), Times.Once);
            processorMock.Verify(x => x.Process(It.IsAny<InsertDataExpression>()), Times.Never);
        }

        [Test]
        public void ExecuteWithInsertsNewRow()
        {
            var expression = new MergeDataExpression
            {
                TableName = "TestTable",
                SchemaName = "TestSchema"
            };

            expression.MatchColumns.Add("Id");

            var row1 = new InsertionDataDefinition();
            row1.Add(new KeyValuePair<string, object>("Id", 2));
            row1.Add(new KeyValuePair<string, object>("Name", "Jane"));
            expression.Rows.Add(row1);

            var existingDataSet = new DataSet();
            var existingTable = new DataTable();
            existingTable.Columns.Add("Id", typeof(int));
            existingTable.Columns.Add("Name", typeof(string));
            
            // Add existing row with Id = 1 (different from new row Id = 2)
            var existingRow = existingTable.NewRow();
            existingRow["Id"] = 1;
            existingRow["Name"] = "John";
            existingTable.Rows.Add(existingRow);
            
            existingDataSet.Tables.Add(existingTable);

            var processorMock = new Mock<IMigrationProcessor>();
            processorMock.Setup(x => x.ReadTableData("TestSchema", "TestTable"))
                        .Returns(existingDataSet);

            expression.ExecuteWith(processorMock.Object);

            // Should call Process with InsertDataExpression (not UpdateDataExpression)
            processorMock.Verify(x => x.Process(It.IsAny<InsertDataExpression>()), Times.Once);
            processorMock.Verify(x => x.Process(It.IsAny<UpdateDataExpression>()), Times.Never);
        }

        [Test]
        public void SchemaNamePropertyWorks()
        {
            var expression = new MergeDataExpression();
            expression.SchemaName = "TestSchema";
            expression.SchemaName.ShouldBe("TestSchema");
        }

        [Test]
        public void TableNamePropertyWorks()
        {
            var expression = new MergeDataExpression();
            expression.TableName = "TestTable";
            expression.TableName.ShouldBe("TestTable");
        }

        [Test]
        public void AdditionalFeaturesPropertyIsInitialized()
        {
            var expression = new MergeDataExpression();
            expression.AdditionalFeatures.ShouldNotBeNull();
        }

        [Test]
        public void RowsPropertyIsInitialized()
        {
            var expression = new MergeDataExpression();
            expression.Rows.ShouldNotBeNull();
            expression.Rows.Count.ShouldBe(0);
        }

        [Test]
        public void MatchColumnsPropertyIsInitialized()
        {
            var expression = new MergeDataExpression();
            expression.MatchColumns.ShouldNotBeNull();
            expression.MatchColumns.Count.ShouldBe(0);
        }
    }
}