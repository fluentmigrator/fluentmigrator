
namespace FluentMigrator.Model
{
    /// <summary>
    /// Represents a value for a specific column
    /// </summary>
    public interface IDataValue
    {
        /// <summary>
        /// Gets the column name
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// Gets the column value
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Returns true if the value should be quoted, otherwise 
        /// returns false. Default is true.
        /// </summary>
        bool QuoteValue { get; }
    }

    /// <summary>
    /// Implementation of <see cref="IDataValue"/>
    /// </summary>
    public class DataValue : IDataValue
    {
        /// <summary>
        /// Instantiates a <see cref="DataValue"/> for the specified
        /// column with a null value
        /// </summary>
        /// <param name="columnName"></param>
        public DataValue(string columnName) : this(columnName, null)
        {

        }

        /// <summary>
        /// Instantiates a <see cref="DataValue"/> for the specified
        /// column with the specified value
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        public DataValue(string columnName, object value) : this(columnName, value, true)
        {

        }

        /// <summary>
        /// Instantiates a <see cref="DataValue"/> for the specified
        /// column with the specified value and sets QuoteValue to
        /// the specified parameter
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        /// <param name="quoteValue"></param>
        public DataValue(string columnName, object value, bool quoteValue)
        {
            ColumnName = columnName;
            Value = value;
            QuoteValue = quoteValue;
        }

        /// <summary>
        /// Gets the column name
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Gets the column value
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Returns true if the value should be quoted, otherwise 
        /// returns false. Default is true.
        /// </summary>
        public bool QuoteValue { get; private set; }
    }
}
