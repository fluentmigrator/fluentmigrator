using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Model
{
    /// <summary>
    /// Represents an <see cref="IDataDefinition"/> that contains
    /// a defined collection of <see cref="IDataValue"/> instances
    /// </summary>
    public class ExplicitDataDefinition : IDataDefinition
    {
        /// <summary>
        /// Instantiates a new <see cref="ExplicitDataDefinition"/> containing
        /// the specified <see cref="IDataValue"/> instances
        /// </summary>
        /// <param name="data"></param>
        public ExplicitDataDefinition(params IDataValue[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Instantiates a new <see cref="ExplicitDataDefinition"/> containing
        /// the specified <see cref="IDataValue"/> instances
        /// </summary>
        /// <param name="data"></param>
        public ExplicitDataDefinition(IEnumerable<IDataValue> data)
        {
            Data = data.ToArray();
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="IDataValue"/>
        /// </summary>
        public IEnumerable<IDataValue> Data { get; private set; }
    }
}
