
namespace FluentMigrator.Model
{
    /// <summary>
    /// Represents a <see cref="IDataDefinition"/> that uses reflection
    /// to retrieve <see cref="IDataValue"/> instances from an anonymous type
    /// </summary>
    public class ReflectedDataDefinition : IDataDefinition
    {
        /// <summary>
        /// Instantiates a <see cref="ReflectedDataDefinition"/> that
        /// contains the specified anonymously typed object
        /// </summary>
        /// <param name="data"></param>
        public ReflectedDataDefinition(object data)
        {
            Data = data;
        }

        /// <summary>
        /// Gets the anonymously typed data associated with the definition
        /// </summary>
        public object Data { get; private set; }
    }
}
