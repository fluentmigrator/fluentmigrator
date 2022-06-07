namespace FluentMigrator.Runner.Logging
{
    public interface IPasswordMaskUtility
    {
        string ApplyMask(string connectionString);
    }
}
