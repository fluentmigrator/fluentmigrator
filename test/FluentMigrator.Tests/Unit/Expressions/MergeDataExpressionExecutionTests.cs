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
    public class MergeDataExpressionExecutionTests
    {
        [Test]
        public void ExecuteWithPerformsBothInsertAndUpdate()
        {
            var expression = new MergeDataExpression
            {
                TableName = "TestTable",
                SchemaName = "TestSchema"
            };

            expression.MatchColumns.Add("Id");

            // Add two rows: one that exists (Id=1) and one that doesn't (Id=3)
            var existingRow = new InsertionDataDefinition();
            existingRow.Add(new KeyValuePair<string, object>("Id", 1));
            existingRow.Add(new KeyValuePair<string, object>("Name", "Updated John"));
            expression.Rows.Add(existingRow);

            var newRow = new InsertionDataDefinition();
            newRow.Add(new KeyValuePair<string, object>("Id", 3));
            newRow.Add(new KeyValuePair<string, object>("Name", "New Jane"));
            expression.Rows.Add(newRow);

            // Setup existing data
            var existingDataSet = new DataSet();
            var existingTable = new DataTable();
            existingTable.Columns.Add("Id", typeof(int));
            existingTable.Columns.Add("Name", typeof(string));
            
            // Add existing row with Id = 1 (should be updated)
            var tableRow = existingTable.NewRow();
            tableRow["Id"] = 1;
            tableRow["Name"] = "Original John";
            existingTable.Rows.Add(tableRow);
            
            existingDataSet.Tables.Add(existingTable);

            var processorMock = new Mock<IMigrationProcessor>();
            processorMock.Setup(x => x.ReadTableData("TestSchema", "TestTable"))
                        .Returns(existingDataSet);

            expression.ExecuteWith(processorMock.Object);

            // Should call Process with both UpdateDataExpression and InsertDataExpression
            processorMock.Verify(x => x.Process(It.IsAny<UpdateDataExpression>()), Times.Once);
            processorMock.Verify(x => x.Process(It.IsAny<InsertDataExpression>()), Times.Once);
        }

        [Test]
        public void ExecuteWithCorrectlyBuildsUpdateExpression()
        {
            var expression = new MergeDataExpression
            {
                TableName = "TestTable",
                SchemaName = "TestSchema"
            };

            expression.MatchColumns.Add("Id");

            var row = new InsertionDataDefinition();
            row.Add(new KeyValuePair<string, object>("Id", 1));
            row.Add(new KeyValuePair<string, object>("Name", "Updated John"));
            row.Add(new KeyValuePair<string, object>("Email", "john@updated.com"));
            expression.Rows.Add(row);

            var existingDataSet = new DataSet();
            var existingTable = new DataTable();
            existingTable.Columns.Add("Id", typeof(int));
            existingTable.Columns.Add("Name", typeof(string));
            existingTable.Columns.Add("Email", typeof(string));
            
            var tableRow = existingTable.NewRow();
            tableRow["Id"] = 1;
            tableRow["Name"] = "Original John";
            tableRow["Email"] = "john@original.com";
            existingTable.Rows.Add(tableRow);
            
            existingDataSet.Tables.Add(existingTable);

            UpdateDataExpression capturedUpdate = null;
            var processorMock = new Mock<IMigrationProcessor>();
            processorMock.Setup(x => x.ReadTableData("TestSchema", "TestTable"))
                        .Returns(existingDataSet);
            processorMock.Setup(x => x.Process(It.IsAny<UpdateDataExpression>()))
                        .Callback<UpdateDataExpression>(expr => capturedUpdate = expr);

            expression.ExecuteWith(processorMock.Object);

            capturedUpdate.ShouldNotBeNull();
            capturedUpdate.TableName.ShouldBe("TestTable");
            capturedUpdate.SchemaName.ShouldBe("TestSchema");
            capturedUpdate.IsAllRows.ShouldBeFalse();
            
            // Should set Name and Email (non-match columns)
            capturedUpdate.Set.Count.ShouldBe(2);
            capturedUpdate.Set.ShouldContain(kvp => kvp.Key == "Name" && kvp.Value.ToString() == "Updated John");
            capturedUpdate.Set.ShouldContain(kvp => kvp.Key == "Email" && kvp.Value.ToString() == "john@updated.com");
            
            // Should have where clause for Id (match column)
            capturedUpdate.Where.Count.ShouldBe(1);
            capturedUpdate.Where.ShouldContain(kvp => kvp.Key == "Id" && (int)kvp.Value == 1);
        }

        [Test]
        public void ExecuteWithCorrectlyBuildsInsertExpression()
        {
            var expression = new MergeDataExpression
            {
                TableName = "TestTable",
                SchemaName = "TestSchema"
            };

            expression.MatchColumns.Add("Id");

            var row = new InsertionDataDefinition();
            row.Add(new KeyValuePair<string, object>("Id", 2));
            row.Add(new KeyValuePair<string, object>("Name", "New Jane"));
            expression.Rows.Add(row);

            var existingDataSet = new DataSet();
            var existingTable = new DataTable();
            existingTable.Columns.Add("Id", typeof(int));
            existingTable.Columns.Add("Name", typeof(string));
            existingDataSet.Tables.Add(existingTable);

            InsertDataExpression capturedInsert = null;
            var processorMock = new Mock<IMigrationProcessor>();
            processorMock.Setup(x => x.ReadTableData("TestSchema", "TestTable"))
                        .Returns(existingDataSet);
            processorMock.Setup(x => x.Process(It.IsAny<InsertDataExpression>()))
                        .Callback<InsertDataExpression>(expr => capturedInsert = expr);

            expression.ExecuteWith(processorMock.Object);

            capturedInsert.ShouldNotBeNull();
            capturedInsert.TableName.ShouldBe("TestTable");
            capturedInsert.SchemaName.ShouldBe("TestSchema");
            capturedInsert.Rows.Count.ShouldBe(1);
            
            var insertedRow = capturedInsert.Rows[0];
            insertedRow.Count.ShouldBe(2);
            insertedRow.ShouldContain(kvp => kvp.Key == "Id" && (int)kvp.Value == 2);
            insertedRow.ShouldContain(kvp => kvp.Key == "Name" && kvp.Value.ToString() == "New Jane");
        }
    }
}