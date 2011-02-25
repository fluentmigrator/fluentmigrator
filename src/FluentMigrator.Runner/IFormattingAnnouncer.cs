using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Runner
{
    public interface IFormattingAnnouncer
    {
        void Heading(string message, params object[] args);
        void Say(string message, params object[] args);
        void Error(string message, params object[] args);
    }
}
