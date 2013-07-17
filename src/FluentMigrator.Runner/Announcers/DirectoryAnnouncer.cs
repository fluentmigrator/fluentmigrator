using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FluentMigrator.Runner.Announcers
{
    public class DirectoryAnnouncer : TextWriterAnnouncer
    {
        private string directory = null;
        private TextWriter tw = null;
        public DirectoryAnnouncer(string directory) 
            : base(s => DirectoryOutput(directory, s))
        {
            this.directory = directory;
            var defaultOutputFile = OutputFileName(directory, null);
            if (File.Exists(defaultOutputFile)) File.Delete(defaultOutputFile);
        }

        public override void StartMigration(long version)
        {
            this.Output = (tw = GetNewWriter(version)).Write;
            base.StartMigration(version);
        }

        private TextWriter GetNewWriter(long version)
        {
            var file = OutputFileName(directory, string.Format("{0}_FluentMigrator.sql", version));
            if (File.Exists(file)) File.Delete(file);
            return new StreamWriter(file);
        }

        public override void EndMigration()
        {
            CloseWriter();
            this.Output = s => DirectoryOutput(directory, s);
            base.EndMigration();
        }

        private void CloseWriter()
        {
            tw.Close();
            tw.Dispose();
            tw = null;
        }

        private static void DirectoryOutput(string directory, string message)
        {
            File.AppendAllText(OutputFileName(directory, null), message);
        }

        private static string OutputFileName(string directory, string file)
        {
            var dir = new System.IO.DirectoryInfo(directory);
            var normDir = dir.Parent.FullName + @"\" + dir.Name;
            return normDir + @"\" + (file ?? "output.txt");
        }
    }
}
