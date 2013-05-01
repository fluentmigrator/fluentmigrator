using System;

namespace FluentMigrator.Runner
{
    public class MigrationScopeHandler
    {
        private readonly IMigrationProcessor _processor;

        public MigrationScopeHandler(IMigrationProcessor processor)
        {
            _processor = processor;
        }

        public IMigrationScope CurrentScope { get; set; }

        public IMigrationScope BeginScope()
        { 
            GuardAgainstActiveMigrationScope();
            CurrentScope = new TransactionalMigrationScope(_processor, ()=> CurrentScope = null);
            return CurrentScope;
        }

        public IMigrationScope CreateOrWrapMigrationScope(bool transactional = true)
        {
            if (HasActiveMigrationScope) return new NoOpMigrationScope();
            if (transactional) return BeginScope();
            return new NoOpMigrationScope();
        }

        private void GuardAgainstActiveMigrationScope()
        {
            if (HasActiveMigrationScope) throw new InvalidOperationException("The runner is already in an active migration scope.");
        }

        private bool HasActiveMigrationScope
        {
            get { return CurrentScope != null && CurrentScope.IsActive; }
        }
    }
}