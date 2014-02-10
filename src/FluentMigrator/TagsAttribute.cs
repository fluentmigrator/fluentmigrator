using System;

namespace FluentMigrator
{
    ///<summary>
    /// Used to filter which migrations are run.
    ///</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class TagsAttribute : Attribute
    {
        public string[] TagNames { get; private set; }


        public TagsAttribute()  // Added only to remove a compiler warning
        {
        }

        public TagsAttribute(params string[] tagNames)
        {
            TagNames = tagNames;
        }
    }
}