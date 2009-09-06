using System;
using System.IO;

namespace FluentMigrator.Runner
{
	public interface IAnnouncer
	{
		void Announce(string message);
		void Say(string message);
		void SaySubItem(string message);
	}

	public class Announcer : IAnnouncer
	{
		private TextWriter _writer;

		public Announcer(TextWriter writer)
		{
			_writer = writer;
		}

		public void Announce(string message)
		{
			_writer.Write("==  " + message + " ");
			for (int i = 0; i < 75 - (message.Length + 1); i++)
			{
				_writer.Write("=");
			}
			_writer.Write(Environment.NewLine);
		}

		public void Say(string message)
		{
			_writer.Write("-- ");
			_writer.Write(message);
			_writer.Write(Environment.NewLine);
		}

		public void SaySubItem(string message)
		{
			_writer.Write("   -> ");
			_writer.Write(message);
			_writer.Write(Environment.NewLine);
		}
	}
}
