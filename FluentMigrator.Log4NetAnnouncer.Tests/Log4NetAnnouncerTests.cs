using Moq;
using NUnit.Framework;
using Shouldly;
using System.Text;
using l4n = log4net;
using l4nAnn = FluentMigrator.Runner.Announcers.Log4Net;

namespace FluentMigrator.Log4NetAnnouncer.Tests
{
    [TestFixture]
    public class Log4NetAnnouncerTests
    {
        [Test]
        public void Instance_for_type_creates_log_for_that_type()
        {
            l4nAnn.Log4NetAnnouncer a = new l4nAnn.Log4NetAnnouncer(typeof(System.Threading.Thread));
            l4n.ILog log = ReflectLogFromAnnouncer(a);
            log.Logger.Name.ShouldBe("System.Threading.Thread");
        }

        [Test]
        public void Instance_for_name_creates_log_with_that_name()
        {
            l4nAnn.Log4NetAnnouncer a = new l4nAnn.Log4NetAnnouncer("TestLogger");
            l4n.ILog log = ReflectLogFromAnnouncer(a);
            log.Logger.Name.ShouldBe("TestLogger");
        }

        [Test]
        public void Instance_for_log_uses_that_log()
        {
            l4n.ILog log = l4n.LogManager.GetLogger("SomeCustomLogger");
            l4nAnn.Log4NetAnnouncer a = new l4nAnn.Log4NetAnnouncer(log);
            l4n.ILog actualLogger = ReflectLogFromAnnouncer(a);
            actualLogger.ShouldBeSameAs(log);
            actualLogger.Logger.Name.ShouldBe(log.Logger.Name);
        }

        [Test]
        public void Emphasize_uses_warn_level()
        {
            string logMessage = "This is more important than other messages.";
            string expectedLogMessage = "[+] " + logMessage;
            var logMock = new Mock<l4n.ILog>();
            logMock.Setup(l => l.Warn(It.IsAny<string>())).Verifiable();
            l4nAnn.Log4NetAnnouncer a = new l4nAnn.Log4NetAnnouncer(logMock.Object);
            a.Emphasize(logMessage);
            logMock.Verify(l => l.Warn(It.Is<string>(v => v == expectedLogMessage)), Times.Once());
        }

        [Test]
        public void Error_uses_error_level()
        {
            string logMessage = "This is an error messages.";
            string errorFormat = "!!! {0}";
            var logMock = new Mock<l4n.ILog>();
            logMock.Setup(l => l.ErrorFormat(errorFormat, It.IsAny<string>())).Verifiable();
            l4nAnn.Log4NetAnnouncer a = new l4nAnn.Log4NetAnnouncer(logMock.Object);
            a.Error(logMessage);
            logMock.Verify(l => l.ErrorFormat(errorFormat, It.Is<string>(v => v == logMessage)), Times.Once());
        }

        [Test]
        public void Write_uses_info_level()
        {
            string logMessage = "This is a log message.";
            var logMock = new Mock<l4n.ILog>();
            logMock.Setup(l => l.Info(It.IsAny<string>())).Verifiable();
            l4nAnn.Log4NetAnnouncer a = new l4nAnn.Log4NetAnnouncer(logMock.Object);
            a.Write(logMessage);
            logMock.Verify(l => l.Info(It.Is<string>(v => v == logMessage)), Times.Once());
        }

        [Test]
        public void HorizontalRule_writes_79_dashes()
        {
            StringBuilder ruler = new StringBuilder();
            for (int i = 0; i < 79; i++)
                ruler.Append("-");
            var logMock = new Mock<l4n.ILog>();
            logMock.Setup(l => l.Info(It.IsAny<string>())).Verifiable();
            l4nAnn.Log4NetAnnouncer a = new l4nAnn.Log4NetAnnouncer(logMock.Object);
            a.HorizontalRule();
            logMock.Verify(l => l.Info(It.Is<string>(v => v == ruler.ToString())), Times.Once());
        }

        [Test]
        public void Heading_writes_a_banner_then_the_message()
        {
            string headerMessage = "This should be in the header.";
            var logMock = new Mock<l4n.ILog>();
            logMock.Setup(l => l.Info(It.IsAny<string>())).Verifiable();
            l4nAnn.Log4NetAnnouncer announcer = new l4nAnn.Log4NetAnnouncer(logMock.Object);
            announcer.Heading(headerMessage);
            logMock.Verify(l => l.Info(It.Is<string>(v => v == headerMessage)), Times.Once());
            logMock.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeast(5));
        }

        private l4n.ILog ReflectLogFromAnnouncer(l4nAnn.Log4NetAnnouncer announcer)
        {
            var logMemberInfo = announcer.GetType().GetField("_log", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            l4n.ILog log = (l4n.ILog)logMemberInfo.GetValue(announcer);
            return log;
        }
    }
}