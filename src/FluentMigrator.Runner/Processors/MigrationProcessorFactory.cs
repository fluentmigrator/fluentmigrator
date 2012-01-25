namespace FluentMigrator.Runner.Processors
{
    public abstract class MigrationProcessorFactory : IMigrationProcessorFactory
    {
        public abstract IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options);

        public virtual bool IsForProvider(string provider)
        {
            return provider.ToLower().Contains(Name.ToLower());
        }

        public virtual string Name
        {
            get { return GetType().Name.Replace("ProcessorFactory", string.Empty); }
        }

        /// <summary>Constructs a configured migration generator.</summary>
        /// <typeparam name="T">The migration generator implementation type.</typeparam>
        /// <param name="options">The options with which to configure the generator.</param>
        protected IMigrationGenerator GetGenerator<T>(IMigrationProcessorOptions options)
            where T : IMigrationGenerator, new()
        {
            return new T
            {
                CompatibilityMode = options.CompatibilityMode
            };
        }
    }
}