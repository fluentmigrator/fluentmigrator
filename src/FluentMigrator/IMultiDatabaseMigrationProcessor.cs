using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;

namespace FluentMigrator {
    public interface IMultiDatabaseMigrationProcessor : IMigrationProcessor {
        IMigrationProcessor DefaultProcessor { get; }
        void AssignDatabaseKey(IMigrationExpression expression, string databaseKey);
        bool HasDatabaseKey(string databaseKey);
        IEnumerable<string> GetDatabaseKeys();
        IMigrationProcessor GetProcessorByDatabaseKey(string databaseKey);
    }
}
