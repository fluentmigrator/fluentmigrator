using System;

namespace FluentMigrator.Infrastructure {
    public interface IGate {
        DateTime? Start { get; }
        DateTime? End { get; }
        bool IsOpen { get; }
        void SetGate(DateTime? start, DateTime? end);
    }
}