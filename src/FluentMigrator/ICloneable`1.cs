using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator
{
    public interface ICloneable<T>
    {
        T Clone();
    }
}
