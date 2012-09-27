using System.Collections.Generic;

namespace FluentMigrator.Runner.Generators.MySql
{
    /// <summary>
    /// Static class containing method names and signatures of MySql functions
    /// </summary>
    public static class MySqlFunctions
    {
        private static readonly MySqlQuoter Quoter = new MySqlQuoter();

        /// <summary>
        /// The name of the Hex function
        /// </summary>
        public static readonly string HexName = "HEX";

        /// <summary>
        /// Generates a call to the Hex function
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Hex(string value)
        {
            return string.Format("{0}({1})", HexName, Quoter.QuoteValue(value));
        }

        /// <summary>
        /// The name of the Unhex function
        /// </summary>
        public static readonly string UnhexName = "UNHEX";

        /// <summary>
        /// Generates a call to the Unhex funciton
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Unhex(string value)
        {
            return string.Format("{0}({1})", UnhexName, Quoter.QuoteValue(value));
        }

        /// <summary>
        /// Gets a colleciton of known funciton names
        /// </summary>
        public static IEnumerable<string> FunctionNames
        {
            get 
            {
                yield return HexName;
                yield return UnhexName;
            }
        }
    }
}
