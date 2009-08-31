using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner
{
	public class MigrationRunner
	{
		public IMigrationConventions Conventions { get; private set; }
		public IMigrationProcessor Processor { get; private set; }
        public IList<Exception> CaughtExceptions { get; private set; }
        public bool SilentlyFail { get; set; }

		public MigrationRunner(IMigrationConventions conventions, IMigrationProcessor processor)
		{
            SilentlyFail = false;
            CaughtExceptions = null;
			Conventions = conventions;
			Processor = processor;
		}

		public void Up(IMigration migration)
		{
            CaughtExceptions = new List<Exception>();

			var context = new MigrationContext(Conventions);
			migration.GetUpExpressions(context);

			//process each expression
            ExecuteExpressions(context.Expressions);
		}

		public void Down(IMigration migration)
		{
            CaughtExceptions = new List<Exception>();

			var context = new MigrationContext(Conventions);
			migration.GetDownExpressions(context);

            //process each expression
            ExecuteExpressions(context.Expressions);
		}

        /// <summary>
        /// execute each migration expression in the expression collection
        /// </summary>
        /// <param name="expressions"></param>
        protected void ExecuteExpressions(ICollection<IMigrationExpression> expressions)
        {
            foreach (IMigrationExpression expression in expressions)
            {
                try
                {
                    expression.ExecuteWith(Processor);
                }
                catch (Exception er)
                {
                    //catch the error and move onto the next expression
                    if (SilentlyFail)
                    {
                        CaughtExceptions.Add(er);
                        continue;
                    }
                    throw;
                }
            }
        }
	}
}