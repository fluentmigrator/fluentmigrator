#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Collections.Generic;
using System.Data;
using System.IO;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Runner.Generators.SQLite;

namespace FluentMigrator.Tests.Integration.Processors
{
	[TestFixture]
	public class FileProcessorTests
	{
		private string _dumpFilename;
		private FileProcessor _fileDumpProcessor;
		private SqliteGenerator _generator;
		private string _tableName;
		private string _columnName;

		[SetUp]
		public void SetUp()
		{
			_dumpFilename = "createtable.dump";
			_tableName = "sample_table";
			_columnName = "sample_column_id";

			_generator = new SqliteGenerator();
			_fileDumpProcessor = new FileProcessor(_dumpFilename, _generator, new TextWriterAnnouncer(System.Console.Out), new ProcessorOptions());
		}

		[Test]
		public void DumpFilenameShouldContainDateTime()
		{
			string formattedDateTime = DateTime.Now.ToString("yyyyMMdd");
			string expectedFilename = string.Format("{0}.{1}.sql", _dumpFilename, formattedDateTime);
			_fileDumpProcessor.DumpFilename.ShouldBe(expectedFilename);
		}

		[Test]
		public void DumpFilenameShouldHaveProperFileExtension()
		{
			int startIndex = _fileDumpProcessor.DumpFilename.Length - 4;
			_fileDumpProcessor.DumpFilename.Substring(startIndex, 4).ShouldBe(".sql");
		}

		[Test]
		public void CanDumpCreateTableExpression()
		{
			var expression = new CreateTableExpression { TableName = _tableName, Columns = new List<ColumnDefinition>{new ColumnDefinition{Name = "TestColumn"}}};
			string expectedSql = _generator.Generate(expression);

			_fileDumpProcessor.Process(expression);

			Lines.ShouldContain(expectedSql);
		}

		[Test]
		public void CanDumpCreateColumnExpression()
		{
			var expression = new CreateColumnExpression { TableName = _tableName, Column = { Name = _columnName, IsIdentity = true, IsPrimaryKey = true, Type = DbType.String } };
			string expectedSql = _generator.Generate(expression);

			_fileDumpProcessor.Process(expression);

			Lines.ShouldContain(expectedSql);
		}

		private IEnumerable<string> Lines
		{
			get
			{
				string line;
				using (var stream = File.OpenText(_fileDumpProcessor.DumpFilename))
					while ((line = stream.ReadLine()) != null)
						yield return line;
			}
		}
	}

	public class FileProcessor : ProcessorBase
	{
		public FileProcessor(string dumpFilename, IMigrationGenerator generator, IAnnouncer announcer, IMigrationProcessorOptions options)
			: base(generator, announcer, options)
		{
			DumpFilename = string.Format("{0}.{1}.sql", dumpFilename, FormattedDateTime);
			File.Delete(DumpFilename);
		}

		public string DumpFilename { get; set; }
		private string FormattedDateTime
		{
			get { return DateTime.Now.ToString("yyyyMMdd"); }
		}

		public override void Process(PerformDBOperationExpression expression)
		{
			throw new NotImplementedException();
		}

		protected override void Process(string sql)
		{
			File.AppendAllText(DumpFilename, sql);
		}

		public override void Execute(string template, params object[] args)
		{
			throw new NotImplementedException();
		}

        public override DataSet ReadTableData(string schemaName, string tableName)
		{
			throw new NotImplementedException();
		}

		public override DataSet Read(string template, params object[] args)
		{
			throw new NotImplementedException();
		}

		public override bool SchemaExists(string tableName)
		{
			throw new NotImplementedException();
		}

        public override bool TableExists(string schemaName, string tableName)
		{
			throw new NotImplementedException();
		}

        public override bool ColumnExists(string schemaName, string tableName, string columnName)
		{
			throw new NotImplementedException();
		}

        public override bool ConstraintExists(string schemaName, string tableName, string constraintName)
		{
			throw new NotImplementedException();
		}

        public override bool IndexExists(string schemaName, string tableName, string indexName)
        {
            throw new NotImplementedException();
        }

		public override bool Exists(string template, params object[] args)
		{
			throw new NotImplementedException();
		}
	}
}
