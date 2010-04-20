using System.Data;

namespace FluentMigrator.Runner.Processors {
    public abstract class MigrationProcessorFactory : IMigrationProcessorFactory {
        public abstract IMigrationProcessor Create(string connectionString);
        public abstract IMigrationProcessor Create(IDbConnection connection);
        
        public virtual bool IsForProvider(string provider) {
            return provider.ToLower().Contains(Name.ToLower());
        }

        public virtual string Name {
            get { return GetType().Name.Replace("ProcessorFactory", string.Empty); }
        }
    }
}