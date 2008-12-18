using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentMigrator.Runner.Processors;
using Xunit;

namespace FluentMigrator.Tests
{
	public class RenameColumnExpressionTests
	{
		private RenameColumnExpression _renameColumnExpression;
		private ICollection<string> _errors;

		public RenameColumnExpressionTests()
		{
			_renameColumnExpression = new RenameColumnExpression();
			_errors = new Collection<string>();
		}

		[Fact]
		public void OmmittingNewNameWillAddError()
		{
			_renameColumnExpression.OldName = "Bacon";
			ExpectedErrorsAreAdded();
		}

		[Fact]
		public void OmmittingOldNameWillAddError()
		{
			_renameColumnExpression.NewName = "ChunkierBacon";
			ExpectedErrorsAreAdded();
		}

		private void ExpectedErrorsAreAdded()
		{
			_renameColumnExpression.CollectValidationErrors(_errors);

			Assert.NotEmpty(_errors);
			Assert.True(_errors.Count == 1);
		}
	}
}