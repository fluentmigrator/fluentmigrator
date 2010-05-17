using System;

namespace FluentMigrator.Runner.Announcers
{
	public class NullAnnouncer : IAnnouncer
	{
		#region IAnnouncer Members

		public void Dispose()
		{
		}

		public void Heading(string message)
		{
		}

		public void Say(string message)
		{
		}

		public void Sql(string sql)
		{
		}

		public void ElapsedTime(TimeSpan timeSpan)
		{
		}

		public void Error(string message)
		{
		}

		#endregion
	}
}