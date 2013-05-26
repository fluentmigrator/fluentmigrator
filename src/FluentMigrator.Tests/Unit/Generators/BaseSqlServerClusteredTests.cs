namespace FluentMigrator.Tests.Unit.Generators
{
    public abstract class BaseSqlServerClusteredTests
    {
        public abstract void CanCreateClusteredIndexWithCustomSchema();
        public abstract void CanCreateClusteredIndexWithDefaultSchema();
        public abstract void CanCreateMultiColumnClusteredIndexWithCustomSchema();
        public abstract void CanCreateMultiColumnClusteredIndexWithDefaultSchema();
        public abstract void CanCreateNamedClusteredPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateNamedClusteredPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateNamedClusteredUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedClusteredUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnClusteredPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnClusteredUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnClusteredUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnNonClusteredPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedMultiColumnNonClusteredUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateNamedNonClusteredPrimaryKeyConstraintWithCustomSchema();
        public abstract void CanCreateNamedNonClusteredPrimaryKeyConstraintWithDefaultSchema();
        public abstract void CanCreateNamedNonClusteredUniqueConstraintWithCustomSchema();
        public abstract void CanCreateNamedNonClusteredUniqueConstraintWithDefaultSchema();
        public abstract void CanCreateUniqueClusteredIndexWithCustomSchema();
        public abstract void CanCreateUniqueClusteredIndexWithDefaultSchema();
        public abstract void CanCreateUniqueClusteredMultiColumnIndexWithCustomSchema();
        public abstract void CanCreateUniqueClusteredMultiColumnIndexWithDefaultSchema();
    }
}
