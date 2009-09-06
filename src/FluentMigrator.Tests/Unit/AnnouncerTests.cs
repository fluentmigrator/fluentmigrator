using System;
using System.IO;
using FluentMigrator.Runner;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
	[TestFixture]
	public class AnnouncerTests
	{
		private Announcer _announcer;
		private StringWriter _stringWriter;

		[SetUp]
		public void SetUp()
		{
			_stringWriter = new StringWriter();
			_announcer = new Announcer(_stringWriter);
		}

		public string Output
		{
			get
			{
				return _stringWriter.GetStringBuilder().ToString();
			}
		}

		[Test]
		public void CanAnnounceAndPadWithEquals()
		{
			_announcer.Announce("Test");
			Output.ShouldBe("==  Test ======================================================================" + Environment.NewLine);
		}

		[Test]
		public void CanSay()
		{
			_announcer.Say("Create table");
			Output.ShouldBe("-- Create table" + Environment.NewLine);
		}

		[Test]
		public void CanSaySubItem()
		{
			_announcer.SaySubItem("0.0512s");
			Output.ShouldBe("   -> 0.0512s" + Environment.NewLine);
		}
	}
}
