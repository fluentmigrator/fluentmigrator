using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace FluentMigrator.T4
{
    /// <summary>
    /// Summary for the InflectorRule class
    /// </summary>
    internal class InflectorRule {
        /// <summary>
        /// 
        /// </summary>
        public readonly Regex regex;

        /// <summary>
        /// 
        /// </summary>
        public readonly string replacement;

        /// <summary>
        /// Initializes a new instance of the <see cref="InflectorRule"/> class.
        /// </summary>
        /// <param name="regexPattern">The regex pattern.</param>
        /// <param name="replacementText">The replacement text.</param>
        public InflectorRule(string regexPattern, string replacementText) {
            this.regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            this.replacement = replacementText;
        }

        /// <summary>
        /// Applies the specified word.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public string Apply(string word) {
            if (!this.regex.IsMatch(word))
                return null;

            string replace = this.regex.Replace(word, this.replacement);
            if (word == word.ToUpper())
                replace = replace.ToUpper();

            return replace;
        }
    }
}