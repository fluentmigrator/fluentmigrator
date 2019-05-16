using System;

namespace FluentMigrator
{
    ///<summary>
    /// Used to filter which migrations are run.
    ///</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class TagsAttribute : Attribute
    {
        /// <summary>
        /// Gets the behavior of the runner when evaluating <see cref="TagNames" />.
        /// </summary>
        public TagBehavior Behavior { get; private set; }

        /// <summary>
        /// Gets the names of the tags that are evaluated by the runner.
        /// </summary>
        public string[] TagNames { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsAttribute" /> class.
        /// </summary>
        /// <param name="behavior">The behavior of the runner when evaluating <paramref name="tagNames"/>.</param>
        /// <param name="tagNames">The names of the tags that are evaluated by the runner.</param>
        public TagsAttribute(TagBehavior behavior, params string[] tagNames)
        {
            Behavior = behavior;
            TagNames = tagNames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsAttribute" /> class.
        /// </summary>
        /// <param name="tagNames">The names of the tags that are evaluated by the runner.</param>
        public TagsAttribute(params string[] tagNames)
            : this(TagBehavior.RequireAll, tagNames)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsAttribute" /> class.
        /// </summary>
        /// <param name="tagName1">The tag that is evaluated by the runner.</param>
        public TagsAttribute(string tagName1)
            : this(new[] { tagName1 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsAttribute" /> class.
        /// </summary>
        /// <param name="tagName1">The first tag that is evaluated by the runner.</param>
        /// <param name="tagName2">The second tag that is evaluated by the runner.</param>
        public TagsAttribute(string tagName1, string tagName2)
            : this(new[] { tagName1, tagName2 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsAttribute" /> class.
        /// </summary>
        /// <param name="tagName1">The first tag that is evaluated by the runner.</param>
        /// <param name="tagName2">The second tag that is evaluated by the runner.</param>
        /// <param name="tagName3">The third tag that is evaluated by the runner.</param>
        public TagsAttribute(string tagName1, string tagName2, string tagName3)
            : this(new[] { tagName1, tagName2, tagName3 })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsAttribute" /> class.
        /// </summary>
        /// <param name="tagName1">The first tag that is evaluated by the runner.</param>
        /// <param name="tagName2">The second tag that is evaluated by the runner.</param>
        /// <param name="tagName3">The third tag that is evaluated by the runner.</param>
        /// <param name="tagName4">The fourth tag that is evaluated by the runner.</param>
        public TagsAttribute(string tagName1, string tagName2, string tagName3, string tagName4)
            : this(new[] { tagName1, tagName2, tagName3, tagName4 })
        {
        }
    }
}
