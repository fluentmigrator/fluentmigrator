using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Infrastructure
{
	public static class ExtensionsForIEnumerableOfString
	{
		public static string Join(this IEnumerable<string> items, string separator)
		{
			return String.Join(separator, items.ToArray());
		}
	}
}