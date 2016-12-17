using System;
using System.Reflection;

namespace FluentMigrator.Runner.Announcers.Log4Net
{
    public class Log4NetAnnouncer : Announcer
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
            : base()
        {
            _log = log;
        }

        public override void Emphasize(string message)
        {
            _log.Warn(string.Format("[+] {0}", message));
        }

        public override void Error(string message)
        {
            _log.ErrorFormat("!!! {0}", message);
        }

        public override void Heading(string message)
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

        public override void Write(string message, bool escaped)
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