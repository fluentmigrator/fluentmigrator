using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Tests
{
	public static class ValidationHelper
	{
		public static ICollection<string> CollectErrors(params ICanBeValidated[] items)
		{
			var collection = new List<string>();

			foreach (ICanBeValidated item in items)
				item.CollectValidationErrors(collection);

			return collection;
		}
	}
}