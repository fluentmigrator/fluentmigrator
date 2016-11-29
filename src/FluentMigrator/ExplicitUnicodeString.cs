using System;

namespace FluentMigrator
{
    /// <summary>
    /// Insert text with unicode data. Primarily for Sql Server, it prefixes the string with 'N
    /// </summary>
    [Obsolete("Use normal CLR strings instead, as they will be formatted to SQL Server Unicode strings")]
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
