using System;

namespace FluentMigrator
{
    /// <summary>
    /// A wrapper class for a SQL value
    /// </summary>
    /// <remarks>
    /// This raw SQL value is normally used when a custom default value was specified
    /// </remarks>
    public class RawSql
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawSql"/> class.
        /// </summary>
        /// <param name="underlyingSql">The underlying SQL value</param>
        private RawSql(string underlyingSql)
        {
            Value = underlyingSql ?? throw new ArgumentNullException(nameof(underlyingSql));
        }

        /// <summary>
        /// Gets the underlying SQL value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Creates a new <see cref="RawSql"/> instance using a given SQL value
        /// </summary>
        /// <param name="sqlToRun">The SQL value</param>
        /// <returns>The new <see cref="RawSql"/> instance</returns>
        public static RawSql Insert(string sqlToRun)
        {
            return new RawSql(sqlToRun);
        }
    }
}
