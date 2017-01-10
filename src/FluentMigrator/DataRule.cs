using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator
{
    public enum DataRule
    {
        None = 0,
        Cascade = 1,
        SetNull = 2,
        SetDefault = 3
    }
}
