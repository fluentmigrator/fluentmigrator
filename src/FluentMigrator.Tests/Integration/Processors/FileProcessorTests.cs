using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using NUnit.Framework;
using NUnit.Should;

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
			_fileDumpProcessor = new FileProcessor(_dumpFilename, _generator);
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
			var expression = new CreateTableExpression { TableName = _tableName };
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
		public FileProcessor(string dumpFilename, IMigrationGenerator generator)
		{
			this.generator = generator;
			DumpFilename = string.Format("{0}.{1}.sql", dumpFilename, FormattedDateTime);
			File.Delete(DumpFilename);
		}

		public string DumpFilename { get; set; }
		private string FormattedDateTime
		{
			get { return DateTime.Now.ToString("yyyyMMdd"); }
		}

		protected override void Process(string sql)
		{
			File.AppendAllText(DumpFilename, sql);
		}

		public override void Execute(string template, params object[] args)
		{
			throw new NotImplementedException();
		}

		public override void UpdateTable(string tableName, List<string> columns, List<string> formattedValues)
		{
			throw new NotImplementedException();
		}

		public override DataSet ReadTableData(string tableName)
		{
			throw new NotImplementedException();
		}

		public override DataSet Read(string template, params object[] args)
		{
			throw new NotImplementedException();
		}

		public override bool TableExists(string tableName)
		{
			throw new NotImplementedException();
		}

		public override bool Exists(string template, params object[] args)
		{
			throw new NotImplementedException();
		}

		public override void DeleteWhere(string tableName, string column, string equals)
		{
			throw new NotImplementedException();
		}
	}
}
