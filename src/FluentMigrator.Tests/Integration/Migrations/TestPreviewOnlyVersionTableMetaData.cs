using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Tests.Integration.Migrations
{
    [VersionTableMetaData]
    public class TestPreviewOnlyVersionTableMetaData : IVersionTableMetaData
    {
        public const string TABLE_NAME = "testVersionTableName";
        public const string COLUMN_NAME = "testColumnName";
        public const string UNIQUE_INDEX_NAME = "testUniqueIndexName";
        public const string DESCRIPTION_COLUMN_NAME = "testDescriptionColumnName";
        public const string APPLIED_ON_COLUMN_NAME = "testAppliedOnColumnName";

        public TestPreviewOnlyVersionTableMetaData()
        {
            SchemaName = "testSchemaName";
            OwnsSchema = true;
        }

        public object ApplicationContext { get; set; }

        public string SchemaName { get; set; }

        public string TableName
        {
            get { return TABLE_NAME; }
        }

        public string ColumnName
        {
            get { return COLUMN_NAME; }
        }

        public string UniqueIndexName
        {
            get { return UNIQUE_INDEX_NAME; }
        }

        public string AppliedOnColumnName
        {
            get { return APPLIED_ON_COLUMN_NAME; }
        }

        public string DescriptionColumnName
        {
            get { return DESCRIPTION_COLUMN_NAME; }
        }

        public bool OwnsSchema { get; set; }
    }
}
