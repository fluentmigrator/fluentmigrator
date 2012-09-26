using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators.Generic
{
    public class GenericEvaluator : IEvaluator
    {
        private static readonly IEnumerable<KeyValuePair<Func<IDataDefinition, bool>, Func<IEvaluator, IDataDefinition, IEnumerable<IDataValue>>>> DefinitionParsers = new List<KeyValuePair<Func<IDataDefinition, bool>, Func<IEvaluator, IDataDefinition, IEnumerable<IDataValue>>>>
        {
            new KeyValuePair<Func<IDataDefinition, bool>, Func<IEvaluator, IDataDefinition, IEnumerable<IDataValue>>>(dd => dd is ReflectedDataDefinition, (dp, dd) => dp.Evaluate((ReflectedDataDefinition)dd)),
            new KeyValuePair<Func<IDataDefinition, bool>, Func<IEvaluator, IDataDefinition, IEnumerable<IDataValue>>>(dd => dd is ExplicitDataDefinition, (dp, dd) => dp.Evaluate((ExplicitDataDefinition)dd)),
        };

        public virtual IDataValue GetDataValue(PropertyDescriptor propertyDescriptor, object data)
        {
            return new DataValue(propertyDescriptor.Name, propertyDescriptor.GetValue(data));
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="IDataValue"/> for
        /// the specified <see cref="ReflectedDataDefinition"/>
        /// </summary>
        /// <param name="dataDefinition"></param>
        /// <returns></returns>
        public virtual IEnumerable<IDataValue> Evaluate(ReflectedDataDefinition dataDefinition)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(dataDefinition.Data);

            return properties.OfType<PropertyDescriptor>().Select(property => GetDataValue(property, dataDefinition.Data));
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="IDataValue"/> for
        /// the specified <see cref="ExplicitDataDefinition"/>
        /// </summary>
        /// <param name="dataDefinition"></param>
        /// <returns></returns>
        public virtual IEnumerable<IDataValue> Evaluate(ExplicitDataDefinition dataDefinition)
        {
            return dataDefinition.Data;
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="IDataValue"/> for
        /// the specified <see cref="IDataDefinition"/>
        /// </summary>
        /// <param name="dataDefinition"></param>
        /// <returns></returns>
        public virtual IEnumerable<IDataValue> Evaluate(IDataDefinition dataDefinition)
        {
            var definitionParser = DefinitionParsers.First(dp => dp.Key(dataDefinition));
            
            return definitionParser.Value(this, dataDefinition);
        }
    }
}
