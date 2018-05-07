using System;

namespace FluentMigrator
{
    /// <summary>
    /// Defines a profile for a migration
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ProfileAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileAttribute"/> class.
        /// </summary>
        /// <param name="profileName">The profile name</param>
        public ProfileAttribute(string profileName)
        {
            ProfileName = profileName;
        }

        /// <summary>
        /// Gets the profile name
        /// </summary>
        public string ProfileName { get; }
    }
}
