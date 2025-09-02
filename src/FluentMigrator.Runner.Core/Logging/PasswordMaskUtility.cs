using System.Text.RegularExpressions;

namespace FluentMigrator.Runner.Logging
{
    /// <summary>
    /// Utility class for masking sensitive information, such as passwords, in connection strings.
    /// </summary>
    /// <remarks>
    /// This class provides functionality to replace sensitive data in connection strings with masked values
    /// to enhance security and prevent accidental exposure of sensitive information.
    /// </remarks>
    public class PasswordMaskUtility : IPasswordMaskUtility
    {
        private static readonly Regex _matchPwd = new Regex("(?<Prefix>.*)(?<Key>PWD\\s*=\\s*|PASSWORD\\s*=\\s*)(?<OptionalValue>((?<None>(;|;$|$))|(?<Value>(([^;]+$|[^;]+)))(?<ValueTerminator>$|;)))(?<Postfix>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc />
        public string ApplyMask(string connectionString)
        {
            return _matchPwd.Replace(connectionString, "${Prefix}${Key}********${None}${ValueTerminator}${Postfix}");
        }
    }
}
