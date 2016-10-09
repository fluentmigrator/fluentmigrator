using FluentMigrator.Runner.Announcers;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FluentMigrator.Tests.Unit.Announcers
{
    public class TextWriterWithGoAnnouncerTests
    {
        private StringWriter _stringWriter;
        private TextWriterWithGoAnnouncer announcer;

        public void TestSetup()
        {
            _stringWriter = new StringWriter();
            announcer = new TextWriterWithGoAnnouncer(_stringWriter)
            {
                ShowElapsedTime = true,
                ShowSql = true
            };
        }

        [Fact]
        public void Adds_Go_StatementAfterSqlAnouncement()
        {
            announcer.Sql("DELETE Blah");
            Output.ShouldBe("DELETE Blah" + Environment.NewLine + 
                "GO" + Environment.NewLine);
        }

        [Fact]
        public void Sql_Should_Not_Write_When_Show_Sql_Is_False()
        {
            announcer.ShowSql = false;

            announcer.Sql("SQL");
            Output.ShouldBe(String.Empty);
        }

        [Fact]
        public void Sql_Should_Not_Write_Go_When_Sql_Is_Empty()
        {
            announcer.Sql("");
            Assert.IsFalse(Output.Contains("GO"));
        }

        public string Output
        {
            get { return _stringWriter.GetStringBuilder().ToString(); }
        }
    }
}
