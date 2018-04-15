using System.Collections.Generic;

namespace FluentMigrator.Infrastructure
{
    public interface ISupportAdditionalFeatures
    {
        IDictionary<string, object> AdditionalFeatures { get; }
    }
}
