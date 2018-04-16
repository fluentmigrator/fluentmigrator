using System.Collections.Generic;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// Provides a dictionary to store values for non-standard additional features
    /// </summary>
    public interface ISupportAdditionalFeatures
    {
        /// <summary>
        /// Gets the dictionary to store the values for additional features
        /// </summary>
        IDictionary<string, object> AdditionalFeatures { get; }
    }
}
