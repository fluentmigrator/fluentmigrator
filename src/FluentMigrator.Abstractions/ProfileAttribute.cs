using System;

namespace FluentMigrator
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ProfileAttribute : Attribute
    {
        public string ProfileName { get; private set; }

        public ProfileAttribute(string profileName)
        {
            ProfileName = profileName;
        }
    }
}