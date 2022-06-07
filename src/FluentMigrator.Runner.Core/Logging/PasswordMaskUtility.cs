using System.Text.RegularExpressions;

namespace FluentMigrator.Runner.Logging
{
    public class PasswordMaskUtility : IPasswordMaskUtility
    {
        private static readonly Regex _matchPwd = new Regex("(?<Prefix>.*)(?<Key>PWD\\s*=\\s*|PASSWORD\\s*=\\s*)(?<OptionalValue>((?<None>(;|;$|$))|(?<Value>(([^;]+$|[^;]+)))(?<ValueTerminator>$|;)))(?<Postfix>.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string ApplyMask(string connectionString)
        {
            return _matchPwd.Replace(connectionString, "${Prefix}${Key}********${None}${ValueTerminator}${Postfix}");
        }
    }
}
