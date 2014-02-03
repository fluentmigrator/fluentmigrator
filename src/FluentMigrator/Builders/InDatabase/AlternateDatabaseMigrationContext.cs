using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.InDatabase
{
    public class AlternateDatabaseMigrationContext : MigrationContext
    {
        private readonly IMultiDatabaseMigrationProcessor _migrationProcessor;
        private readonly string _databaseKey;

        public AlternateDatabaseMigrationContext(IMigrationContext parent, IMultiDatabaseMigrationProcessor migrationProcessor, string databaseKey)
            : base(parent.Conventions, migrationProcessor.GetProcessorByDatabaseKey(databaseKey), parent.MigrationAssembly, parent.ApplicationContext, migrationProcessor.ConnectionString)
        {
            _migrationProcessor = migrationProcessor;
            _databaseKey = databaseKey;
            Expressions = new InterceptingExpressionCollection(parent.Expressions, e => _migrationProcessor.AssignDatabaseKey(e, _databaseKey));
        }

        public override ICollection<IMigrationExpression> Expressions { get; set; }
        
        private class InterceptingExpressionCollection : ICollection<IMigrationExpression>
        {
            private readonly ICollection<IMigrationExpression> _parent;
            private readonly Action<IMigrationExpression> _processBeforeAdd;

            public InterceptingExpressionCollection(ICollection<IMigrationExpression> parent, Action<IMigrationExpression> processBeforeAdd)
            {
                _parent = parent;
                _processBeforeAdd = processBeforeAdd;
            }

            public void Add(IMigrationExpression item) 
            {
                _processBeforeAdd(item);
                _parent.Add(item);
            }

            public void Clear()
            {
                _parent.Clear();
            }

            public bool Contains(IMigrationExpression item)
            {
                return _parent.Contains(item);
            }

            public void CopyTo(IMigrationExpression[] array, int arrayIndex)
            {
                _parent.CopyTo(array, arrayIndex);
            }

            public int Count 
            {
                get { return _parent.Count; }
            }

            public bool IsReadOnly
            {
                get { return _parent.IsReadOnly; }
            }

            public bool Remove(IMigrationExpression item)
            {
                return _parent.Remove(item);
            }

            public IEnumerator<IMigrationExpression> GetEnumerator()
            {
                return _parent.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() 
            {
                return _parent.GetEnumerator();
            }
        }
    }
}
