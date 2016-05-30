using System;

namespace FluentMigrator
{
    ///<summary>
    /// Used to filter which migrations are run.
    ///</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    [CLSCompliant(false)]
    public class TagsAttribute : Attribute
    {
        public string[] TagNames { get; private set; }

        [CLSCompliant(false)]
        public TagsAttribute(params string[] tagNames)
        {
            TagNames = tagNames;
        }

        public TagsAttribute(string tagName1)
            : this(new[] { tagName1 })
        {
        }

        public TagsAttribute(string tagName1, string tagName2)
            : this(new[] { tagName1, tagName2 })
        {
        }

        public TagsAttribute(string tagName1, string tagName2, string tagName3)
            : this(new[] { tagName1, tagName2, tagName3 })
        {
        }

        public TagsAttribute(string tagName1, string tagName2, string tagName3, string tagName4)
            : this(new[] { tagName1, tagName2, tagName3, tagName4 })
        {
        }
    }
}
