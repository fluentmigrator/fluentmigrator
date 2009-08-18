using System;
using System.Collections.Generic;

namespace FluentMigrator.Infrastructure
{
	public interface ICanBeValidated
	{
		void CollectValidationErrors(ICollection<string> errors);
	}
}