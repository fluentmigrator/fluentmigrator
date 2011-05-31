using System;

namespace FluentMigrator.Runner.Announcers
{
    public class DebugAnnouncer: IAnnouncer, IFormattingAnnouncer
    {
        public DebugAnnouncer()
        {
            NonSqlPrefix = "-- ";
        }

        public string NonSqlPrefix { get; set; }
        public bool ShowSql { get; set; }
        public bool ShowElapsedTime { get; set; }

        private void Write(string message)
        {
            Console.WriteLine(message);
        }

        public void Heading(string message)
        {
            Write(NonSqlPrefix + message + " ");
            for (var i = 0; i < 75 - (message.Length + 1); i++)
            {
                Write("=");
            }
            Write("");
            Write("");
        }

        public void Say(string message)
        {
            Write(NonSqlPrefix + message);
        }

        public void Sql(string sql)
        {
            if (!ShowSql)
                return;

            if (!string.IsNullOrEmpty(sql))
                Write(sql);
            else
                Say("No SQL statement executed.");
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
            if (!ShowElapsedTime)
                return;

            Say(string.Format("-> {0}s", timeSpan.TotalSeconds));
            Write("");
        }

        public void Error(string message)
        {
            Write(NonSqlPrefix + "ERROR: ");
            Write(message);
            Write("");
        }

        public void Heading(string message, params object[] args)
        {
            Heading(string.Format(message, args));
        }

        public void Say(string message, params object[] args)
        {
            Say(string.Format(message, args));
        }

        public void Error(string message, params object[] args)
        {
            Error(string.Format(message, args));
        }

        public void Dispose()
        {
            
        }
    }
}