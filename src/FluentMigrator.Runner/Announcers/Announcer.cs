#region License

// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

#endregion

using System;
using System.IO;

namespace FluentMigrator.Runner.Announcers
{
	public class Announcer : IAnnouncer
	{
		private readonly TextWriter _writer;

		public Announcer(TextWriter writer)
		{
			_writer = writer;
		}

		#region IAnnouncer Members

		public void Announce(string message)
		{
			_writer.Write("-- " + message + " ");
			for (var i = 0; i < 75 - (message.Length + 1); i++)
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
			_writer.Write("-- -> ");
			_writer.Write(message);
			_writer.Write(Environment.NewLine);
		}

		public void SaySql(string sql)
		{
			_writer.Write(sql);
			_writer.Write(Environment.NewLine);
		}

		public void Dispose()
		{
		}

		#endregion
	}
}