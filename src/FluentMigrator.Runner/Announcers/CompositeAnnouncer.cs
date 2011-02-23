using System;
using System.Collections.Generic;

namespace FluentMigrator.Runner.Announcers
{
	public class CompositeAnnouncer : IAnnouncer
	{
		private readonly IEnumerable<IAnnouncer> _announcers;

		public CompositeAnnouncer(IEnumerable<IAnnouncer> announcers)
		{
			_announcers = announcers;
		}

		#region IAnnouncer Members

		public void Dispose()
		{
			Each(a => a.Dispose());
		}

		public void Heading(string message)
		{
			Each(a => a.Heading(message));
		}

		public void Say(string message)
		{
			Each(a => a.Say(message));
		}

        public void Heading(string message,params object[] args)
        {
            Heading(string.Format(message, args));
        }

        public void Say(string message, params object[] args)
        {
            Say(string.Format(message, args));
        }

		public void Sql(string sql)
		{
			Each(a => a.Sql(sql));
		}

		public void ElapsedTime(TimeSpan timeSpan)
		{
			Each(a => a.ElapsedTime(timeSpan));
		}

		public void Error(string message)
		{
			Each(a => a.Error(message));
		}

        public void Error(string message, params object[] args)
        {
            Error(string.Format(message ,args));
        }

		#endregion

		private void Each(Action<IAnnouncer> predicate)
		{
			foreach (var announcer in _announcers)
				predicate(announcer);
		}
	}
}