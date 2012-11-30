
namespace System.Data
{
    static class DataReaderExtension
    {
        
        public static T Get<T>(this IDataReader reader, string columnName)
        {
            return reader.Get(columnName, default(T));
        }

        public static T Get<T>(this IDataReader reader, string columnName, T defaultValue)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException("columnName");
            T value = defaultValue;
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal) == false)
                value = (T)reader.GetValue(ordinal);
            return value;
        }
    }
}
