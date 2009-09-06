using System;
using System.Text;

namespace FluentMigrator.Runner
{
	public interface IStopWatch
	{
		void Start();
		void Stop();
		TimeSpan ElapsedTime();
	}

	public class StopWatch : IStopWatch
	{
		public static Func<DateTime> TimeNow = () => DateTime.Now;

		private DateTime _startTime;
		private DateTime _endTime;

		public void Start()
		{
			_startTime = TimeNow();
		}

		public void Stop()
		{
			_endTime = TimeNow();
		}

		public TimeSpan ElapsedTime()
		{
			return _endTime - _startTime;
		}
	}
}
