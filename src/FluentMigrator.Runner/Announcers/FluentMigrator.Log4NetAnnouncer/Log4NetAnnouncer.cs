using System;
using System.Reflection;

namespace FluentMigrator.Runner.Announcers.Log4Net
{
    public class Log4NetAnnouncer : IAnnouncer
    {
        private readonly log4net.ILog _log;

        public Log4NetAnnouncer()
            : this(MethodBase.GetCurrentMethod().DeclaringType)
        { }

        public Log4NetAnnouncer(Type loggerType)
            : this(log4net.LogManager.GetLogger(loggerType))
        { }

        public Log4NetAnnouncer(string loggerName)
            : this(log4net.LogManager.GetLogger(loggerName))
        { }

        public Log4NetAnnouncer(string repository, Type loggerType)
            : this(log4net.LogManager.GetLogger(repository, loggerType))
        { }

        public Log4NetAnnouncer(string repository, string loggerName)
            : this(log4net.LogManager.GetLogger(repository, loggerName))
        { }

        public Log4NetAnnouncer(Assembly repositoryAssembly, Type loggerType)
            : this(log4net.LogManager.GetLogger(repositoryAssembly, loggerType))
        { }

        public Log4NetAnnouncer(Assembly repositoryAssembly, string loggerName)
            : this(log4net.LogManager.GetLogger(repositoryAssembly, loggerName))
        { }

        public Log4NetAnnouncer(log4net.ILog log)
        {
            _log = log;
        }

        public void ElapsedTime(TimeSpan timeSpan)
        {
            Write(string.Format("=> {0}s", timeSpan.TotalSeconds), true);
        }

        public void Emphasize(string message)
        {
            Say(string.Format("[+] {0}", message));
        }

        public void Error(Exception exception)
        {
            while (exception != null)
            {
                Error(exception.Message);
                exception = exception.InnerException;
            }
        }

        public void Error(string message)
        {
            _log.ErrorFormat("!!! {0}", message);
        }

        public void Heading(string message)
        {
            HorizontalRule();
            Write("=============================== FluentMigrator ================================");
            HorizontalRule();
            Write("Source Code:");
            Write("  http://github.com/schambers/fluentmigrator");
            Write("Ask For Help:");
            Write("  http://groups.google.com/group/fluentmigrator-google-group");
            HorizontalRule();
        }

        public void Say(string message)
        {
            Write(message);
        }

        public void Sql(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                Write("No SQL statement executed.", true);
            else
                Write(sql, false);
        }

        public void Write(string message, bool escaped)
        {
            _log.Info(message);
        }

        public void HorizontalRule()
        {
            Write("".PadRight(79, '-'));
        }

        public void Write(string message)
        {
            Write(message, true);
        }
    }
}