namespace FluentMigrator
{
    /// <summary>
    /// An explicitly non-Unicode string literal ('some string' in T-SQL)
    /// </summary>
    public class NonUnicodeString
    {
        /// <summary>
        /// The value of the non-Unicode string literal
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Used for explicitly creating a non-Unicode string literal in Transact SQL
        /// </summary>
        /// <param name="value">The value of the non-Unicode string literal</param>
        public NonUnicodeString(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Overrides ToString() to return the value.
        /// </summary>
        /// <returns>
        /// The value of the non-Unicode string literal.
        /// </returns>
        public override string ToString()
        {
            return Value;
        }
    }
}
