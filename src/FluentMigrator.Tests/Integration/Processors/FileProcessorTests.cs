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
		private string _dumpFile;

		[Test]
		public void CanDumpCreateTableExpression()
		{
			string testTableName = "sample_table";
			_dumpFile = "createtable.dump.sql";
			var generator = new SqliteGenerator();
			var fileDumpProcessor = new FileProcessor(_dumpFile, generator);

			CreateTableExpression expression = new CreateTableExpression { TableName = testTableName };
			fileDumpProcessor.Process(expression);
			string expectedSql = generator.Generate(expression);

			DumpedLines.ShouldContain(expectedSql);
		}

		private IEnumerable<string> DumpedLines
		{
			get
			{
				string line;
				using (var stream = File.OpenText(_dumpFile))
					while ((line = stream.ReadLine()) != null)
						yield return line;
			}
		}
	}

	public class FileProcessor : ProcessorBase
	{
		public FileProcessor(string dumpFile, IMigrationGenerator generator)
		{
			DumpFilename = dumpFile;
			File.Delete(DumpFilename);
			this.generator = generator;
		}

		protected string DumpFilename { get; set; }

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
	}
}
