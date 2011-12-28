namespace FluentMigrator.Builders.Insert
{
    public interface IInsertDataAdditionalFeatures
    {
        IInsertDataSyntax AddAdditionalFeature(string feature, object value);
    }
}
