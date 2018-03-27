using System;

namespace FluentMigrator.Runner.Processors.Firebird
{
    using Generators.Firebird;  

    public class FirebirdProcessorFactory : MigrationProcessorFactory
    {
        public FirebirdOptions FBOptions { get; set; }

        public FirebirdProcessorFactory() : this(FirebirdOptions.AutoCommitBehaviour()) { }
        public FirebirdProcessorFactory(FirebirdOptions fbOptions)
            : base()
        {
            if (fbOptions == null)
                throw new ArgumentNullException("fbOptions");
            FBOptions = fbOptions;
        }
        
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new FirebirdDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new FirebirdProcessor(connection, new FirebirdGenerator(FBOptions), announcer, options, factory, FBOptions);
        }
    }
}