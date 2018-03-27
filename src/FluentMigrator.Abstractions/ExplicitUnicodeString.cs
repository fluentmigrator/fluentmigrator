namespace FluentMigrator
{
    /// <summary>
    /// Insert text with unicode data. Primarily for Sql Server, it prefixes the string with 'N
    /// </summary>
    public class ExplicitUnicodeString
    {
        public string Text { get; set; }

        /// <summary>
        /// Insert text with unicode data. Primarily for Sql Server, it prefixes the string with 'N
        /// </summary>
        /// <param name="text">Unicode string</param>
        public ExplicitUnicodeString(string text)
        {
            Text = text;
        }
        
        public override string ToString()
        {
            return Text;
        }
    }
}
