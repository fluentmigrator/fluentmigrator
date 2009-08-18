using System;
using System.Collections.Generic;

namespace FluentMigrator.Infrastructure.Extensions
{
	public static class ExtensionsForIEnumerable
	{
		public static IEnumerable<T> CloneAll<T>(this IEnumerable<T> items)
			where T : ICloneable
		{
			foreach (T item in items)
				yield return (T)item.Clone();
		}
	}
}