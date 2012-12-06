using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FluentMigrator.T4
{
    public class DelegateTextWriter : TextWriter
    {
        private readonly Action<string> writeLine;

        public DelegateTextWriter(Action<string> writeLine)
        {
            this.writeLine = writeLine;
        }

        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        public override void WriteLine(string value)
        {
            writeLine(value);
        }

    }
}
