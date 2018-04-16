
namespace FluentMigrator
{
    /// <summary>
    /// The well-known system methods
    /// </summary>
    public enum SystemMethods
    {
        /// <summary>
        /// The function to create a new GUID
        /// </summary>
        NewGuid,

        /// <summary>
        /// The function to create a new sequential GUID
        /// </summary>
        NewSequentialId,

        /// <summary>
        /// The function to get the current timestamp
        /// </summary>
        CurrentDateTime,

        /// <summary>
        /// The function to get the current timestamp with time zone
        /// </summary>
        CurrentDateTimeOffset,

        /// <summary>
        /// The function to get the current UTC timestamp
        /// </summary>
        CurrentUTCDateTime,

        /// <summary>
        /// The function to get the current user
        /// </summary>
        CurrentUser,
    }
}
