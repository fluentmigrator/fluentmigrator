using System;
using FluentMigrator.Runner;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
	[TestFixture]
	public class StopWatchTests
	{
		[Test]
		public void CanGetTheElapsedTime()
		{
			var watch = new StopWatch();
			
			StopWatch.TimeNow = () => new DateTime(2009, 9, 6, 15, 53, 0, 0);

			watch.Start();
			
			StopWatch.TimeNow = () => new DateTime(2009, 9, 6, 15, 53, 0, 5);
			
			watch.Stop();

			watch.ElapsedTime().Milliseconds.ShouldBe(5);
		}
	}
}
