using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FluentMigrator.Runner.Announcers
{
    public class TextWriterWithGoAnnouncer : TextWriterAnnouncer
    {
        public TextWriterWithGoAnnouncer(TextWriter writer)
            : base(writer)
        { }

        public  TextWriterWithGoAnnouncer(Action<string> write) 
            : base(write)
        { }

        public override void Sql(string sql)
        {
            if (!ShowSql) return;

            base.Sql(sql);

            if (!string.IsNullOrEmpty(sql))
                Write("GO", false);
        } 
    }
}
