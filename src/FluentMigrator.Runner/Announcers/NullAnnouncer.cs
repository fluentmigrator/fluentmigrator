using System;

namespace FluentMigrator.Runner.Announcers
{
    public class NullAnnouncer : IAnnouncer, IFormattingAnnouncer
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

        public void Heading(string message, params object[] args)
        {
        }

        public void Say(string message, params object[] args)
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

        public void Error(string message, params object[] args)
        {
        }

        #endregion
    }
}