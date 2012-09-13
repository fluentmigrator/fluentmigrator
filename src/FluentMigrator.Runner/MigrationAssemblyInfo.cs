using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FluentMigrator.Runner
{
    public class MigrationAssemblyInfo
    {
        public Assembly Assembly { get;set; }
        public string @Namespace { get; set; }
    }
}
