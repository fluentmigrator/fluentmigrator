using System;
using System.Collections.Generic;
using System.Data;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    public class PerformDBOperationExpression : MigrationExpressionBase
    {
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (Operation == null)
                errors.Add(ErrorMessages.OperationCannotBeNull);
        }

        public Action<IDbConnection, IDbTransaction> Operation { get; set; }
    }
}
