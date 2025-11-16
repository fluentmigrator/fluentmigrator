namespace FluentMigrator.Runner.Logging
{
    /// <summary>
    /// Provides functionality to mask sensitive information, such as passwords, in connection strings.
    /// </summary>
    public interface IPasswordMaskUtility
    {
        /// <summary>
        /// Masks sensitive information, such as passwords, in the provided connection string.
        /// </summary>
        /// <param name="connectionString">The connection string that may contain sensitive information.</param>
        /// <returns>The connection string with sensitive information masked.</returns>
        string ApplyMask(string connectionString);
    }
}
