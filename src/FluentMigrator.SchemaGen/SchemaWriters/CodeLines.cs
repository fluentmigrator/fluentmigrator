using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.SchemaGen.SchemaWriters
{
    public class CodeLines : List<string>
    {
        private int indent = 0;

        public CodeLines()
        {
        }

        public CodeLines(string line)
        {
            AddLine(line);
        }

        public CodeLines(string format, params object[] args)
        {
            AddLine(string.Format(format, args));
        }

        public void Indent(int by = 1)
        {
            indent += by;
        }

        private string[] SplitCodeLines(string codeText)
        {
            // Need to split into lines so indenting works 
            return codeText.Trim().Replace(Environment.NewLine, "\n").Split('\n');
        }

        private void AddLine(string line)
        {
            Add(new string(' ', indent * 4) + line);
        }

        public void WriteLine()
        {
            Add("");
        }

        public void WriteLine(string line)
        {
            AddLine(line);
        }

        public void WriteSplitLine(string line)
        {
            WriteLines(SplitCodeLines(line.Trim()));
        }

        public void WriteSplitLines(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                WriteLine();
                WriteSplitLine(line);
            }
        }

        public void WriteLine(string format, params object[] args)
        {
            AddLine(string.Format(format, args));
        }

        public void WriteLines(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                WriteLine(line);
            }
        }

        public void WriteLines(IEnumerable<string> lines, string appendLastLine)
        {
            var lineArr = lines.ToArray();
            for (int i = 0; i < lineArr.Length; i++)
            {
                if (i < lineArr.Length - 1)
                {
                    WriteLine(lineArr[i]);
                }
                else
                {
                    WriteLine(lineArr[i] + appendLastLine);
                }
            }
        }

        public void WriteComment(string comment)
        {
            // Split to ensure that lines indent correctly
            WriteLines(SplitCodeLines(comment.Trim()).Select(line => "// " + line));
        }

        public void WriteComments(IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                WriteLine("// " + line);
            }
        }
    }
}