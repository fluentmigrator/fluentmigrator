using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// Represents an object that is able to extract <see cref="IDataValue"/> instances
    /// from a <see cref="IDataDefinition"/> (and derived classes)
    /// </summary>
    public interface IEvaluator
    {
        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="IDataValue"/> for
        /// the specified <see cref="ReflectedDataDefinition"/>
        /// </summary>
        /// <param name="dataDefinition"></param>
        /// <returns></returns>
        IEnumerable<IDataValue> Evaluate(ReflectedDataDefinition dataDefinition);

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="IDataValue"/> for
        /// the specified <see cref="ExplicitDataDefinition"/>
        /// </summary>
        /// <param name="dataDefinition"></param>
        /// <returns></returns>
        IEnumerable<IDataValue> Evaluate(ExplicitDataDefinition dataDefinition);

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="IDataValue"/> for
        /// the specified <see cref="IDataDefinition"/>
        /// </summary>
        /// <param name="dataDefinition"></param>
        /// <returns></returns>
        IEnumerable<IDataValue> Evaluate(IDataDefinition dataDefinition);
    }
}
