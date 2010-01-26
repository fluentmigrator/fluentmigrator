using System;
using System.Collections.Generic;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner
{
	public class MigrationRunner
	{
		private IAnnouncer _announcer;
		public IMigrationConventions Conventions { get; private set; }
		public IMigrationProcessor Processor { get; private set; }
		public IList<Exception> CaughtExceptions { get; private set; }
		public bool SilentlyFail { get; set; }
		private IStopWatch _stopWatch;

		public MigrationRunner(IMigrationConventions conventions, IMigrationProcessor processor)
			: this(conventions, processor, new Announcer(Console.Out), new StopWatch())
		{
		}

		public MigrationRunner(IMigrationConventions conventions, IMigrationProcessor processor, IAnnouncer announcer, IStopWatch stopWatch)
		{
			_announcer = announcer;
			SilentlyFail = false;
			CaughtExceptions = null;
			Conventions = conventions;
			Processor = processor;
			_stopWatch = stopWatch;
		}

		public void Up(IMigration migration)
		{
			var name = migration.GetType().Name;
			_announcer.Announce(name + ": migrating");
			
			CaughtExceptions = new List<Exception>();

			var context = new MigrationContext(Conventions);

			migration.GetUpExpressions(context);

			_stopWatch.Start();

			ExecuteExpressions(context.Expressions);

			_stopWatch.Stop();
			
			var elapsed = _stopWatch.ElapsedTime().TotalSeconds;

			_announcer.Announce(name + ": migrated (" + elapsed + "s" + ")");
		}

		public void Down(IMigration migration)
		{
			var name = migration.GetType().Name;
			_announcer.Announce(name + ": reverting");
			
			CaughtExceptions = new List<Exception>();

			var context = new MigrationContext(Conventions);
			migration.GetDownExpressions(context);

			_stopWatch.Start();

			ExecuteExpressions(context.Expressions);

			_stopWatch.Stop();

			var elapsed = _stopWatch.ElapsedTime().TotalSeconds;

			_announcer.Announce(name + ": reverted (" + elapsed + "s" + ")");
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
					time(expression.ToString(), () => expression.ExecuteWith(Processor));
				}
				catch (Exception er)
				{
					_announcer.Say(er.Message);

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

		private void time(string message, Action action)
		{
			_announcer.Say(message);

			_stopWatch.Start();

			action();

			_stopWatch.Stop();

			var elapsed = _stopWatch.ElapsedTime().TotalSeconds;

			_announcer.SaySubItem(elapsed + "s");
		}
	}
}