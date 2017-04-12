using System;

namespace FluentMigrator.Infrastructure {
    public interface IGate {
        DateTime? Start { get; }
        DateTime? End { get; }
    }
}