using System;
using System.ComponentModel;

namespace FluentMigrator.Infrastructure
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IFluentSyntax
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		Type GetType();

		[EditorBrowsable(EditorBrowsableState.Never)]
		int GetHashCode();

		[EditorBrowsable(EditorBrowsableState.Never)]
		string ToString();

		[EditorBrowsable(EditorBrowsableState.Never)]
		bool Equals(object other);
	}
}