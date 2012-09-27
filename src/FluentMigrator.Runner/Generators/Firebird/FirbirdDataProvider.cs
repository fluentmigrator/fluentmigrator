using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Generic;

namespace FluentMigrator.Runner.Generators.Firebird
{
    public class Firbirdevaluator : GenericEvaluator
    {
        private readonly FirebirdTruncator _trucator;

        public Firbirdevaluator(FirebirdTruncator truncator)
        {
            _trucator = truncator;
        }

        public override IEnumerable<IDataValue> Evaluate(Model.IDataDefinition dataDefinition)
        {
            return base.Evaluate(dataDefinition).Select(kvp => new DataValue(_trucator.Truncate(kvp.ColumnName), kvp.Value)).Cast<IDataValue>();
        }
    }
}
