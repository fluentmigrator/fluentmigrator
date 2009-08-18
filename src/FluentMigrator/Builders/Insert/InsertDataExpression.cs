using System.Collections.Generic;
using FluentMigrator.Expressions;

namespace FluentMigrator.Builders.Insert
{
    public class InsertDataExpression : IMigrationExpression
    {
        private List<InsertionData> rows = new List<InsertionData>();
        public string TableName { get; set; }

        public List<InsertionData> Rows
        {
            get { return rows; }
        }

        public void CollectValidationErrors(ICollection<string> errors)
        {            
        }

        public void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        public IMigrationExpression Reverse()
        {
            return null;
        }
    }
}