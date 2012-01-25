namespace FluentMigrator.Runner.Generators
{
    /// <summary>Extends <see cref="CompatibilityMode"/>.</summary>
    public static class CompatibilityModeExtensions
    {
        /// <summary>Whether the mode includes the specified flag.</summary>
        /// <param name="mode">The compatibility mode.</param>
        /// <param name="flag">The mode to check.</param>
        public static bool HasFlag(this CompatibilityMode mode, CompatibilityMode flag)
        {
            return (mode & flag) > 0;
        }

        /// <summary>Generate a blank string for an unsupported SQL command, or throw an exception if the generator is in strict compatibility mode.</summary>
        /// <param name="mode">The extended compatibility mode.</param>
        /// <param name="message">The exception message describing the incompatibility.</param>
        /// <exception cref="DatabaseOperationNotSupportedException">The SQL command is not supported by the underlying database, and the generator is in strict compatibility mode.</exception>
        public static string GetNotSupported(this CompatibilityMode mode, string message)
        {
            if (mode.HasFlag(CompatibilityMode.Strict))
                throw new DatabaseOperationNotSupportedException(message);
            return string.Empty;
        }
    }
}