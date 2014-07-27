namespace FluentMigrator.Infrastructure
{
    public interface ISupportAdditionalFeatures
    {
        void AddAdditionalFeature(string feature, object value);
    }
}
